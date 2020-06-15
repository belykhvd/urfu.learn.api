using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Chat;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Services
{
    public class ChatService : PgRepo, IChatService
    {
        public ChatService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task Send(Guid taskId, Guid studentId, Guid senderId, string message)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.chat} (id, task_id, student_id, timestamp, sender_id, message)
                       values (@Id, @TaskId, @StudentId, @Timestamp, @SenderId, @Message)",
                new
                {
                    id = Guid.NewGuid(),
                    taskId,
                    studentId,
                    timestamp = DateTime.UtcNow,
                    senderId,
                    message
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessages(Guid taskId, Guid studentId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<ChatMessage>(
                $@"select jsonb_build_object(
                            'id', ch.id,
                            'taskId', task_id,
                            'studentId', student_id,
                            'timestamp', timestamp,
                            'senderId', sender_id,
                            'senderName', ui.fio,
                            'message', message
                       )
                       from {PgSchema.chat} ch
                       left join {PgSchema.user_index} ui
                         on ch.sender_id = ui.id
                       where task_id = @TaskId
                         and student_id = @StudentId
                       order by timestamp desc", new
                {
                    taskId,
                    studentId
                }).ConfigureAwait(false);
        }
    }
}