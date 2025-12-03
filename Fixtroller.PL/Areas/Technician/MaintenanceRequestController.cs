using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Fixtroller.PL.Areas.Technician
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Technician")]
    [Authorize(Roles = "Technician")]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly IMaintenanceRequestService _maintenanceRequestService;
        private readonly ITechnicianService _technicianService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public MaintenanceRequestController(
            IMaintenanceRequestService maintenanceRequestService,
            ITechnicianService technicianService,
            IStringLocalizer<SharedResource> localizer)
        {
            _maintenanceRequestService = maintenanceRequestService;
            _technicianService = technicianService;
            _localizer = localizer;
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] MaintenanceRequestRequestDTO dto, CancellationToken ct)
        {
            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var id = await _maintenanceRequestService.CreateWithFile(dto, userId, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }


        [HttpPost("scenario")]
        public async Task<IActionResult> CreateScenario(
                [FromForm] MaintenanceRequestScenarioRequestDTO dto,
                CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var role = User.FindFirst("role")?.Value
                     ?? "Technician";

            var (id, key) = await _maintenanceRequestService.CreateScenarioAsync(
                dto,
                callerUserId: userId,
                callerRole: role,
                ct: ct);

            if (id is null)
            {
                // فشل (حالة غير مسموحة، ownerUserId ناقص، ...الخ)
                return BadRequest(new { message = _localizer[key].Value });
            }

            // نجاح
            return CreatedAtAction(
                nameof(GetById),
                new { id = id.Value },
                new
                {
                    id = id.Value,
                    message = _localizer[key].Value
                });
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("Id")?.Value
                         ?? string.Empty;

            var role = User.FindFirst("role")?.Value
                     ?? "Technician"; // قيمة افتراضية آمنة

            try
            {
                var res = await _maintenanceRequestService.GetByIdAsync(id, userId, role, language, ct);

                return res is null
                    ? NotFound(new { message = _localizer["Request_NotFound"].Value })
                    : Ok(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
      int pageNumber = 1,
      int pageSize = 10,
      CancellationToken ct = default)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var role = User.FindFirst("role")?.Value
                    ?? User.FindFirst(ClaimTypes.Role)?.Value
                    ?? string.Empty;

            var list = await _maintenanceRequestService.GetMineAsync(
                userId, role, language, pageNumber, pageSize, ct);

            return Ok(list);
        }

        [HttpGet("assigned")]
        public async Task<IActionResult> GetAssigned(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
        {
            // اللغة
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            // ايد الفني من التوكن
            var technicianUserId = User.FindFirst("Id")?.Value
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(technicianUserId))
                return Unauthorized();

            var result = await _technicianService.GetMyAssignedAsync(technicianUserId,language,pageNumber,pageSize,ct);

            return Ok(result);
        }


        [HttpPatch("{id:int}/case")]
        public async Task<IActionResult> ChangeCase(int id, [FromBody] ChangeCaseTypeRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var (res, key) = await _maintenanceRequestService.ChangeCaseAsync(
                id, dto, userId, role, preferOwnerPath: false, language, ct);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpPatch("{id:int}/caseMine")]
        public async Task<IActionResult> ChangeCaseMine(int id, [FromBody] ChangeCaseTypeRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst("role")?.Value ?? ""; // قد يكون Empty, service يتعامل بمنطق المالك

            var (res, key) = await _maintenanceRequestService.ChangeCaseAsync(
                id, dto, userId, role, preferOwnerPath: true, language, ct);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpPost("{id:int}/work/start")]
        public async Task<IActionResult> StartWork(int id, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? "";

            // لو دخل شخص مش فني بالغلط (احتياط لو تغيّر الـ attribute)
            var isTech = string.Equals(role, "Technician", StringComparison.OrdinalIgnoreCase);
            if (!isTech) return Forbid();

            // الفني يبدّأ لنفسه
            var (ok, key) = await _maintenanceRequestService.StartWorkAsync(
                requestId: id,
                technicianUserId: userId,
                callerUserId: userId,
                callerRole: role,
                ct: ct);

            if (!ok) return BadRequest(new { message = _localizer[key].Value });
            return Ok(new { message = _localizer[key].Value });
        }


        [HttpPost("{id:int}/notes")]
        public async Task<IActionResult> AddNote(int id, [FromBody] AddNoteRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id")?.Value ?? "";
            var role = User.FindFirst("role")?.Value ?? "Technician";

            var (res, key) = await _maintenanceRequestService.AddNoteAsync(id, userId, role, dto, language, ct);

            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMine(int id, [FromForm] MaintenanceRequestUpdateDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var role = User.FindFirst("role")?.Value
                      ?? User.FindFirst(ClaimTypes.Role)?.Value
                      ?? string.Empty;

            try
            {
                var (res, key) = await _maintenanceRequestService.UpdateMineAsync(id, userId, role, dto, language, ct);
                if (res is null) return BadRequest(new { message = _localizer[key].Value });
                return Ok(new { message = _localizer[key].Value, data = res });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost("{id:int}/images")]
        public async Task<IActionResult> AddImages(int id, [FromForm] AddImagesRequestDTO dto, CancellationToken ct)
        {
            var language = Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(language)) language = "ar";

            var userId = User.FindFirst("Id")?.Value
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? string.Empty;

            var role = User.FindFirst("role")?.Value ?? "Technician";

            var (res, key) = await _maintenanceRequestService.AddImagesAsync(id, userId, role, dto, language, ct);
            if (res is null)
                return BadRequest(new { message = _localizer[key].Value });

            return Ok(new { message = _localizer[key].Value, data = res });
        }

    }
}
