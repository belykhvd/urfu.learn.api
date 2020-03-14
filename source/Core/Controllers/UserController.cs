using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.User;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        [Route("profile/update")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromBody] Profile profile)
        {
            if (!Guid.TryParse(User.Identity.Name, out var userId) || profile.Id != userId)
                return Unauthorized($"Нет прав на изменение профиля с ID {profile.Id}");

            profile.Sanitize(); // 500
            var validationStatus = profile.Validate(); // 500
            if (!validationStatus.IsSuccess)
                return Result<Guid>.Fail(OperationStatusCode.ValidationError, validationStatus.ErrorMessage).ActionResult();

            return (await userService.Update(userId, profile).ConfigureAwait(false)).ActionResult();
        }

        #endregion
    }
}