using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Contracts.Types.CheckSystem;
using Contracts.Types.Media;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Repo
{
    public class FileRepo : PgRepo
    {
        private readonly string rootPath;

        public FileRepo(IConfiguration configuration) : base(configuration)
        {
            rootPath = configuration.GetValue<string>("FileRootPath");

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
            
            await RegisterFile(attachment).ConfigureAwait(false);
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

            await RegisterFile(attachment).ConfigureAwait(false);
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

        public async Task<string> ReadSourceCode(Guid fileId)
        {
            var stream = StreamFile(fileId);
            if (stream == null)
                return null;

            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }

        public async Task RegisterFile(Attachment attachment)
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

        public async Task<IEnumerable<TestCase>> ReadTests(Guid taskId)
        {
            var testPath = BuildTestPath(taskId);

            var testCases = new Dictionary<int, TestCase>();

            var testDirectory = new DirectoryInfo(testPath);
            foreach (var file in testDirectory.EnumerateFiles())
            {
                var splitted = file.Name.Split(".");
                if (splitted.Length < 2
                    || !int.TryParse(splitted[0], out var number)
                    || (splitted[1].ToLower() != "in"
                    && splitted[1].ToLower() != "out"))
                {
                    continue;
                }

                if (!testCases.TryGetValue(number, out var testCase))
                {
                    testCases[number] = new TestCase
                    {
                        Number = number
                    };
                }

                testCase = testCases[number];

                var fileContent = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);

                if (splitted[1].ToLower() == "in")
                    testCase.Input = fileContent;
                else
                    testCase.CorrectOutput = fileContent;
            }

            return testCases.Values;
        }

        public long GetFileSize(Guid fileId) => new FileInfo(BuildPath(fileId)).Length;

        private string BuildPath(Guid fileId) => Path.Combine(rootPath, "files", $"{fileId:N}");
        private string BuildTestPath(Guid taskId) => Path.Combine(rootPath, "tests", $"{taskId:N}");
    }
}