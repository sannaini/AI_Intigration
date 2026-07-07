using System;
using System.Collections.Generic;
using System.Text;
using LearningAIIntegrations.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    public interface IChatHistoryService
    {
        List<ChatMessageEffectiveDating> GetHistory(string sessionId);
        void AddMessage(string sessionId, string role, string content);
        void ClearHistory(string sessionId);
    }
}
