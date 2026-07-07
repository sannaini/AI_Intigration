using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Models
{
    public class DocumentUploadResponse
    {
        // How many chunks the document was split into
        public int TotalChunks { get; set; }

        // The filename that was indexed
        public string FileName { get; set; } = string.Empty;

        // Confirmation message
        public string Message { get; set; } = string.Empty;
    }
}
