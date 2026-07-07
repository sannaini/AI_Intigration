using System;
using System.Collections.Generic;
using System.Text;
using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces.AI
{
    public interface IMortgageToolAIService
    {
        Task<MortgageAIResponse> AskAsync(string question);
    }
}
