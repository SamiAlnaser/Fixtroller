using Fixtroller.BLL.Services.ProblemTypesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.Employee
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Employee")]
    [Authorize(Roles = "Employee , SpecialEmployee")]
    public class ProblemTypesController : ControllerBase
    {
        private readonly IProblemTypesService _problemTypesService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProblemTypesController(IProblemTypesService problemTypesService, IStringLocalizer<SharedResource> localizer)
        {
            _problemTypesService = problemTypesService;
            _localizer = localizer;
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
    }
}
