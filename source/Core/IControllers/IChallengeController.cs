using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Core.IControllers
{
    public interface IChallengeController
    {
        [Route("challenges")] Task<IActionResult> Get([FromQuery] Guid challengeId, [FromQuery] Guid userId);
    }
}