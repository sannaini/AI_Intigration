namespace LearningAIIntegrations.Core.Models
{
    public class DocumentChunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Text { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }
    }
}
