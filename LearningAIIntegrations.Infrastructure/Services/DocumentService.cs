using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Models;
using Microsoft.Extensions.Logging;

namespace LearningAIIntegrations.Infrastructure.Services
{
    // ── Splits raw text into overlapping chunks ───────────────────
    // This is pure string manipulation — no API calls, no async needed
    public class DocumentService : IDocumentService
    {
        private readonly ILogger<DocumentService> _logger;

        // How many words per chunk
        // 150 words ≈ ~200 tokens — well within nomic-embed-text's 512 token limit
        private const int ChunkSize = 150;

        // How many words to overlap between consecutive chunks
        // 20 words of overlap preserves context at chunk boundaries
        private const int OverlapSize = 20;

        public DocumentService(ILogger<DocumentService> logger)
        {
            _logger = logger;
        }

        public List<DocumentChunk> SplitIntoChunks(string text, string fileName)
        {
            _logger.LogInformation(
                "Splitting document '{FileName}' into chunks", fileName);

            var chunks = new List<DocumentChunk>();

            // 1. Clean the text — normalize whitespace
            //    Multiple spaces/newlines → single space
            var cleanText = string.Join(" ",
                text.Split(new[] { ' ', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries));

            // 2. Split into individual words
            //    "Hello world foo" → ["Hello", "world", "foo"]
            var words = cleanText.Split(' ');

            if (words.Length == 0)
            {
                _logger.LogWarning("Document '{FileName}' has no content", fileName);
                return chunks;
            }

            // 3. Slide through words with overlap
            //    ChunkSize = 150 words per chunk
            //    OverlapSize = 20 words shared between chunks
            //    Step = 150 - 20 = 130 words forward each iteration
            var step = ChunkSize - OverlapSize;
            var chunkIndex = 0;

            for (int i = 0; i < words.Length; i += step)
            {
                // Take up to ChunkSize words starting at position i
                // Math.Min prevents going past the end of the array
                var chunkWords = words
                    .Skip(i)
                    .Take(ChunkSize)
                    .ToArray();

                // Join words back into a readable sentence
                var chunkText = string.Join(" ", chunkWords);

                chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    Text = chunkText,
                    FileName = fileName,
                    ChunkIndex = chunkIndex
                });

                chunkIndex++;

                // If this chunk reached the end of the document, stop
                if (i + ChunkSize >= words.Length)
                    break;
            }

            _logger.LogInformation(
                "Split '{FileName}' into {Count} chunks " +
                "(ChunkSize: {ChunkSize} words, Overlap: {Overlap} words)",
                fileName, chunks.Count, ChunkSize, OverlapSize);

            return chunks;
        }
    }
}