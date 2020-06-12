using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("signUp")]
        public async Task<ActionResult<AuthResult>> SignUp([FromBody] RegistrationData registrationData)
        {
            var authResult = await authService.SignUp(registrationData).ConfigureAwait(false);
            if (authResult == null)
                return Conflict();

            await HttpAuthorize(authResult.UserId, authResult.Role).ConfigureAwait(false);

            return authResult;
        }

        [HttpPost]
        [Route("signIn")]
        public async Task<ActionResult<AuthResult>> Login([FromBody] AuthData authData)
        {
            var authResult = await authService.Authorize(authData).ConfigureAwait(false);
            if (authResult == null)
                return Unauthorized();

            await HttpAuthorize(authResult.UserId, authResult.Role).ConfigureAwait(false);

            return authResult;
        }

        [HttpPost]
        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Ok();
        }

        [HttpPost]
        [Route("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordData passwordData)
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);

            if (await authService.ChangePassword(userId, passwordData).ConfigureAwait(false))
                return Ok();

            return Unauthorized();
        }

        private async Task HttpAuthorize(Guid userId, UserRole userRole)
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, $"{userId:N}"),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, $"{userRole}")

            }, "ApplicationCookie");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        }
    }
}