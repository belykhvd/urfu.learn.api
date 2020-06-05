using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Types.CheckSystem;
using Dapper;
using Microsoft.Extensions.Configuration;
using Repo;

namespace JsJob
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var dbStorableTypes = new[] {typeof(CheckTask)};
            foreach (var type in dbStorableTypes)
                SqlMapper.AddTypeHandler(type, new DapperTypeHandler());

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var jsRunner = new JsRunner();
            var fileRepo = new FileRepo(configuration);
            var jsJob = new JsCheckJob(configuration, jsRunner, fileRepo);

            var cancellationSource = new CancellationTokenSource();

            //Task.Run(() => , cancellationSource.Token);

            jsJob.Run(cancellationSource.Token).GetAwaiter().GetResult();

            while (true)
            {
                var key = Console.ReadKey(true);
                if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.C)
                {
                    cancellationSource.Cancel();
                    break;
                }
            }
        }
    }
}