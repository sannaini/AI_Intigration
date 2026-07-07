using LearningAIIntegrations.Core.Interfaces.AI;
using LearningAIIntegrations.Core.Models;
using LearningAIIntegrations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using ChatRequest = OllamaSharp.Models.Chat.ChatRequest;

namespace LearningAIIntegrations.Infrastructure.Services.AI;

public class OllamaChatService(IOllamaApiClient client, ILogger<OllamaChatService> logger, 
    IOptions<AppSettings> appSettings) : IChatAiService
{
    public async Task<string> AskQuestionAsync(string question)
    {
        try
        {
            logger.LogInformation("Sending question to Ollama: {question}", question);


            var messages = new List<Message>
            {
                new Message { Role = ChatRole.User, Content = question }
            };

            var chatRequest = new ChatRequest
            {
                Model = appSettings.Value.Ollama.Model,
                Messages = messages,
                Stream = false  
            };

            // Collect the full response
            var answerBuilder = new System.Text.StringBuilder();

            await foreach (var chunk in client.ChatAsync(chatRequest))
            {
                if (chunk?.Message?.Content != null)
                    answerBuilder.Append(chunk.Message.Content);
            }

            var answer = answerBuilder.ToString().Trim();


            // 2. Extract the reply text
            var replyText = answer ?? "No response from ollama";

            // 3. Token counts
            var inputTokens =  0;
            var outputTokens =  0;

            logger.LogInformation(
                "Ollama replied. Tokens — input: {In}, output: {Out}",
                inputTokens, outputTokens);

            return replyText;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
      
    }

    public async IAsyncEnumerable<string> AskQuestionAsync(string question, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending question to Ollama: {question}", question);

        var messages = new List<Message>
        {
            new Message { Role = ChatRole.User, Content = question }
        };

        var chatRequest = new ChatRequest
        {
            Model = appSettings.Value.Ollama.Model,
            Messages = messages,
            Stream = true
        };

        await foreach (var chunk in client.ChatAsync(chatRequest, cancellationToken))
        {
            yield return chunk?.Message?.Content ?? string.Empty;
        }
        logger.LogInformation("Streaming complete for: {Message}", question);
    }

    public async Task<ChatResponse> ChatWithHistoryAsync(List<ChatMessageEffectiveDating> history, string userMessage)
    {
        logger.LogInformation(
            "Sending message to Gemini with {Count} previous messages",
            history.Count);


        var historyMessages  = history.Select(msg => new Message { Role = ChatRole.System, Content = msg.Content }).ToList();


        var uMessage = new Message()
        {
            Role = ChatRole.User,
            Content = userMessage
        };


        var chatRequest = new ChatRequest
        {
            Model = appSettings.Value.Ollama.Model,
            Messages = historyMessages.Concat(new[] { uMessage }).ToList(),
            Stream = true
        };


        // Collect the full response
        var answerBuilder = new System.Text.StringBuilder();


        await foreach (var chunk in client.ChatAsync(chatRequest))
        {
            if (chunk?.Message?.Content != null)
                answerBuilder.Append(chunk.Message.Content);
        }

        var answer = answerBuilder.ToString().Trim();


        var replyText = answer ?? "No response from Ollama";

        logger.LogInformation("Ollama replied with history context. Reply: {Reply}", replyText);

        return new ChatResponse
        {
            Reply = replyText,
            InputTokens =  0,
            OutputTokens =0
        };
    }


}
