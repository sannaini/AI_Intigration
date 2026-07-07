using System;
using System.Collections.Generic;
using System.Text;
using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    public interface IDocumentService
    {
        // Takes the raw text content + filename
        // Returns a list of DocumentChunk objects ready for embedding
        //
        // Example:
        //   Input:  "10 page document text...", "policy.txt"
        //   Output: [chunk0, chunk1, chunk2, ... chunk49]
        List<DocumentChunk> SplitIntoChunks(string text, string fileName);
    }
}
