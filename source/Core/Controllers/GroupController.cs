using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService groupService;

        public GroupController(IGroupService groupService)
        {
            this.groupService = groupService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Guid>> Save([FromBody] Group group)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values);

            if (group.Id == Guid.Empty)
                group.Id = Guid.NewGuid();

            await groupService.Save(group.Id, group).ConfigureAwait(false);

            return group.Id;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Group>> Get([FromQuery] Guid id)
        {
            var group = await groupService.Get(id).ConfigureAwait(false);
            if (group == null)
                return NotFound();

            return group;
        }

        [HttpPost]
        [Authorize]
        public async Task Delete([FromQuery] Guid id)
        {
            await groupService.Delete(id).ConfigureAwait(false);
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Group>> List()
            => await groupService.List().ConfigureAwait(false);

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<GroupStudent>> GetStudents([FromQuery] Guid groupId)
            => await groupService.GetStudents(groupId).ConfigureAwait(false);




        [HttpGet][Route("studentlist")]
        public async Task<IEnumerable<StudentList>> GetStudentList([FromQuery] int year, [FromQuery] int semester)
        {
            return await groupService.GetStudentList(year, semester).ConfigureAwait(false);
        }


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