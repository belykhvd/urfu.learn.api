using System;
using System.Threading.Tasks;
using Core.Repo;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly FileRepo fileRepo;

        public MediaController(FileRepo fileRepo) => this.fileRepo = fileRepo;

        [HttpGet]
        [Route("attachment")]
        public async Task<FileResult> Attachment(Guid fileId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("download")]
        public IActionResult Download([FromQuery] Guid id)
        {
            var fileStream = fileRepo.StreamFile(id);
            if (fileStream == null)
                return NotFound();

            return File(fileStream, "application/octet-stream", $"{id:N}");
        }
    }
}