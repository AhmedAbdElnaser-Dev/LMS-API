using System;
using System.Threading.Tasks;
using LMS_API.Controllers.Units.Commands;
using LMS_API.Controllers.Units.Queries;
using LMS_API.Controllers.Units.ViewModels;
using LMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers.Units
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly UnitService _unitService;

        public UnitsController(UnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpPost]
        public async Task<ActionResult<UnitViewModel>> CreateUnit([FromBody] AddUnitCommand command)
        {
            var unit = await _unitService.CreateUnitAsync(command.CourseId, command.Name, command.Description);
            return Ok(new UnitViewModel
            {
                Id = unit.Id,
                CourseId = unit.CourseId,
                Name = unit.Translations[0].Name,
                //Description = unit.Translations[0].Description
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UnitViewModel>> UpdateUnit(Guid id, [FromBody] EditUnitCommand command)
        {
            var unit = await _unitService.UpdateUnitAsync(id, command.Name, command.Description);
            return Ok(new UnitViewModel
            {
                Id = unit.Id,
                CourseId = unit.CourseId,
                Name = unit.Translations[0].Name,
                //Description = unit.Translations[0].Description
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnitViewModel>> GetUnit(Guid id)
        {
            var unit = await _unitService.GetUnitAsync(id);
            return Ok(new UnitViewModel
            {
                Id = unit.Id,
                CourseId = unit.CourseId,
                Name = unit.Translations[0].Name,
                //Description = unit.Translations[0].Description
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUnit(Guid id)
        {
            await _unitService.DeleteUnitAsync(id);
            return NoContent();
        }
    }
}
