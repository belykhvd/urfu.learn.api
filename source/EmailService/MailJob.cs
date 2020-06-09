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
            await using var conn = new NpgsqlConnection(ConnectionString);
            var (secret, email) = await conn.QuerySingleOrDefaultAsync<(Guid Secret, string Email)>(
                $@"select secret, email
                       from {PgSchema.invite}
                       where not is_sent
                       limit 1").ConfigureAwait(false);

            if (email == null)
                return;

            Console.WriteLine($"Sending invite to {email}");

            var inviteUrl = $"http://localhost:8080/acceptInvite?secret={secret}";
            await emailSender.SendInvite(email, inviteUrl).ConfigureAwait(false);
        }
    }
}