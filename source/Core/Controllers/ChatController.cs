using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;

        public ChatController(IChatService chatService)
            => this.chatService = chatService;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromQuery] Guid taskId, [FromQuery] Guid studentId,
                                                     [FromBody][Required][MaxLength(Constants.ChatMessageMaxLength)] string message)
        {
            if (!Guid.TryParse(HttpContext.User.Identity.Name, out var senderId) ||
                senderId != studentId && !HttpContext.User.IsInRole(nameof(UserRole.Professor)) && !HttpContext.User.IsInRole(nameof(UserRole.Admin)))
            {
                return Unauthorized();
            }

            await chatService.Send(taskId, studentId, senderId, message).ConfigureAwait(true);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages([FromQuery] Guid taskId, [FromQuery] Guid studentId)
        {
            if (!Guid.TryParse(HttpContext.User.Identity.Name, out var senderId) ||
                senderId != studentId && !HttpContext.User.IsInRole(nameof(UserRole.Professor)) && !HttpContext.User.IsInRole(nameof(UserRole.Admin)))
            {
                return Unauthorized();
            }

            return Ok(await chatService.GetMessages(taskId, studentId).ConfigureAwait(true));
        }
    }
}