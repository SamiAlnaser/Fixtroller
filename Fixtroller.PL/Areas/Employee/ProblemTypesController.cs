using Fixtroller.BLL.Services.ProblemTypesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.Employee
{
    [Route("Api/[area]/[controller]")]
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
        [HttpGet("Active")]
        public async Task<IActionResult> GetActiveProblemsTypes(CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _problemTypesService.GetActiveForUserAsync(language, ct);
            return Ok(new { message = _localizer["Success"].Value, data = result });
        }
    }
}
