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

            return (await groupService.Create(group).ConfigureAwait(false)).ActionResult();
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
        public async Task<IActionResult> Update([FromBody] Group group)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            return (await groupService.Update(group).ConfigureAwait(false)).ActionResult();
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

        // [HttpPost]
        // [Route("{groupId}/include")]
        // public async Task<IActionResult> Include([FromRoute] Guid groupId, [FromQuery] Guid userId)
        // {
        //     if (!User.Identity.IsAuthenticated)
        //         return Unauthorized();
        //
        //     // authorization
        //
        //     await groupService.Include()
        // }

        #endregion
    }
}