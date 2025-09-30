using Fixtroller.BLL.Services.TCategoryServices;
using Fixtroller.DAL.DTOs.TCategoryDTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixtroller.PL.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TCategoriesController : ControllerBase
    {
        private readonly ITCategoryService _TcategoryService;

        public TCategoriesController(ITCategoryService TcategoryService)
        {
            _TcategoryService = TcategoryService;
        }

        // GET: api/Tcategories
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _TcategoryService.GetAllAsync();
            return Ok(result);
        }

        // GET: api/Tcategories/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTCategories()
        {
            var result = await _TcategoryService.GetActiveAsync();
            return Ok(new { message = "Success", data = result });
        }



        // GET: api/Tcategories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _TcategoryService.GetByIdAsync(id);
            return result == null
                ? NotFound(new { message = "Category not found" })
                : Ok(result);
        }

        // POST: api/Tcategories
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TCategoryRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var Id = await _TcategoryService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { Id }, new { message = "Category added successfully", Id });
        }

        // PUT: api/Tcategories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TCategoryRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _TcategoryService.UpdateAsync(id, dto);
            return updated == 0
                ? NotFound(new { message = "Category not found" })
                : Ok(new { message = "Category updated successfully" });
        }

        // DELETE: api/Tcategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var removed = await _TcategoryService.RemoveAsync(id);
            return removed == 0
                ? NotFound(new { message = "Category not found" })
                : Ok(new { message = "Category deleted successfully" });
        }

        // PATCH: api/Tcategories/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var toggled = await _TcategoryService.ToggleStatusAsync(id);
            return toggled == false
                ? NotFound(new { message = "Category not found" })
                : Ok(new { message = "Category status toggled successfully" });
        }

    }
}
