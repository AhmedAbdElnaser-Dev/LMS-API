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
                command.Title,
                command.Description
                );

            return Ok(new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LessonViewModel>> UpdateLesson(Guid id, [FromBody] EditLessonCommand command)
        {
            var lesson = await _lessonService.UpdateLessonAsync(
                id,
                command.Title,
                command.Description);

            return Ok(new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonViewModel>> GetLesson(Guid id)
        {
            var lesson = await _lessonService.GetLessonAsync(id);

            var viewModel = new LessonViewModel
            {
                Id = lesson.Id,
                UnitId = lesson.UnitId,
                Title = lesson.Title,
                Description = lesson.Description,
                Translations = lesson.Translations.Select(t => new LessonTranslationViewModel
                {
                    Id = t.Id,
                    Language = t.Language,
                    Content = t.Content
                }).ToList()
            };

            return Ok(viewModel);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLesson(Guid id)
        {
            await _lessonService.DeleteLessonAsync(id);
            return NoContent();
        }

        [HttpPost("translation")]
        public async Task<ActionResult<LessonTranslationViewModel>> AddTranslation([FromBody] AddLessonTranslationCommand command)
        {
            var result = await _lessonService.AddTranslationAsync(command);

            return Ok(new LessonTranslationViewModel
            {
                Id = result.Id,
                Language = result.Language,
                Title = result.Title,
                Content = result.Content
            });
        }

        [HttpPut("translation/{id}")]
        public async Task<ActionResult<LessonTranslationViewModel>> UpdateTranslation(Guid id, [FromBody] EditLessonTranslationCommand command)
        {
            var result = await _lessonService.UpdateTranslationAsync(id, command);

            return Ok(new LessonTranslationViewModel
            {
                Id = result.Id,
                Language = result.Language,
                Title = result.Title,
                Content = result.Content
            });
        }

    }
}
