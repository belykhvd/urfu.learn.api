using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.User;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> SignUp([FromBody] RegistrationData registrationData)
        {
            var result = await authService.SignUp(registrationData).ConfigureAwait(false);
            if (result.IsSuccessful)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Route("signIn")]
        public async Task<IActionResult> SignIn([FromBody] AuthData authData)
        {
            var result = await authService.SignIn(authData).ConfigureAwait(false);

            if (result.IsSuccessful)
            {
                Response.Cookies.Append("auth", result.Value, new CookieOptions {HttpOnly = true});
                return Ok();
            }

            return Forbid(result.ErrorMessage);
        }

        [HttpPost]
        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            if (Request.Cookies.ContainsKey("auth"))
            {
                var isSignedOut = await authService.SignOut(Request.Cookies["auth"]).ConfigureAwait(false);

                if (isSignedOut)
                    Response.Cookies.Append("auth", "", new CookieOptions {Expires = DateTime.Now.AddDays(-1)});
            }

            return Ok();
        }
    }
}