using System;
using System.Threading.Tasks;
using LMS_API.Controllers.Lessons.Commands;
using LMS_API.Controllers.Lessons.ViewModels;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers.Lessons
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonsController : ControllerBase
    {
        private readonly LessonService _lessonService;

        public LessonsController(LessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost]
        public async Task<ActionResult<LessonViewModel>> CreateLesson([FromBody] AddLessonCommand command)
        {
            var lesson = await _lessonService.CreateLessonAsync(
                command.UnitId, 
                command.Name, 
                command.Description,
                command.Content);

            return Ok(new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
                //Name = lesson.Translations[0].Name,
                //Description = lesson.Translations[0].Description,
                Content = lesson.Translations[0].Content
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LessonViewModel>> UpdateLesson(Guid id, [FromBody] EditLessonCommand command)
        {
            var lesson = await _lessonService.UpdateLessonAsync(
                id,
                command.Name,
                command.Description,
                command.Content);

            return Ok(new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
                //Name = lesson.Translations[0].Name,
                //Description = lesson.Translations[0].Description,
                Content = lesson.Translations[0].Content
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonViewModel>> GetLesson(Guid id)
        {
            var lesson = await _lessonService.GetLessonAsync(id);
            return Ok(new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
                //Name = lesson.Translations[0].Name,
                //Description = lesson.Translations[0].Description,
                Content = lesson.Translations[0].Content
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLesson(Guid id)
        {
            await _lessonService.DeleteLessonAsync(id);
            return NoContent();
        }
    }
}
