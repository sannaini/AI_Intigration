using LearningAIIntegrations.Core.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LearningAIIntegrations.Infrastructure.Services
{
    // ── Converts text into vectors using Ollama + nomic-embed-text ─
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
        private readonly ILogger<EmbeddingService> _logger;

        // OllamaApiClient implements IEmbeddingGenerator out of the box
        // We just cast it — no extra setup needed
        public EmbeddingService(
            IEmbeddingGenerator<string, Embedding<float>> generator,
            ILogger<EmbeddingService> logger)
        {
            _generator = generator;
            _logger = logger;
        }

        // ── Embed a single text ───────────────────────────────────
        // Used when: converting user question to vector for searching
        public async Task<float[]> EmbedAsync(string text)
        {
            _logger.LogInformation(
                "Generating embedding for text: '{Text}'",
                text.Length > 50 ? text[..50] + "..." : text);

            // GenerateAsync returns a list of embeddings
            // We only pass one text so we take the first result
            var result = await _generator.GenerateAsync(new[] { text });

            // Embedding<float>.Vector is a ReadOnlyMemory<float>
            // .ToArray() converts it to float[] which we work with
            return result[0].Vector.ToArray();
        }

        // ── Embed multiple texts in one batch ─────────────────────
        // Used when: indexing all chunks of an uploaded document
        // Much more efficient than calling EmbedAsync one by one
        public async Task<List<float[]>> EmbedBatchAsync(List<string> texts)
        {
            _logger.LogInformation(
                "Generating embeddings for {Count} texts", texts.Count);

            // Pass all texts at once — Ollama processes them together
            var results = await _generator.GenerateAsync(texts);

            // Convert each Embedding<float> to float[]
            return results
                .Select(e => e.Vector.ToArray())
                .ToList();
        }
    }
}