using System;

namespace Contracts.Types.Chat
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
    }
}