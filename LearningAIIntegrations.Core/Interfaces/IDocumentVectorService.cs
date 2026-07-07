using System;
using System.Collections.Generic;
using System.Text;
using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    public interface IDocumentVectorService
    {
        Task StoreDocumentsAsync(List<VectorRecord<DocumentChunk>> chunks);
        Task<List<VectorRecord<DocumentChunk>>> SearchDocumentsAsync(float[] queryVector, int topK);
    }
}
