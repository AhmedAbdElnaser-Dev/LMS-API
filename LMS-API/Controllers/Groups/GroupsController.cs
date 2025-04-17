using LMS_API.Controllers.Groups.Commands;
using LMS_API.Helpers;
using LMS_API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS_API.Controllers.Groups
{
    [ApiController]
    [Route("api/groups")]
    public class GroupsController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupsController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost("add-translation")]
        public async Task<IActionResult> AddGroupTranslation([FromBody] AddGroupTranslationCommand command)
        {
            var result = await _groupService.AddGroupTranslationAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Translation);
        }

        [HttpPut("update-translation")]
        public async Task<IActionResult> UpdateGroupTranslation([FromBody] UpdateGroupTranslationCommand command)
        {
            var result = await _groupService.UpdateGroupTranslationAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Translation);
        }

        [HttpPost("add-to-course")]
        public async Task<IActionResult> AddGroupToCourse([FromBody] AddGroupToCourseCommand command)
        {
            var result = await _groupService.AddGroupToCourseAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Group);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseGroups(Guid courseId)
        {
            var result = await _groupService.GetCourseGroupsAsync(courseId);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Groups);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupDetails(Guid groupId)
        {
            var result = await _groupService.GetGroupDetailsAsync(groupId);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Group);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditGroup([FromBody] EditGroupCommand command)
        {
            var result = await _groupService.EditGroupAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Group);
        }

        [HttpPost("add-student")]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentToGroupCommand command)
        {
            var result = await _groupService.AddStudentToGroupAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("Student added to group successfully");
        }

        [HttpDelete("remove-student")]
        public async Task<IActionResult> RemoveStudent([FromBody] RemoveStudentFromGroupCommand command)
        {
            var result = await _groupService.RemoveStudentFromGroupAsync(command);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("Student removed from group successfully");
        }

        [HttpGet("{groupId}/students")]
        public async Task<IActionResult> GetGroupStudents(Guid groupId)
        {
            var result = await _groupService.GetGroupStudentsAsync(groupId);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Students);
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            var (success, errorMessage) = await _groupService.DeleteGroupAsync(groupId);
            if (!success)
                return BadRequest(new { Message = errorMessage });

            return Ok(new { Message = "Group deleted successfully" });
        }
    }
}
