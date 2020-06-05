using System;
using Microsoft.AspNetCore.Mvc;
using Repo;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly FileRepo fileRepo;

        public MediaController(FileRepo fileRepo) => this.fileRepo = fileRepo;

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