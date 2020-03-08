using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Repo
{
    public class CrudRepo<TEntity> : PgRepo, ICrudRepo<TEntity> where TEntity : DbEntity
    {
        private readonly string relationName;

        protected CrudRepo(IConfiguration config, string relationName) : base(config)
        {
            this.relationName = relationName;
        }

        public async Task<OperationStatus<Guid>> Create(TEntity data)
        {
            try
            {
                data.InitAsFresh();

                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.ExecuteAsync(
                    $@"insert into {relationName} (id, deleted, version, data)
                           values (@Id, false, 0, @Data::jsonb)", new {data.Id, data}).ConfigureAwait(false);

                return OperationStatus<Guid>.Success(data.Id);
            }
            catch (PostgresException e) when (e.SqlState == "23505")
            {
                return OperationStatus<Guid>.Fail(OperationStatusCode.Conflict, "Entity with such ID already exists");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // TODO logging
                return OperationStatus<Guid>.Fail(OperationStatusCode.InternalServerError);
            }
        }

        public async Task<OperationStatus<TEntity>> Read(Guid id)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                var data = await conn.QuerySingleOrDefaultAsync<TEntity>(
                    $@"select data
                           from {relationName}
                           where id = @Id
                             and not deleted
                           limit 1", new {id}).ConfigureAwait(false);

                return data != null
                    ? OperationStatus<TEntity>.Success(data)
                    : OperationStatus<TEntity>.Fail(OperationStatusCode.NotFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // TODO logging
                return OperationStatus<TEntity>.Fail(OperationStatusCode.InternalServerError);
            }
        }

        public async Task<OperationStatus> Update(TEntity data)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);

                var version = await conn.QuerySingleOrDefaultAsync<int?>(
                    $@"select version
                          from {relationName}
                          where id = @Id
                            and not deleted
                          limit 1", new {data.Id}).ConfigureAwait(false);

                if (version == null)
                    return OperationStatus.Fail(OperationStatusCode.NotFound);

                if (data.Version != version)
                    return OperationStatus.Fail(OperationStatusCode.Conflict, "Your data version is outdated");

                data.Version = version.Value + 1;

                var updated = await conn.QuerySingleOrDefaultAsync<bool?>(
                    $@"update {relationName}
                          set data = @Data::jsonb,
                              version = version + 1
                          where id = @Id
                            and not deleted
                            and version = @Version
                          returning true", new {data.Id, version, data}).ConfigureAwait(false);
                
                if (updated == null)
                    return OperationStatus.Fail(OperationStatusCode.Conflict, "Your data version is outdated");

                return OperationStatus.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // TODO logging
                return OperationStatus.Fail(OperationStatusCode.InternalServerError);
            }
        }

        public async Task<OperationStatus> Delete(Guid id)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                var deleted = await conn.QuerySingleOrDefaultAsync<bool?>(
                    $@"update {relationName}
                          set deleted = true
                          where id = @Id
                          returning true", new {id}).ConfigureAwait(false);

                if (deleted == null)
                    return OperationStatus.Fail(OperationStatusCode.NotFound);

                return OperationStatus.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // TODO logging
                return OperationStatus.Fail(OperationStatusCode.InternalServerError);
            }
        }
    }
}