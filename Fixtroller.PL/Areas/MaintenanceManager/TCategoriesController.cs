using Fixtroller.BLL.Services.TCategoryServices;
using Fixtroller.DAL.Data.DTOs.TCategoryDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Areas.MaintenanceManager
{
    [Route("Api/[area]/[controller]")]
    [ApiController]
    [Area("MaintenanceManager")]
    [Authorize(Roles = "MaintenanceManager")]
    public class TCategoriesController : ControllerBase
    {
        private readonly ITCategoryService _TcategoryService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TCategoriesController(ITCategoryService TcategoryService, IStringLocalizer<SharedResource> localizer)
        {
            _TcategoryService = TcategoryService;
            _localizer = localizer;
        }
        // GET: api/Tcategories
        [HttpGet("")]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _TcategoryService.GetAllForUserAsync(language, isActive, ct);
            return Ok(result);
        }

        // GET: api/Tcategories/active
        [HttpGet("Active")]
        public async Task<IActionResult> GetActiveTCategories(CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _TcategoryService.GetActiveForUserAsync(language, ct);
            return Ok(new { message = _localizer["Success"].Value, data = result });
        }

        // GET: api/Tcategories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var result = await _TcategoryService.GetByIdForUserAsync(id, language, ct);
            return result == null
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(result);
        }

        // POST: api/Tcategories
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TCategoryRequestDTO dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _TcategoryService.AddAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = _localizer["Created"].Value, id });
        }

        // PUT: api/Tcategories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TCategoryRequestDTO dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _TcategoryService.UpdateAsync(id, dto, ct);
            return updated == 0
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["Updated"].Value });
        }

        // DELETE: api/Tcategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var removed = await _TcategoryService.RemoveAsync(id, ct);
            return removed == 0
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["Deleted"].Value });
        }

        // PATCH: api/Tcategories/{id}/toggle-status
        [HttpPatch("{id}/Toggle-Status")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id, CancellationToken ct)
        {
            var toggled = await _TcategoryService.ToggleStatusAsync(id, ct);
            return toggled == false
                ? NotFound(new { message = _localizer["NotFound"].Value })
                : Ok(new { message = _localizer["StatusToggled"].Value });
        }
    }
}
