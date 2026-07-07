namespace LearningAIIntegrations.Models
{
    // ── Module 1 & 2 — unchanged ────────────────────────────────
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

    // ── Module 3 — Chat History ──────────────────────────────────

    // What the frontend sends us
    // SessionId ties messages together into one conversation
    public class ChatHistoryRequest
    {
        public string SessionId { get; set; } = string.Empty;  // e.g. "user-123"
        public string Message { get; set; } = string.Empty;
    }

    // What we send back — same as ChatResponse but includes the session
    public class ChatHistoryResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Reply { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int TotalMessagesInHistory { get; set; } // useful for debugging
    }

    // Represents a single message in the conversation history
    // Role is either "user" or "model" (Gemini's name for the AI)
    public class ChatMessageEffectiveDating
    {
        public string Role { get; set; } = string.Empty;    // "user" or "model"
        public string Content { get; set; } = string.Empty; // the actual text
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}