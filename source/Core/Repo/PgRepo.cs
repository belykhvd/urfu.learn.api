using Microsoft.Extensions.Configuration;

namespace Core.Repo
{
    public class PgRepo
    {
        protected readonly string ConnectionString;

        protected PgRepo(IConfiguration config)
        {
            ConnectionString = config.GetValue<string>("PostgresConnectionString");
        }
    }
}