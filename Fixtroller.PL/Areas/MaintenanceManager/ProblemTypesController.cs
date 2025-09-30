using Fixtroller.BLL.Services.ProblemTypesServices;
using Fixtroller.DAL.DTOs.ProblemTypeDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class ProblemTypesController : ControllerBase
    {

        private readonly IProblemTypesService _problemTypesService;

        public ProblemTypesController(IProblemTypesService problemTypesService)
        {
            _problemTypesService = problemTypesService;
        }

        // GET: api/ProblemsTypes
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _problemTypesService.GetAllAsync();
            return Ok(result);
        }

        // GET: api/ProblemsTypes/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProblemsTypes()
        {
            var result = await _problemTypesService.GetActiveAsync();
            return Ok(new { message = "Success", data = result });
        }

        // GET: api/ProblemsTypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _problemTypesService.GetByIdAsync(id);
            return result == null
                ? NotFound(new { message = "Problem type not found" })
                : Ok(result);
        }

        // POST: api/ProblemsTypes
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProblemTypeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _problemTypesService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "Problem type added successfully", id });
        }

        // PUT: api/ProblemsTypes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProblemTypeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _problemTypesService.UpdateAsync(id, dto);
            return updated == 0
                ? NotFound(new { message = "Problem type not found" })
                : Ok(new { message = "Problem type updated successfully" });
        }

        // DELETE: api/ProblemsTypes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var removed = await _problemTypesService.RemoveAsync(id);
            return removed == 0
                ? NotFound(new { message = "Problem type not found" })
                : Ok(new { message = "Problem type deleted successfully" });
        }

        // PATCH: api/ProblemsTypes/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var toggled = await _problemTypesService.ToggleStatusAsync(id);
            return toggled == false
                ? NotFound(new { message = "Problem type not found" })
                : Ok(new { message = "Problem type status toggled successfully" });
        }
    }

}

