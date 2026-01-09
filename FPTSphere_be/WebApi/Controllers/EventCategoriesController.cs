using BusinessLayer.DTOs;
using BusinessLayer.DTOs.EventCategory;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly IEventCategoryService _eventCategoryService;

        public EventCategoriesController(IEventCategoryService eventCategoryService)
        {
            _eventCategoryService = eventCategoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var result = await _eventCategoryService.GetAllAsync();
                return Ok(ApiResponse<List<EventCategoryDto>>.SuccessResult(result, $"Retrieved {result.Count} categories"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var result = await _eventCategoryService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Category not found"));
                return Ok(ApiResponse<EventCategoryDto>.SuccessResult(result, "Category retrieved"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }
    }
}

