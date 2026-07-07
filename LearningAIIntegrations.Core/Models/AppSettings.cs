namespace LearningAIIntegrations.Core.Models
{
    public class AppSettings
    {
        public OllamaSettings Ollama { get; set; } = new();
        public QdrantSettings Qdrant { get; set; } = new();
    }

    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    public class QdrantSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
