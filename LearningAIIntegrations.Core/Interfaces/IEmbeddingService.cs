using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Interfaces
{
    // ── Contract for converting text into vectors (embeddings) ────
    //
    // WHY embeddings?
    // → Computers can't compare meaning of words directly
    // → But they CAN compare numbers
    // → Similar meaning = similar numbers = close in vector space
    //
    // Example:
    //   "refund policy"    → [0.23, 0.87, 0.12, ...]
    //   "return policy"    → [0.24, 0.85, 0.13, ...]  ← very close! similar meaning
    //   "cooking recipes"  → [0.91, 0.11, 0.67, ...]  ← very different
    //
    // Implementation lives in: Infrastructure/Services/EmbeddingService.cs
    // It calls Ollama's nomic-embed-text model at http://localhost:11434
    public interface IEmbeddingService
    {
        // Embed a SINGLE text
        // Used when: converting user's question to a vector for searching
        //
        // Example:
        //   Input:  "What is the refund period?"
        //   Output: [0.24, 0.85, 0.11, 0.78, ...] (768 numbers)
        Task<float[]> EmbedAsync(string text);

        // Embed MULTIPLE texts in one batch
        // Used when: indexing all chunks of an uploaded document
        //
        // WHY batch? Efficiency:
        //   ❌ One by one: 50 chunks = 50 separate API calls to Ollama
        //   ✅ Batch:      50 chunks = 1 API call to Ollama
        //
        // Example:
        //   Input:  ["chunk0 text", "chunk1 text", "chunk2 text"]
        //   Output: [[0.23, ...],   [0.45, ...],   [0.67, ...]]
        Task<List<float[]>> EmbedBatchAsync(List<string> texts);
    }
}
