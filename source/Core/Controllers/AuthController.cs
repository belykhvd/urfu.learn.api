using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public async Task<AuthResult> SignUp([FromBody] RegistrationData registrationData)
        {
            var authResult = await authService.SignUp(registrationData).ConfigureAwait(false);

            await HttpAuthorize(authResult.UserId).ConfigureAwait(false);

            return authResult;
        }


        [HttpPost]
        [Route("signIn")]
        public async Task<ActionResult<AuthResult>> Login([FromBody] AuthData authData)
        {
            var authResult = await authService.Authorize(authData).ConfigureAwait(false);
            if (authResult == null)
                return Unauthorized();

            await HttpAuthorize(authResult.UserId).ConfigureAwait(false);

            return authResult;
        }

        [HttpPost]
        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Ok();
        }

        private async Task HttpAuthorize(Guid userId)
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, $"{userId}")

            }, "ApplicationCookie");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        }
    }
}