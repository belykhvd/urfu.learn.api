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

        #region PROFILE CRUD

        [HttpGet]
        [Route("profile/{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            return (await userService.Read(userId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("profile/update")]
        public async Task<IActionResult> SaveProfile([FromBody] Profile profile)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await userService.Update(profile).ConfigureAwait(false)).ActionResult();
        }

        #endregion
    }
}