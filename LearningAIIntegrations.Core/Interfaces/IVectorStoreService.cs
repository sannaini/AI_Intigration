using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    // ── Contract for storing and searching vectors in Qdrant ──────
    //
    // Think of this like a repository pattern you already know:
    //
    //   SQL Repository:          Vector Store:
    //   ─────────────────        ──────────────────────
    //   SaveAsync(entity)   →    StoreChunksAsync(chunks, vectors)
    //   SearchAsync(query)  →    SearchAsync(questionVector, topK)
    //
    // Implementation lives in: Infrastructure/Services/VectorStoreService.cs
    // It talks to Qdrant running at http://localhost:6333
    public interface IVectorStoreService
    {
        // ── PHASE 1: Store ────────────────────────────────────────
        // Save document chunks + their vectors into Qdrant
        //
        // WHY two separate lists?
        // → chunks carry the text + metadata (what to return when found)
        // → vectors carry the float[] (what to search against)
        // → they are parallel lists: chunks[0] matches vectors[0] etc.
        //
        // Example:
        //   chunks:  [chunk0, chunk1, chunk2]
        //   vectors: [[0.23,...], [0.45,...], [0.67,...]]
        //   → Qdrant stores 3 points, each with text + vector
        Task StoreChunksAsync<TPayload>(string collectionName, List<VectorRecord<TPayload>> records);

        // ── PHASE 2: Search ───────────────────────────────────────
        // Find the most similar chunks to the question vector
        //
        // HOW similarity works (cosine similarity):
        //   questionVector:  [0.24, 0.85, 0.11, ...]
        //   chunk0 vector:   [0.23, 0.87, 0.12, ...]  → score: 0.97 ✅ close!
        //   chunk1 vector:   [0.45, 0.11, 0.93, ...]  → score: 0.21 ❌ far
        //   chunk2 vector:   [0.67, 0.34, 0.55, ...]  → score: 0.43
        //
        // Returns TOP K chunks sorted by similarity score
        //
        // Example:
        //   Input:  [0.24, 0.85, 0.11, ...], topK: 3
        //   Output: [chunk0, chunk4, chunk7]  ← 3 most relevant chunks
        Task<List<VectorRecord<TPayload>>> SearchAsync<TPayload>(string collectionName ,float[] questionVector, int topK);

        // ── UTILITY: Create Collection ────────────────────────────
        // Qdrant organizes data into "collections" (like SQL tables)
        // We need to create one before storing anything
        // Called once during app startup or first upload
        //
        // vectorSize: must match embedding model output
        //   nomic-embed-text → 768 dimensions
        //   So vectorSize = 768 always for our project
        Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize = 768);
    }
}