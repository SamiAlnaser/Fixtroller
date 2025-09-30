using Fixtroller.BLL.Services.ProblemTypesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixtroller.PL.Areas.Technician
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Technician")]
    [Authorize(Roles = "Technician")]
    public class ProblemTypesController : ControllerBase
    {
        private readonly IProblemTypesService _problemTypesService;

        public ProblemTypesController(IProblemTypesService problemTypesService)
        {
            _problemTypesService = problemTypesService;
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
    }
}
