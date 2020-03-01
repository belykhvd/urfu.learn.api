using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.User;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;

        public UserController(IAuthService authService, IUserService userService)
        {
            this.authService = authService;
            this.userService = userService;
        }

        #region Profile

        [HttpGet]
        [Route("profile/{userId}")]
        public async Task<ActionResult<Profile>> GetProfile(Guid userId)
        {
            var profile = await userService.GetProfile(userId).ConfigureAwait(false);
            if (profile != null)
                return Ok(profile);

            return NotFound();
        }

        [HttpPost]
        [Route("profile/save")]
        public async Task<IActionResult> SaveProfile([FromBody] Profile profile)
        {
            var userId = await Authorize().ConfigureAwait(false);
            if (userId == null)
                return Unauthorized();

            await userService.SaveProfile(userId.Value, profile).ConfigureAwait(false);
            return Ok();
        }

        #endregion

        #region Profile photo

        [HttpGet]
        [Route("profile/photo/{userId}")]
        public async Task<IActionResult> GetProfilePhoto(Guid userId)
        {
            var photoBase64 = await userService.GetProfilePhoto(userId).ConfigureAwait(false);
            if (photoBase64 != null)
                return Ok(photoBase64);

            return NotFound();
        }

        [HttpPost]
        [Route("profile/photo/save")]
        public async Task<IActionResult> SaveProfilePhoto([FromBody] string photoBase64)
        {
            var userId = await Authorize().ConfigureAwait(false);
            if (userId == null)
                return Unauthorized();

            await userService.GetProfilePhoto(userId.Value).ConfigureAwait(false);
            return Ok();
        }

        #endregion

        // TODO пока что так
        private async Task<Guid?> Authorize()
        {
            var sessionToken = Request.Cookies["auth"]; 
            if (sessionToken == null)
                return null;

            return await authService.Authenticate(sessionToken).ConfigureAwait(false);
        }
    }
}