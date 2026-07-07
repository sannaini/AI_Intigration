using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Models;

namespace LearningAIIntegrations.Infrastructure.Services
{
    // ── Implementation ───────────────────────────────────────────
    // ConcurrentDictionary is like a regular Dictionary but THREAD-SAFE
    // meaning multiple users can read/write at the same time without errors
    // Key   = sessionId (e.g. "user-123")
    // Value = list of messages in that session
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly Dictionary<string, List<ChatMessageEffectiveDating>> _sessions = new();

        // Returns the full message history for a session
        // If session doesn't exist yet, returns an empty list
        public List<ChatMessageEffectiveDating> GetHistory(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var history))
                return history;

            // First time we see this sessionId — create an empty history for it
            _sessions[sessionId] = new List<ChatMessageEffectiveDating>();
            return _sessions[sessionId];
        }

        // Adds a single message to the session history
        // Called twice per turn: once for user message, once for AI reply
        public void AddMessage(string sessionId, string role, string content)
        {
            var history = GetHistory(sessionId); // creates session if needed

            history.Add(new ChatMessageEffectiveDating
            {
                Role = role,        // "user" or "model"
                Content = content,
                Timestamp = DateTime.UtcNow
            });
        }

        // Clears all messages for a session — useful for "Start New Chat"
        public void ClearHistory(string sessionId)
        {
            if (_sessions.ContainsKey(sessionId))
                _sessions[sessionId].Clear();
        }
    }
}
