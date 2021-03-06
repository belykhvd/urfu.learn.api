﻿using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Repo
{
    public class Repo<TEntity> : PgRepo, IRepo<TEntity>
    {
        private readonly string relationName;

        public Repo(IConfiguration config, string relationName) : base(config)
        {
            this.relationName = relationName;
        }

        public async Task<Guid> Save(Guid? id, TEntity data)
        {
            id ??= Guid.NewGuid();

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @$"insert into {relationName} (id, data)
                       values (@Id, @Data::jsonb)
                       on conflict (id) do update set data = @Data::jsonb", new {id, data}).ConfigureAwait(false);

            await SaveIndex(conn, id.Value, data).ConfigureAwait(false);

            return id.Value;
        }

        protected virtual Task SaveIndex(NpgsqlConnection conn, Guid id, TEntity data) => Task.CompletedTask;

        protected virtual async Task DeleteIndex(NpgsqlConnection conn, Guid id)
        {
            await conn.ExecuteAsync(@$"delete from {relationName} where id = @Id", new {id}).ConfigureAwait(false);
        }

        public async Task<TEntity> Get(Guid id)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<TEntity>(
                $@"select data
                       from {relationName}
                       where id = @Id
                       limit 1", new {id}).ConfigureAwait(false);
        }

        public async Task Delete(Guid id)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync($@"delete from {relationName} where id = @Id", new {id}).ConfigureAwait(false);
            await DeleteIndex(conn, id).ConfigureAwait(false);
        }

        public async Task<Result<Guid>> Save(TEntity data)
        {
            try
            {
                var entityId = Guid.NewGuid();

                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.ExecuteAsync(
                    $@"insert into {relationName} (id, data)
                           values (@Id, @Data::jsonb)", new {Id = entityId, data}).ConfigureAwait(false);

                return Result<Guid>.Success(entityId);
            }
            catch (PostgresException e) when (e.SqlState == "23505")
            {
                return Result<Guid>.Fail(OperationStatusCode.Conflict, "Entity with such ID already exists");
            }
        }

        public async Task<Result<TEntity>> Read(Guid id)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var data = await conn.QuerySingleOrDefaultAsync<TEntity>(
                $@"select data
                       from {relationName}
                       where id = @Id
                       limit 1", new {id}).ConfigureAwait(false);

            return data == null
                ? Result<TEntity>.Fail(OperationStatusCode.NotFound, "Entity with such ID does not exists")
                : Result<TEntity>.Success(data);
        }

        public async Task<Result> Update(Guid id, TEntity data)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);

            var updated = await conn.QuerySingleOrDefaultAsync<bool?>(
                $@"update {relationName}
                      set data = @Data::jsonb
                      where id = @Id
                      returning true", new {id, data}).ConfigureAwait(false);

            return updated == null
                ? Result.Fail(OperationStatusCode.NotFound, "Entity with such ID does not exists")
                : Result.Success;
        }

        public async Task<Result> DeleteOne(Guid id)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync($@"delete from {relationName} where id = @Id", new {id}).ConfigureAwait(false);

            return Result.Success;
        }
    }
}