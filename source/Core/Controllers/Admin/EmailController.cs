using System.Threading.Tasks;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers.Admin
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService emailService;

        public EmailController(EmailService emailService) => this.emailService = emailService;

        [HttpPost]
        [Route("send")]
        public async Task Send([FromQuery] string email, [FromQuery] string subject, [FromBody] string message)
        {
            await emailService.SendEmail(email, subject, message).ConfigureAwait(false);
        }
    }
}