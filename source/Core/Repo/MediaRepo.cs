using System;
using System.IO;
using System.Threading.Tasks;
using Contracts.Types.Media;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Repo
{
    public class MediaRepo : PgRepo
    {
        private static readonly string RootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", "media");

        public MediaRepo(IConfiguration configuration) : base(configuration)
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);
        }

        public async Task SaveAttachment(string path, Attachment attachment)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.file_index} (id, name, size, timestamp, author)
                       values (@Id, @Name, @Size, @Timestamp, @Author)",
                new
                {
                    attachment.Id,
                    attachment.Name,
                    attachment.Size,
                    attachment.Timestamp,
                    attachment.Author
                }).ConfigureAwait(false);

            //await File.WriteAllBytesAsync(path, attachment.Content).ConfigureAwait(false);
        }

        public async Task<Guid> WriteFile(byte[] content)
        {
            var fileId = Guid.NewGuid();

            await File.WriteAllBytesAsync(MediaPath(fileId), content).ConfigureAwait(false);

            return fileId;
        }

        public async Task<byte[]> ReadFile(Guid fileId)
        {
            try
            {
                return await File.ReadAllBytesAsync(MediaPath(fileId)).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public FileStream StreamFile(Guid fileId)
        {
            try
            {
                return new FileStream(MediaPath(fileId), FileMode.Open);
            }
            catch (Exception ex) when (ex is IOException)
            {
                return null;
            }
        }

        private static string MediaPath(Guid fileId) => Path.Combine(RootPath, $"{fileId:N}");
        private static string UserMediaPath(Guid userId, Guid fileId) => Path.Combine(RootPath, $"{userId:N}", $"{fileId:N}");
    }
}