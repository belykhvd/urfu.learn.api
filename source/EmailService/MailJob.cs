using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace EmailService
{
    public class MailJob : PgRepo
    {
        private const int DelayTimeout = 2000;

        private readonly EmailSender emailSender;

        public MailJob(IConfiguration config, EmailSender emailSender) : base(config)
        {
            this.emailSender = emailSender;
        }

        public async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(DelayTimeout, cancellation).ConfigureAwait(false);
                await Next().ConfigureAwait(false);
            }
        }

        private async Task Next()
        {
            (Guid secret, string email) = (Guid.Empty, null);

            await using (var conn = new NpgsqlConnection(ConnectionString))
            {
                (secret, email) = await conn.QuerySingleOrDefaultAsync<(Guid Secret, string Email)>(
                    $@"select secret, email
                       from {PgSchema.invite}
                       where not is_sent
                       limit 1").ConfigureAwait(false);
            };

            if (email == null)
                return;

            Console.WriteLine($"Sending invite to {email}");

            var inviteUrl = $"http://localhost:8080/group/acceptInvite?secret={secret}";
            await emailSender.SendInvite(email, inviteUrl).ConfigureAwait(false);

            await using var conn2 = new NpgsqlConnection(ConnectionString);
            await conn2.ExecuteAsync(
                $@"update {PgSchema.invite}
                      set is_sent = true
                      where secret = @Secret", new {secret}).ConfigureAwait(false);
        }
    }
}