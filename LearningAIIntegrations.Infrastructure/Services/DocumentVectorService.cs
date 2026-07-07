using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Infrastructure.Services
{
    public class DocumentVectorService(IVectorStoreService baseStore) : IDocumentVectorService
    {
        private const string CollectionKey = "documents";

        public Task StoreDocumentsAsync(List<VectorRecord<DocumentChunk>> chunks)
        {
            return baseStore.StoreChunksAsync(CollectionKey, chunks );
        }

        public Task<List<VectorRecord<DocumentChunk>>> SearchDocumentsAsync(float[] queryVector, int topK)
        {
            return baseStore.SearchAsync<DocumentChunk>(CollectionKey, queryVector, topK);
        }
    }
}
