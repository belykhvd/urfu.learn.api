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
        public async Task<Guid> SignUp([FromBody] RegistrationData registrationData)
            => await authService.SignUp(registrationData).ConfigureAwait(false);

        [HttpPost]
        [Route("signIn")]
        public async Task<IActionResult> SignIn([FromBody] AuthData authData)
        {
            var userId = await authService.TryGetUserId(authData).ConfigureAwait(false);

            if (userId == null)
                return Unauthorized();

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userId.Value.ToString())
            }, "ApplicationCookie");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            return Ok();
        }

        [HttpPost]
        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Ok();
        }
    }
}