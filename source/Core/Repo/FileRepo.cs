using System;
using System.IO;
using System.Threading.Tasks;
using Contracts.Types.Media;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Core.Repo
{
    public class FileRepo : PgRepo
    {
        private readonly string rootPath;

        public FileRepo(IConfiguration configuration) : base(configuration)
        {
            rootPath = AppDomain.CurrentDomain.BaseDirectory; //environment.ContentRootPath;

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);
        }

        public async Task<Guid> SaveAttachment(MultipartSection section, string nameHtmlEncoded, Guid author)
        {
            var fileId = await WriteOnDisk(section).ConfigureAwait(false);
            var attachment = new Attachment
            {
                Id = fileId,
                Name = nameHtmlEncoded,
                Size = GetFileSize(fileId),
                Author = author,
                Timestamp = DateTime.UtcNow
            };
            
            await RegisterAttachment(attachment).ConfigureAwait(false);
            return fileId;
        }

        public async Task<Guid> SaveAttachment(IFormFile formFile, string nameHtmlEncoded, Guid author)
        {
            var fileId = await WriteOnDisk(formFile).ConfigureAwait(false);
            var attachment = new Attachment
            {
                Id = fileId,
                Name = nameHtmlEncoded,
                Size = GetFileSize(fileId),
                Author = author,
                Timestamp = DateTime.UtcNow
            };

            await RegisterAttachment(attachment).ConfigureAwait(false);
            return fileId;
        }

        public async Task<Guid> WriteOnDisk(MultipartSection section)
        {
            var fileId = Guid.NewGuid();
            
            using (var fileStream = new FileStream(BuildPath(fileId), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await section.Body.CopyToAsync(fileStream);
            }

            return fileId;
        }

        public async Task<Guid> WriteOnDisk(IFormFile formFile)
        {
            var fileId = Guid.NewGuid();

            using (var fileStream = new FileStream(BuildPath(fileId), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await formFile.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return fileId;
        }

        public FileStream StreamFile(Guid fileId)
        {
            try
            {
                return new FileStream(BuildPath(fileId), FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex) when (ex is IOException)
            {
                return null;
            }
        }
        
        public async Task RegisterAttachment(Attachment attachment)
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
        }

        public async Task<Attachment> GetAttachment(Guid attachmentId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Attachment>(
                $@"select json_build_object(
		                    'id', fi.id,
		                    'name', fi.name,
		                    'size', fi.size,
		                    'timestamp', fi.timestamp,
		                    'author', fi.author)
                       from file_index fi
                       where id = @Id
                       limit 1",
                new {Id = attachmentId}).ConfigureAwait(false);
        }

        public long GetFileSize(Guid fileId) => new FileInfo(BuildPath(fileId)).Length;

        private string BuildPath(Guid fileId) => Path.Combine(rootPath, "files", $"{fileId:N}");
    }
}