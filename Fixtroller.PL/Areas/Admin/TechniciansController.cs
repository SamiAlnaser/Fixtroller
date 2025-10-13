using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.TechnicianDTOs.Requests;
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
    public class TechniciansController : ControllerBase
    {
        private readonly ITechnicianService _TechnicianService;
        private readonly IMaintenanceRequestService _requestService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TechniciansController(ITechnicianService TechnicianService, IMaintenanceRequestService RequestService, IStringLocalizer<SharedResource> localizer)
        {
            _TechnicianService = TechnicianService;
            _requestService = RequestService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TechniciansFilterRequestDTO filter)
        {
            var list = await _TechnicianService.GetAsync(filter);
            return Ok(list);
        }

        [HttpPost("{id:int}/assign")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignTechnicianRequestDTO dto)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var (res, key) = await _requestService.AssignTechnicianAsync(id, dto.TechnicianUserId, language);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }
        [HttpPatch("category")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateTechnicianCategoryRequestDTO dto)
        {
            var ok = await _TechnicianService.UpdateTechnicianCategoryAsync(dto);
            return ok ? NoContent() : BadRequest(new { message = _localizer["BadRequest"].Value });
        }

        [HttpGet("by-category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId, [FromQuery] string? search)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";
            var data = await _TechnicianService.GetByCategoryAsync(categoryId, search, language);
            return Ok(data);
        }
    }
}
