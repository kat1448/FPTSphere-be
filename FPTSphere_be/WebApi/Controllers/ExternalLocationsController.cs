using BusinessLayer.DTOs.ExternalLocation;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExternalLocationsController : ControllerBase
    {
        private readonly IExternalLocationService _externalLocationService;

        public ExternalLocationsController(IExternalLocationService externalLocationService)
        {
            _externalLocationService = externalLocationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetExternalLocations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] decimal? minCost = null,
            [FromQuery] decimal? maxCost = null,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var result = await _externalLocationService.GetExternalLocationsAsync(
                    page, pageSize, search, minCost, maxCost, sortBy, sortDescending);

                return Ok(ApiResponse<PagedResult<ExternalLocationDto>>.SuccessResult(
                    result, $"Retrieved {result.TotalRecords} external locations"));
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
                var location = await _externalLocationService.GetByIdAsync(id);
                if (location == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"External location {id} not found"));

                return Ok(ApiResponse<ExternalLocationDto>.SuccessResult(location, "External location retrieved"));
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

                var locations = await _externalLocationService.SearchAsync(term);
                return Ok(ApiResponse<List<ExternalLocationDto>>.SuccessResult(
                    locations, $"Found {locations.Count} external locations"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("cost-range")]
        public async Task<IActionResult> GetByCostRange(
            [FromQuery] decimal minCost,
            [FromQuery] decimal maxCost)
        {
            try
            {
                if (minCost > maxCost)
                    return BadRequest(ApiResponse<object>.ErrorResult("Min cost cannot be greater than max cost"));

                var locations = await _externalLocationService.GetByCostRangeAsync(minCost, maxCost);
                return Ok(ApiResponse<List<ExternalLocationDto>>.SuccessResult(
                    locations, $"Found {locations.Count} external locations in cost range"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Director,Event Manager")]
        public async Task<IActionResult> Create([FromBody] CreateExternalLocationDto dto)
        {
            try
            {
                if (await _externalLocationService.NameExistsAsync(dto.Name))
                    return BadRequest(ApiResponse<object>.ErrorResult("External location name already exists"));

                var location = await _externalLocationService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = location.ExternalLocationId },
                    ApiResponse<ExternalLocationDto>.SuccessResult(location, "External location created"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExternalLocationDto dto)
        {
            try
            {
                if (await _externalLocationService.NameExistsAsync(dto.Name, id))
                    return BadRequest(ApiResponse<object>.ErrorResult("External location name already exists"));

                var location = await _externalLocationService.UpdateAsync(id, dto);
                if (location == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"External location {id} not found"));

                return Ok(ApiResponse<ExternalLocationDto>.SuccessResult(location, "External location updated"));
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
                var (success, message) = await _externalLocationService.DeleteAsync(id);
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
