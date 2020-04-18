using System;
using System.Collections.Generic;
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

        [HttpGet][Route("studentlist")]
        public async Task<IEnumerable<StudentList>> GetStudentList([FromQuery] int year, [FromQuery] int semester)
        {
            return await groupService.GetStudentList(year, semester).ConfigureAwait(false);
        }


        [HttpGet][Route("list")]
        public async Task<GroupLink[]> List() => await groupService.List().ConfigureAwait(false);
       
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

        [HttpGet][Route("{groupId}/listMembers")]
        public async Task<IActionResult> ListMembers([FromQuery] int year, [FromQuery] int semester, [FromRoute] Guid groupId)
        {
            return (await groupService.ListMembers(year, semester, groupId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost][Route("{groupId}/include")]
        public async Task<IActionResult> Include([FromQuery] int year, [FromQuery] int semester, [FromRoute] Guid groupId, [FromQuery] Guid userId)
        {
            return (await groupService.Include(year, semester, groupId, userId).ConfigureAwait(false)).ActionResult();
        }

        [HttpPost][Route("{groupId}/exclude")]
        public async Task<IActionResult> Exclude([FromQuery] int year, [FromQuery] int semester, [FromRoute] Guid groupId, [FromQuery] Guid userId)
        {
            return (await groupService.Exclude(year, semester, groupId, userId).ConfigureAwait(false)).ActionResult();
        }

        #endregion
    }
}