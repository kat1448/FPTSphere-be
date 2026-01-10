using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Resource;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        // GET LIST RESOURCES - All roles can view
        [HttpGet]
        [Authorize] // All authenticated users can view
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

        // GET RESOURCE BY ID - All roles can view
        [HttpGet("{id}")]
        [Authorize] // All authenticated users can view
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

        // GET ACTIVE RESOURCES - All roles can view
        [HttpGet("active")]
        [Authorize] // All authenticated users can view
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

        // GET RESOURCES BY TYPE - All roles can view
        [HttpGet("type/{type}")]
        [Authorize] // All authenticated users can view
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

        // SEARCH RESOURCES - All roles can view
        [HttpGet("search")]
        [Authorize] // All authenticated users can view
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

        // GET AVAILABLE QUANTITY - All roles can view
        [HttpGet("{id}/available-quantity")]
        [Authorize] // All authenticated users can view
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

        // CREATE RESOURCE - Admin only
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

        // UPDATE RESOURCE - Admin only
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

        // TOGGLE RESOURCE STATUS (Active/Deactive) - Admin only (Soft Delete)
        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var resource = await _resourceService.GetByIdAsync(id);
                if (resource == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Resource {id} not found"));

                var isActive = await _resourceService.ToggleStatusAsync(id);
                var updatedResource = await _resourceService.GetByIdAsync(id);
                
                return Ok(ApiResponse<ResourceDto>.SuccessResult(
                    updatedResource!, 
                    $"Resource {(isActive ? "activated" : "deactivated")} successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        // DELETE RESOURCE (Soft Delete - Deactivate) - Admin only
        // This endpoint uses soft delete by setting IsActive = false
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resource = await _resourceService.GetByIdAsync(id);
                if (resource == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Resource {id} not found"));

                // Soft delete: deactivate if currently active
                if (resource.IsActive == true)
                {
                    await _resourceService.ToggleStatusAsync(id);
                }

                var updatedResource = await _resourceService.GetByIdAsync(id);
                return Ok(ApiResponse<ResourceDto>.SuccessResult(
                    updatedResource!, 
                    "Resource deactivated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}