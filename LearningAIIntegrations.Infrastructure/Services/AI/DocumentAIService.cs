using System.Text;
using LearningAIIntegrations.Core.Interfaces.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LearningAIIntegrations.Infrastructure.Services.AI
{
    public class DocumentAIService : IDocumentAiService
    {
        private readonly IOllamaApiClient _ollamaClient;
        private readonly ILogger<DocumentAIService> _logger;

        public DocumentAIService(
            IOllamaApiClient ollamaClient,
            ILogger<DocumentAIService> logger)
        {
            _ollamaClient = ollamaClient;
            _logger = logger;
        }

        public async Task<string> GetAnswerAsync(string question, IEnumerable<string> contextChunks)
        {
            var context = string.Join("\n\n", contextChunks);

            var prompt = $"""
                          You are a helpful assistant. Answer using ONLY the context below.
                          If the answer is not present, say you cannot find it.

                          CONTEXT:
                          {context}

                          QUESTION:
                          {question}

                          ANSWER:
                          """;

            var messages = new List<Message>
            {
                new Message { Role = ChatRole.User, Content = prompt }
            };

            var request = new ChatRequest
            {
                Messages = messages,
                Stream = false
            };

            var sb = new StringBuilder();

            await foreach (var chunk in _ollamaClient.ChatAsync(request))
            {
                if (!string.IsNullOrEmpty(chunk?.Message?.Content))
                    sb.Append(chunk.Message.Content);
            }

            var answer = sb.ToString().Trim();

            _logger.LogInformation("AI response generated. Length={Len}", answer.Length);

            return answer;
        }
    }
}