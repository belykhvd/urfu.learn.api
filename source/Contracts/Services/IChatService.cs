using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Types.Chat;

namespace Contracts.Services
{
    public interface IChatService
    {
        Task Send(Guid taskId, Guid studentId, Guid senderId, string message);
        Task<IEnumerable<ChatMessage>> GetMessages(Guid taskId, Guid studentId);
    }
}