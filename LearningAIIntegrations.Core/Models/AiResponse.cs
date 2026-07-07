namespace LearningAIIntegrations.Core.Models
{
    public class AiResponse
    {
        public string Answer { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
