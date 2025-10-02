using Fixtroller.BLL.Services.TCategoryServices;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fixtroller.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TCategoriesController : ControllerBase
    {
        private readonly ITCategoryService _TcategoryService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TCategoriesController(ITCategoryService TcategoryService , IStringLocalizer<SharedResource> localizer)
        {
            _TcategoryService = TcategoryService;
            _localizer = localizer;
        }

        //// GET: api/Tcategories
        //[HttpGet("")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var result = await _TcategoryService.GetAllForUserAsync();
        //    return Ok(result);
        //}

        //// GET: api/Tcategories/active
        //[HttpGet("active")]
        //public async Task<IActionResult> GetActiveTCategories()
        //{
        //    var result = await _TcategoryService.GetActiveForUserAsync();
        //    return Ok(new { message = _localizer["Success"].Value, data = result });
        //}

        //// GET: api/Tcategories/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById([FromRoute] int id)
        //{
        //    var result = await _TcategoryService.GetByIdForUserAsync(id);
        //    return result == null
        //        ? NotFound(new { message = _localizer["NotFound"].Value })
        //        : Ok(result);
        //}

        //// POST: api/Tcategories
        //[HttpPost]
        //public async Task<IActionResult> Add([FromBody] TCategoryRequestDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var Id = await _TcategoryService.AddAsync(dto);
        //    return CreatedAtAction(nameof(GetById), new { Id }, new { message = _localizer["Created"].Value, Id });
        //}

        //// PUT: api/Tcategories/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TCategoryRequestDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var updated = await _TcategoryService.UpdateAsync(id, dto);
        //    return updated == 0
        //        ? NotFound(new { message = _localizer["NotFound"].Value })
        //        : Ok(new { message = _localizer["Updated"].Value });
        //}

        //// DELETE: api/Tcategories/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete([FromRoute] int id)
        //{
        //    var removed = await _TcategoryService.RemoveAsync(id);
        //    return removed == 0
        //        ? NotFound(new { message = _localizer["NotFound"].Value })
        //        : Ok(new { message = _localizer["Deleted"].Value });
        //}

        //// PATCH: api/Tcategories/{id}/toggle-status
        //[HttpPatch("{id}/toggle-status")]
        //public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        //{
        //    var toggled = await _TcategoryService.ToggleStatusAsync(id);
        //    return toggled == false
        //        ? NotFound(new { message = _localizer["NotFound"].Value })
        //        : Ok(new { message = _localizer["StatusToggled"].Value });
        //}
    }
}
