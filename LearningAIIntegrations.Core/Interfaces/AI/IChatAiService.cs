using LearningAIIntegrations.Models;

namespace LearningAIIntegrations.Core.Interfaces.AI
{
    /// <summary>
    /// Defines the contract for a chat AI service that can handle user questions and maintain chat history.
    /// </summary>
    public interface IChatAiService
    {
        Task<string> AskQuestionAsync(string question);

        IAsyncEnumerable<string> AskQuestionAsync(string question, CancellationToken cancellationToken);

        Task<ChatResponse> ChatWithHistoryAsync(List<ChatMessageEffectiveDating> history, string userMessage);
    }
}
