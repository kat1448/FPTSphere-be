using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Director,Event Manager,Staff")]
    public class ParticipantsController : ControllerBase
    {
        private readonly IParticipantService _participantService;
        private readonly IFileService _fileService;

        public ParticipantsController(IParticipantService participantService, IFileService fileService)
        {
            _participantService = participantService;
            _fileService = fileService;
        }

        /// <summary>
        /// Get participants list by event and sub-event
        /// </summary>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetParticipantsByEvent(
            int eventId,
            [FromQuery] int? subEventId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _participantService.GetParticipantsByEventAsync(
                    eventId, 
                    subEventId, 
                    search, 
                    status);

                return Ok(ApiResponse<ParticipantListResponseDto>.SuccessResult(
                    result, 
                    $"Retrieved {result.TotalCount} participants"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Refresh/Sync attendance data from previously uploaded Excel file.
        /// This endpoint reads the last sync configuration and re-syncs from the same source.
        /// NOTE: Google Sheets sync is NOT YET IMPLEMENTED - only Excel file refresh is supported.
        /// </summary>
        [HttpPost("sync/refresh")]
        public async Task<IActionResult> RefreshSync([FromBody] SyncAttendanceRequestDto request)
        {
            try
            {
                var result = await _participantService.RefreshSyncAsync(
                    request.EventId, 
                    request.SubEventId);

                if (result.Success)
                {
                    return Ok(ApiResponse<SyncAttendanceResponseDto>.SuccessResult(
                        result, 
                        result.Message));
                }
                else
                {
                    return BadRequest(ApiResponse<SyncAttendanceResponseDto>.ErrorResult(
                        result.Message));
                }
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(501, ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Unified endpoint to sync attendance data from either Excel file or Google Sheet.
        /// Priority: Excel file > Google Sheet
        /// - If Excel file is provided: Uploads and syncs from Excel file
        /// - If Google Sheet ID is provided: Attempts to sync from Google Sheet
        ///   - If Google Sheets API is not available: Returns error requesting Excel file upload
        /// Note: Google Sheet ID can be extracted from Google Sheet URL:
        /// Example: https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit
        /// </summary>
        [HttpPost("sync")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SyncAttendance([FromForm] SyncAttendanceFormDto dto)
        {
            try
            {
                string? excelFileUrl = null;

                // Priority 1: Process Excel file if provided
                if (dto.File != null && dto.File.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".xlsx", ".xls" };
                    var fileExtension = System.IO.Path.GetExtension(dto.File.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Only Excel files (.xlsx, .xls) are allowed"));
                    }

                    // Upload file
                    try
                    {
                        var uploadResult = await _fileService.UploadFileAsync(dto.File, "attendance-excel");
                        if (string.IsNullOrWhiteSpace(uploadResult.Url))
                        {
                            return BadRequest(ApiResponse<object>.ErrorResult("File upload failed: No URL returned"));
                        }

                        excelFileUrl = uploadResult.Url;
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult($"File upload failed: {ex.Message}"));
                    }
                }

                // Create unified request
                var request = new SyncAttendanceUnifiedRequestDto
                {
                    EventId = dto.EventId,
                    SubEventId = dto.SubEventId,
                    GoogleSheetId = dto.GoogleSheetId
                };

                // Sync using unified method
                var syncResult = await _participantService.SyncAttendanceUnifiedAsync(request, excelFileUrl);

                if (syncResult.Success)
                {
                    return Ok(ApiResponse<SyncAttendanceResponseDto>.SuccessResult(
                        syncResult, 
                        syncResult.Message));
                }
                else
                {
                    return BadRequest(ApiResponse<SyncAttendanceResponseDto>.ErrorResult(
                        syncResult.Message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Sync from Google Sheet (Legacy endpoint - kept for backward compatibility)
        /// NOTE: This endpoint is NOT YET IMPLEMENTED. It requires Google Sheets API credentials and authentication.
        /// Please use POST /api/Participants/sync instead with GoogleSheetId parameter.
        /// </summary>
        [HttpPost("sync/google-sheet")]
        [Obsolete("Use POST /api/Participants/sync instead")]
        public async Task<IActionResult> SyncFromGoogleSheet([FromBody] SyncAttendanceRequestDto request)
        {
            try
            {
                var unifiedRequest = new SyncAttendanceUnifiedRequestDto
                {
                    EventId = request.EventId,
                    SubEventId = request.SubEventId,
                    GoogleSheetId = request.GoogleSheetId
                };

                var result = await _participantService.SyncAttendanceUnifiedAsync(unifiedRequest);

                if (result.Success)
                {
                    return Ok(ApiResponse<SyncAttendanceResponseDto>.SuccessResult(result, result.Message));
                }
                else
                {
                    return BadRequest(ApiResponse<SyncAttendanceResponseDto>.ErrorResult(result.Message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload Excel file and sync attendance data (Legacy endpoint - kept for backward compatibility)
        /// Please use POST /api/Participants/sync instead with file parameter.
        /// </summary>
        [HttpPost("sync/upload-excel")]
        [Consumes("multipart/form-data")]
        [Obsolete("Use POST /api/Participants/sync instead")]
        public async Task<IActionResult> UploadExcelAndSync([FromForm] SyncAttendanceFormDto dto)
        {
            return await SyncAttendance(dto);
        }
    }
}

