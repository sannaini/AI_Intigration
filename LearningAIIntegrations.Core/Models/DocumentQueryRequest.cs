using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Models
{
    public class DocumentQueryRequest
    {
        // The user's question
        // e.g. "What is the refund period?"
        public string Question { get; set; } = string.Empty;

        // How many relevant chunks to retrieve from Qdrant
        // 3 is a good default — enough context, not too noisy
        public int TopChunks { get; set; } = 3;
    }
}
