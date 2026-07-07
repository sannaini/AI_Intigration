using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Interfaces.AI
{
    public interface IDocumentAiService
    {
        Task<string> GetAnswerAsync(string question, IEnumerable<string> contextChunks);
    }
}
