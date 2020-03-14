using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Solution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolutionController : Controller
    {
        private readonly ISolutionService solutionService;

        public SolutionController(ISolutionService solutionService)
        {
            this.solutionService = solutionService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult<Guid>> Upload([FromBody] Solution solution)
        {
            solution.Id = Guid.NewGuid();
            solution.Timestamp = DateTime.Now;

            await solutionService.Upload(solution).ConfigureAwait(false);

            return Ok(solution.Id);
        }

        [HttpGet]
        [Route("download")]
        public async Task<ActionResult<byte[]>> Download(Guid solutionId)
        {
            var content = await solutionService.Download(solutionId).ConfigureAwait(false);

            if (content == null)
                return NotFound();

            return File(content, "application/octet-stream");
        }

        [HttpPost]
        [Route("rate")]
        public async Task<ActionResult> RateProgress([FromBody] SolutionProgress progress)
        {
            progress.Timestamp = DateTime.Now;

            await solutionService.RateProgress(progress).ConfigureAwait(false);

            return Ok();
        }

        [HttpGet]
        [Route("student/summaries")]
        public async Task<ActionResult<IEnumerable<SolutionStudentSummary>>> SelectStudentSummaries(Guid taskId, int lastLoadedIndex)
        {
            return Ok(await solutionService.SelectStudentSummaries(taskId, lastLoadedIndex, 10).ConfigureAwait(false));
        }
    }
}