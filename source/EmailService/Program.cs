using System;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace EmailService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var settings = new Settings
            {
                Server = configuration.GetValue<string>("Server"),
                Port = configuration.GetValue<int>("Port"),
                Email = configuration.GetValue<string>("Email"),
                Password = configuration.GetValue<string>("Password"),
                Sender = configuration.GetValue<string>("Sender")
            };

            var emailSender = new EmailSender(settings);
            var mailJob = new MailJob(configuration, emailSender);

            Console.WriteLine("Starting job...");

            mailJob.Run(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}