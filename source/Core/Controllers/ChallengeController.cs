using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Challenge;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService challengeService;

        public ChallengeController(IChallengeService challengeService)
        {
            this.challengeService = challengeService;
        }

        [HttpGet]
        [Route("challenge")]
        public async Task<IActionResult> Get([FromQuery] Guid challengeId, [FromQuery] Guid userId)
        {
            return (await challengeService.Get(challengeId, userId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("challenges/add")]
        public async Task<IActionResult> Add([FromBody] ChallengeBase challengeBase)
        {
            return (await challengeService.Save(challengeBase).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("challenge/{challengeId}/update")]
        public async Task<IActionResult> Update([FromRoute] Guid challengeId, [FromBody] ChallengeBase challenge)
        {
            return (await challengeService.Update(challengeId, challenge).ConfigureAwait(false)).ActionResult();
        }
    }
}