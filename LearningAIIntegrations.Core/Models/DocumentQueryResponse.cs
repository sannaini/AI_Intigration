using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Models
{
    public class DocumentQueryResponse
    {
        // The AI's answer based on document context
        public string Answer { get; set; } = string.Empty;

        // Which chunks were used to answer — great for transparency
        // User can see EXACTLY which part of the doc was referenced
        public List<string> RelevantChunks { get; set; } = new();

        // The question that was asked (useful for frontend display)
        public string Question { get; set; } = string.Empty;
    }
}
