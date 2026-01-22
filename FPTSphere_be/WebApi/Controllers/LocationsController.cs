using BusinessLayer.DTOs.Location;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? building = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var result = await _locationService.GetLocationsAsync(
                    page, pageSize, search, building, isActive, sortBy, sortDescending);

                return Ok(ApiResponse<PagedResult<LocationDto>>.SuccessResult(
                    result, $"Retrieved {result.TotalRecords} locations"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Get location by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var location = await _locationService.GetByIdAsync(id);
                if (location == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Location {id} not found"));

                return Ok(ApiResponse<LocationDto>.SuccessResult(location, "Location retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// Get all active locations
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var locations = await _locationService.GetActiveAsync();
                return Ok(ApiResponse<List<LocationDto>>.SuccessResult(
                    locations, $"Retrieved {locations.Count} active locations"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Get locations by building
        [HttpGet("building/{building}")]
        public async Task<IActionResult> GetByBuilding(string building)
        {
            try
            {
                var locations = await _locationService.GetByBuildingAsync(building);
                return Ok(ApiResponse<List<LocationDto>>.SuccessResult(
                    locations, $"Found {locations.Count} in building {building}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Search locations by keyword
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(ApiResponse<object>.ErrorResult("Search term required"));

                var locations = await _locationService.SearchAsync(term);
                return Ok(ApiResponse<List<LocationDto>>.SuccessResult(
                    locations, $"Found {locations.Count} locations"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// Get available locations for event (checks time conflicts)
        /// Optional params:
        ///  - ignoreEventId: bỏ qua chính event đang cập nhật
        ///  - ignoreParentEventId: bỏ qua parent event khi tạo/đổi sub-event
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int? minCapacity = null,
            [FromQuery] string? building = null,
            [FromQuery] int? ignoreEventId = null,
            [FromQuery] int? ignoreParentEventId = null)
        {
            try
            {
                if (startTime >= endTime)
                    return BadRequest(ApiResponse<object>.ErrorResult("Start time must be before end time"));

                if (startTime < DateTime.Now)
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot book locations in the past"));

                var locations = await _locationService.GetAvailableLocationsAsync(
                    startTime, endTime, minCapacity, building, ignoreEventId, ignoreParentEventId);

                var message = $"Found {locations.Count} available location(s)";
                if (minCapacity.HasValue)
                    message += $" with capacity >= {minCapacity}";
                if (!string.IsNullOrWhiteSpace(building))
                    message += $" in building {building}";

                return Ok(ApiResponse<List<LocationDto>>.SuccessResult(locations, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Create new location (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            try
            {
                if (await _locationService.NameExistsAsync(dto.Name))
                    return BadRequest(ApiResponse<object>.ErrorResult("Location name already exists"));

                var location = await _locationService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = location.LocationId },
                    ApiResponse<LocationDto>.SuccessResult(location, "Location created"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// Update location (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLocationDto dto)
        {
            try
            {
                if (await _locationService.NameExistsAsync(dto.Name, id))
                    return BadRequest(ApiResponse<object>.ErrorResult("Location name already exists"));

                var location = await _locationService.UpdateAsync(id, dto);
                if (location == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Location {id} not found"));

                return Ok(ApiResponse<LocationDto>.SuccessResult(location, "Location updated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Toggle location active status (Admin only)
        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                var isActive = await _locationService.ToggleStatusAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(
                    isActive, $"Location {(isActive ? "activated" : "deactivated")}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
        /// Delete location (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _locationService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<object>.ErrorResult($"Location {id} not found"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Location deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}