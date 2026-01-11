using BusinessLayer.DTOs;
using BusinessLayer.DTOs.File;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload file to Cloudinary
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="folder">Optional folder path in Cloudinary (e.g., "events/banners", "users/avatars")</param>
        /// <param name="transformation">Optional transformation string (e.g., "w_500,h_500,c_fill")</param>
        /// <returns>File upload response with Cloudinary URL</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileWithSettingsDto dto)
        {
            try
            {
                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("File is required and cannot be empty"));

                var result = await _fileService.UploadFileAsync(dto.File, dto.Folder, dto.Transformation);
                return Ok(ApiResponse<FileUploadResponseDto>.SuccessResult(result, "File uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error uploading file: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload multiple files to Cloudinary
        /// </summary>
        /// <param name="files">Files to upload</param>
        /// <param name="folder">Optional folder path in Cloudinary</param>
        /// <returns>List of file upload responses</returns>
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] UploadMultipleFilesDto dto)
        {
            try
            {
                if (dto.Files == null || dto.Files.Count == 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("At least one file is required"));

                var results = new List<FileUploadResponseDto>();
                var errors = new List<string>();

                foreach (var file in dto.Files)
                {
                    try
                    {
                        var result = await _fileService.UploadFileAsync(file, dto.Folder);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{file.FileName}: {ex.Message}");
                    }
                }

                if (results.Count == 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("All files failed to upload", errors));

                var response = ApiResponse<List<FileUploadResponseDto>>.SuccessResult(
                    results,
                    $"Successfully uploaded {results.Count} out of {dto.Files.Count} file(s)");

                if (errors.Count > 0)
                {
                    response.Errors = errors;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error uploading files: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete file from Cloudinary
        /// </summary>
        /// <param name="publicId">Public ID of the file in Cloudinary</param>
        /// <returns>Success status</returns>
        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteFile(string publicId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Public ID is required"));

                var result = await _fileService.DeleteFileAsync(publicId);

                if (result)
                    return Ok(ApiResponse<object>.SuccessResult(null, "File deleted successfully"));
                else
                    return BadRequest(ApiResponse<object>.ErrorResult("Failed to delete file"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error deleting file: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get file URL from Cloudinary public ID
        /// </summary>
        /// <param name="publicId">Public ID of the file</param>
        /// <param name="transformation">Optional transformation string</param>
        /// <returns>File URL</returns>
        [HttpGet("url/{publicId}")]
        public IActionResult GetFileUrl(string publicId, [FromQuery] string? transformation = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                    return BadRequest(ApiResponse<object>.ErrorResult("Public ID is required"));

                var url = _fileService.GetFileUrl(publicId, transformation);
                return Ok(ApiResponse<string>.SuccessResult(url, "File URL generated"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error generating file URL: {ex.Message}"));
            }
        }
    }
}

