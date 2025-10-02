using Fixtroller.BLL.Services.ProblemTypesServices;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProblemTypesController : ControllerBase
    {
        private readonly IProblemTypesService _problemTypesService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProblemTypesController(IProblemTypesService problemTypesService, IStringLocalizer<SharedResource> localizer)
        {
            _problemTypesService = problemTypesService;
            _localizer = localizer;
        }

        // GET: api/ProblemsTypes
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _problemTypesService.GetAllForUserAsync();
            return Ok(result);
        }

        // GET: api/ProblemsTypes/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProblemsTypes()
        {
            var result = await _problemTypesService.GetActiveForUserAsync();
            return Ok(new { message = _localizer["Success"].Value, data = result });
        }

        // GET: api/ProblemsTypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _problemTypesService.GetByIdForUserAsync(id);
            return result == null
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(result);
        }

        // POST: api/ProblemsTypes
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProblemTypeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _problemTypesService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = _localizer["Created"].Value, id });
        }

        // PUT: api/ProblemsTypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProblemTypeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _problemTypesService.UpdateAsync(id, dto);
            return updated == 0
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["Updated"].Value });
        }

        // DELETE: api/ProblemsTypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var removed = await _problemTypesService.RemoveAsync(id);
            return removed == 0
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["Deleted"].Value });
        }

        // PATCH: api/ProblemsTypes/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var toggled = await _problemTypesService.ToggleStatusAsync(id);
            return toggled == false
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["StatusToggled"].Value });
        }
    }
}
