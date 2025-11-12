using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Resource;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetResources(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? type = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var result = await _resourceService.GetResourcesAsync(
                    page, pageSize, search, type, isActive, sortBy, sortDescending);

                return Ok(ApiResponse<PagedResult<ResourceDto>>.SuccessResult(
                    result, $"Retrieved {result.TotalRecords} resources"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var resource = await _resourceService.GetByIdAsync(id);
                if (resource == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Resource {id} not found"));

                return Ok(ApiResponse<ResourceDto>.SuccessResult(resource, "Resource retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var resources = await _resourceService.GetActiveAsync();
                return Ok(ApiResponse<List<ResourceDto>>.SuccessResult(
                    resources, $"Retrieved {resources.Count} active resources"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                var resources = await _resourceService.GetByTypeAsync(type);
                return Ok(ApiResponse<List<ResourceDto>>.SuccessResult(
                    resources, $"Found {resources.Count} resources of type {type}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(ApiResponse<object>.ErrorResult("Search term required"));

                var resources = await _resourceService.SearchAsync(term);
                return Ok(ApiResponse<List<ResourceDto>>.SuccessResult(
                    resources, $"Found {resources.Count} resources"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}/available-quantity")]
        public async Task<IActionResult> GetAvailableQuantity(int id)
        {
            try
            {
                var availableQty = await _resourceService.GetAvailableQuantityAsync(id);
                return Ok(ApiResponse<int>.SuccessResult(
                    availableQty, $"Available quantity: {availableQty}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
        {
            try
            {
                if (await _resourceService.NameExistsAsync(dto.Name))
                    return BadRequest(ApiResponse<object>.ErrorResult("Resource name already exists"));

                var resource = await _resourceService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = resource.ResourceId },
                    ApiResponse<ResourceDto>.SuccessResult(resource, "Resource created"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceDto dto)
        {
            try
            {
                if (await _resourceService.NameExistsAsync(dto.Name, id))
                    return BadRequest(ApiResponse<object>.ErrorResult("Resource name already exists"));

                var resource = await _resourceService.UpdateAsync(id, dto);
                if (resource == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Resource {id} not found"));

                return Ok(ApiResponse<ResourceDto>.SuccessResult(resource, "Resource updated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                var isActive = await _resourceService.ToggleStatusAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(
                    isActive, $"Resource {(isActive ? "activated" : "deactivated")}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _resourceService.HardDeleteAsync(id);
                if (!success)
                    return BadRequest(ApiResponse<object>.ErrorResult(message));

                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}