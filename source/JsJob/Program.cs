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

            var dbStorableTypes = new[]
            {
                typeof(CheckTask)
            };

            foreach (var type in dbStorableTypes)
                SqlMapper.AddTypeHandler(type, new DapperTypeHandler());

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var jsRunner = new JsRunner();
            var fileRepo = new FileRepo(configuration);
            var jsJob = new JsCheckJob(configuration, jsRunner, fileRepo);

            jsJob.Next().GetAwaiter().GetResult();
        }
    }
}