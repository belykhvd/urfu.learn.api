using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.User;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Route("profile/get")]
        public async Task<Profile> GetProfile([FromQuery] Guid userId)
            => await userService.GetProfile(userId).ConfigureAwait(false);

        [HttpPost]
        [Route("profile/save")]
        public async Task SaveProfile([FromQuery] Guid userId, [FromBody] Profile profile)
            => await userService.SaveProfile(userId, profile).ConfigureAwait(false);
    }
}