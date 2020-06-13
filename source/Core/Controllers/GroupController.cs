﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Group;
using Contracts.Types.Group.ViewModel;
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
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<Guid>> Save([FromBody][Required] Group group)
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
        [Authorize(Roles = nameof(UserRole.Admin))]
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
        public async Task<IEnumerable<StudentInvite>> GetStudents([FromQuery] Guid groupId)
            => await groupService.GetStudents(groupId).ConfigureAwait(false);


        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> InviteStudent([FromQuery] Guid groupId, [FromQuery][EmailAddress] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values);

            await groupService.InviteStudent(groupId, email).ConfigureAwait(true);

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AcceptInvite([FromQuery] Guid secret)
        {
            if (!Guid.TryParse(HttpContext.User.Identity.Name, out var userId))
                return Unauthorized();

            var success = await groupService.AcceptInvite(secret, userId).ConfigureAwait(true);
            if (!success)
                return Unauthorized();

            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IEnumerable<GroupInviteItem>> GetInviteList()
        {
            return await groupService.GetInviteList().ConfigureAwait(true);
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<GroupItem>> GetUserList()
        {
            return await groupService.GetUsers().ConfigureAwait(true);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task GrantAccess([FromQuery] Guid groupId, [FromBody][Required] Guid[] courseIds)
        {
            await groupService.GrantAccess(groupId, courseIds).ConfigureAwait(true);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task RevokeAccess([FromQuery] Guid groupId, [FromBody][Required] Guid[] courseIds)
        {
            await groupService.RevokeAccess(groupId, courseIds).ConfigureAwait(true);
        }
    }
}