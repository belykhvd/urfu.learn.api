using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Group;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService groupService;

        public GroupController(IGroupService groupService)
        {
            this.groupService = groupService;
        }

        
        
        #region CRUD

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add([FromBody] Group group)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // TODO authorization
            // TODO preprocessing and validation

            return (await groupService.Save(group).ConfigureAwait(false)).ActionResult();
        }

        [HttpGet]
        [Route("get/{groupId}")]
        public async Task<IActionResult> Get(Guid groupId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await groupService.Read(groupId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> Update(Guid groupId, [FromBody] Group group)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await groupService.Update(groupId, group).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] Guid groupId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await groupService.Delete(groupId).ConfigureAwait(false)).ActionResult();
        }

        #endregion

        #region MEMBERSHIP

        [HttpPost]
        [Route("{groupId}/include")]
        public async Task<IActionResult> Include([FromRoute] Guid groupId, [FromQuery] Guid userId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // authorization

            return (await groupService.Include(groupId, userId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost]
        [Route("{groupId}/exclude")]
        public async Task<IActionResult> Exclude([FromRoute] Guid groupId, [FromQuery] Guid userId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // authorization

            return (await groupService.Exclude(groupId, userId).ConfigureAwait(false)).ActionResult();
        }

        [HttpGet]
        [Route("{groupId}/getMembers")]
        public async Task<IActionResult> GetMembers([FromRoute] Guid groupId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // authorization

            return (await groupService.GetMembers(groupId).ConfigureAwait(false)).ActionResult();
        }

        #endregion
    }
}