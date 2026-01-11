using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.Extensions.Http;

namespace BusinessLayer.Services.Implementations
{
    public class ParticipantService : IParticipantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantService(IUnitOfWork unitOfWork, IUserRepository userRepository, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ParticipantListResponseDto> GetParticipantsByEventAsync(
            int eventId, 
            int? subEventId = null, 
            string? searchTerm = null, 
            string? statusFilter = null)
        {
            // Get raw records
            var rawRecords = subEventId.HasValue
                ? await _unitOfWork.AttendanceRawRecords.GetBySubEventWithUserAsync(subEventId.Value)
                : await _unitOfWork.AttendanceRawRecords.GetByEventWithUserAsync(eventId);

            // Get event attendances for users
            var eventAttendances = await _unitOfWork.EventAttendances.GetByEventAsync(eventId);
            var attendanceDict = eventAttendances.ToDictionary(a => a.UserId, a => a);

            // Build participant list
            var participants = new List<ParticipantDto>();

            foreach (var raw in rawRecords)
            {
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    if (!(raw.Email?.ToLower().Contains(searchLower) == true ||
                          raw.FullName?.ToLower().Contains(searchLower) == true ||
                          raw.User?.Email?.ToLower().Contains(searchLower) == true ||
                          raw.User?.FullName?.ToLower().Contains(searchLower) == true))
                    {
                        continue;
                    }
                }

                var participant = new ParticipantDto
                {
                    RawId = raw.RawId,
                    Email = raw.Email ?? raw.User?.Email ?? "",
                    FullName = raw.FullName ?? raw.User?.FullName ?? "",
                    ParticipantType = raw.IsGuest ? "Guest" : "User",
                    UserId = raw.UserId,
                    SubmittedAt = raw.SubmittedAt
                };

                // Determine status and attendance info
                if (raw.UserId.HasValue && attendanceDict.TryGetValue(raw.UserId.Value, out var attendance))
                {
                    participant.AttendanceId = attendance.AttendanceId;
                    participant.CheckinAt = attendance.CheckinAt;
                    participant.CheckoutAt = attendance.CheckoutAt;

                    if (attendance.CheckinAt.HasValue && attendance.CheckoutAt.HasValue)
                    {
                        participant.Status = "Checked Out";
                    }
                    else if (attendance.CheckinAt.HasValue)
                    {
                        participant.Status = "Checked In";
                    }
                    else
                    {
                        participant.Status = "Invited";
                    }
                }
                else
                {
                    // Guest or no attendance record
                    if (raw.CheckType == "CHECKOUT" || raw.CheckType == "CHECK_OUT")
                    {
                        participant.Status = "Checked Out";
                    }
                    else if (raw.CheckType == "CHECKIN" || raw.CheckType == "CHECK_IN")
                    {
                        participant.Status = "Checked In";
                    }
                    else
                    {
                        participant.Status = "Invited";
                    }
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                {
                    if (participant.Status != statusFilter)
                    {
                        continue;
                    }
                }

                participants.Add(participant);
            }

            // Calculate statistics
            var response = new ParticipantListResponseDto
            {
                Participants = participants,
                TotalCount = participants.Count,
                CheckedInCount = participants.Count(p => p.Status == "Checked In"),
                CheckedOutCount = participants.Count(p => p.Status == "Checked Out"),
                InvitedCount = participants.Count(p => p.Status == "Invited"),
                GuestCount = participants.Count(p => p.ParticipantType == "Guest"),
                UserCount = participants.Count(p => p.ParticipantType == "User")
            };

            return response;
        }

        /// <summary>
        /// Sync attendance data from Google Sheet.
        /// NOTE: This method requires Google Sheets API credentials and proper authentication.
        /// Currently returns NotImplementedException - use Excel file upload instead if Google Sheets API is not available.
        /// </summary>
        public async Task<SyncAttendanceResponseDto> SyncFromGoogleSheetAsync(SyncAttendanceRequestDto request)
        {
            // TODO: Implement Google Sheets API integration
            // This requires:
            // 1. Google Cloud Project with Sheets API enabled
            // 2. Service Account credentials or OAuth2 credentials
            // 3. Proper authentication setup
            // For now, return error message
            throw new NotImplementedException(
                "Google Sheets API integration is not yet implemented. " +
                "This requires Google Cloud credentials and API setup. " +
                "Please use Excel file upload instead (POST /api/Participants/sync/upload-excel).");
        }

        public async Task<SyncAttendanceResponseDto> SyncFromExcelFileAsync(int eventId, int? subEventId, string fileUrl)
        {
            try
            {
                // Download Excel file
                var httpClient = _httpClientFactory.CreateClient();
                var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
                using var stream = new MemoryStream(fileBytes);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                {
                    return new SyncAttendanceResponseDto
                    {
                        Success = false,
                        Message = "Excel file does not contain any worksheets.",
                        NewRecordsCount = 0,
                        UpdatedRecordsCount = 0,
                        TotalRecordsCount = 0,
                        SyncedAt = DateTime.Now
                    };
                }

                var newRecords = 0;
                var updatedRecords = 0;
                var syncedAt = DateTime.Now;

                // Detect column mapping from header row
                var headerRow = 1;
                var startRow = 2; // Data starts from row 2
                var endRow = worksheet.Dimension?.End.Row ?? startRow;

                // Column indices (will be detected from header)
                int emailCol = -1;
                int fullNameCol = -1;
                int checkTypeCol = -1;
                int submittedAtCol = -1;

                // Detect columns from header row
                var maxCol = worksheet.Dimension?.End.Column ?? 10;
                for (int col = 1; col <= maxCol; col++)
                {
                    var headerValue = worksheet.Cells[headerRow, col]?.Value?.ToString()?.Trim() ?? "";
                    var headerLower = headerValue.ToLower();

                    // Map common header names to columns
                    if (emailCol == -1 && (
                        headerLower.Contains("email") || 
                        headerLower == "e-mail" ||
                        headerLower == "mail"))
                    {
                        emailCol = col;
                    }
                    else if (fullNameCol == -1 && (
                        headerLower.Contains("họ") && headerLower.Contains("tên") ||
                        headerLower.Contains("full") && headerLower.Contains("name") ||
                        headerLower.Contains("tên") ||
                        headerLower.Contains("name") ||
                        headerLower == "họ và tên" ||
                        headerLower == "ho va ten"))
                    {
                        fullNameCol = col;
                    }
                    else if (checkTypeCol == -1 && (
                        headerLower.Contains("check") ||
                        headerLower.Contains("loại") ||
                        headerLower == "checktype" ||
                        headerLower == "check type"))
                    {
                        checkTypeCol = col;
                    }
                    else if (submittedAtCol == -1 && (
                        headerLower == "time" ||
                        headerLower == "timestamp" ||
                        headerLower == "thời gian" ||
                        headerLower == "ngày giờ" ||
                        headerLower == "ngay gio" ||
                        headerLower.Contains("timestamp") ||
                        headerLower.Contains("thời gian") ||
                        headerLower.Contains("time") ||
                        headerLower.Contains("submitted")))
                    {
                        submittedAtCol = col;
                    }
                }

                // Fallback: If columns not found, use default positions
                // Common Google Form format: STT, Timestamp, Email, FullName
                if (emailCol == -1)
                {
                    // Try to find email in common positions (col 3 or 4)
                    for (int col = 2; col <= 5; col++)
                    {
                        var sampleValue = worksheet.Cells[startRow, col]?.Value?.ToString() ?? "";
                        if (sampleValue.Contains("@") && emailCol == -1)
                        {
                            emailCol = col;
                            break;
                        }
                    }
                }

                if (fullNameCol == -1)
                {
                    // Usually after email column
                    if (emailCol > 0)
                    {
                        fullNameCol = emailCol + 1;
                    }
                    else
                    {
                        // Try to find name in common positions
                        for (int col = 3; col <= 5; col++)
                        {
                            var sampleValue = worksheet.Cells[startRow, col]?.Value?.ToString() ?? "";
                            if (!string.IsNullOrWhiteSpace(sampleValue) && !sampleValue.Contains("@") && fullNameCol == -1)
                            {
                                fullNameCol = col;
                                break;
                            }
                        }
                    }
                }

                if (submittedAtCol == -1)
                {
                    // Timestamp is usually in column 2 (after STT) in Google Forms
                    submittedAtCol = 2;
                }

                // Process data rows
                for (int row = startRow; row <= endRow; row++)
                {
                    var email = emailCol > 0 ? worksheet.Cells[row, emailCol]?.Value?.ToString()?.Trim() : null;
                    var fullName = fullNameCol > 0 ? worksheet.Cells[row, fullNameCol]?.Value?.ToString()?.Trim() : null;
                    var checkTypeStr = checkTypeCol > 0 ? worksheet.Cells[row, checkTypeCol]?.Value?.ToString()?.Trim() : null;
                    var submittedAtCell = submittedAtCol > 0 ? worksheet.Cells[row, submittedAtCol] : null;

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(fullName))
                    {
                        continue;
                    }

                    // Parse submitted date - EPPlus can return DateTime directly, Excel date serial (double), or string
                    DateTime submittedAt;
                    if (submittedAtCell?.Value != null)
                    {
                        // Case 1: EPPlus returns DateTime object directly
                        if (submittedAtCell.Value is DateTime dateTimeValue)
                        {
                            submittedAt = dateTimeValue;
                        }
                        // Case 2: Excel date serial number (double)
                        else if (submittedAtCell.Value is double excelDateSerial)
                        {
                            submittedAt = DateTime.FromOADate(excelDateSerial);
                        }
                        // Case 3: String representation - try to parse
                        else
                        {
                            var submittedAtStr = submittedAtCell.Value?.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(submittedAtStr))
                            {
                                if (DateTime.TryParse(submittedAtStr, out var parsedDate))
                                {
                                    submittedAt = parsedDate;
                                }
                                else if (double.TryParse(submittedAtStr, out var excelDate))
                                {
                                    // Excel date serial as string
                                    submittedAt = DateTime.FromOADate(excelDate);
                                }
                                else
                                {
                                    submittedAt = syncedAt;
                                }
                            }
                            else
                            {
                                submittedAt = syncedAt;
                            }
                        }
                    }
                    else
                    {
                        submittedAt = syncedAt;
                    }

                    // Find user by email first (needed for checkType auto-detection)
                    User? user = null;
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        user = await _userRepository.GetByEmailAsync(email);
                    }

                    // Parse check type - if not found in Excel, auto-detect based on existing records
                    string checkType;
                    if (!string.IsNullOrWhiteSpace(checkTypeStr))
                    {
                        checkType = checkTypeStr.ToUpper().Replace(" ", "_");
                        if (checkType != "CHECK_IN" && checkType != "CHECK_OUT")
                        {
                            checkType = "CHECK_IN"; // Default
                        }
                    }
                    else
                    {
                        // Auto-detect: Check if this email already has a raw record for this event
                        var existingRaws = await _unitOfWork.AttendanceRawRecords.GetByEventAsync(eventId);
                        var existingRawForEmail = existingRaws.FirstOrDefault(r => 
                            !string.IsNullOrWhiteSpace(r.Email) && 
                            r.Email.ToLower() == email?.ToLower() && 
                            r.SubEventId == subEventId);

                        if (existingRawForEmail != null)
                        {
                            // If already has CHECK_IN, this is likely CHECK_OUT
                            if (existingRawForEmail.CheckType == "CHECK_IN")
                            {
                                checkType = "CHECK_OUT";
                            }
                            else
                            {
                                checkType = "CHECK_IN"; // Default
                            }
                        }
                        else if (user != null)
                        {
                            // Check EventAttendance for existing check-in
                            var existingAttendance = await _unitOfWork.EventAttendances.GetByEventAsync(eventId);
                            var hasCheckIn = existingAttendance.Any(a => a.UserId == user.UserId && a.CheckinAt.HasValue && !a.CheckoutAt.HasValue);
                            checkType = hasCheckIn ? "CHECK_OUT" : "CHECK_IN";
                        }
                        else
                        {
                            checkType = "CHECK_IN"; // Default for new records
                        }
                    }
                    {
                        // Check if this email already has a raw record for this event
                        var existingRaws = await _unitOfWork.AttendanceRawRecords.GetByEventAsync(eventId);
                        var existingRawForEmail = existingRaws.FirstOrDefault(r => 
                            !string.IsNullOrWhiteSpace(r.Email) && 
                            r.Email.ToLower() == email?.ToLower() && 
                            r.SubEventId == subEventId);

                        if (existingRawForEmail != null)
                        {
                            // If already has CHECK_IN, this is likely CHECK_OUT
                            if (existingRawForEmail.CheckType == "CHECK_IN")
                            {
                                checkType = "CHECK_OUT";
                            }
                            else
                            {
                                checkType = "CHECK_IN"; // Default
                            }
                        }
                        else if (user != null)
                        {
                            // Check EventAttendance for existing check-in
                            var existingAttendance = await _unitOfWork.EventAttendances.GetByEventAsync(eventId);
                            var hasCheckIn = existingAttendance.Any(a => a.UserId == user.UserId && a.CheckinAt.HasValue && !a.CheckoutAt.HasValue);
                            checkType = hasCheckIn ? "CHECK_OUT" : "CHECK_IN";
                        }
                        else
                        {
                            checkType = "CHECK_IN"; // Default for new records
                        }
                    }

                    // Check if record already exists (by email and event, not by row)
                    AttendanceRawRecord? existingRaw = null;
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var existingRaws = await _unitOfWork.AttendanceRawRecords.GetByEventAsync(eventId);
                        existingRaw = existingRaws.FirstOrDefault(r => 
                            !string.IsNullOrWhiteSpace(r.Email) &&
                            r.Email.ToLower() == email.ToLower() && 
                            r.SubEventId == subEventId);
                    }

                    if (existingRaw != null)
                    {
                        // Update existing record
                        existingRaw.Email = email;
                        existingRaw.FullName = fullName;
                        existingRaw.CheckType = checkType;
                        existingRaw.SubmittedAt = submittedAt;
                        existingRaw.SyncedAt = syncedAt;
                        existingRaw.SheetRow = row; // Update row number
                        _unitOfWork.AttendanceRawRecords.UpdateAsync(existingRaw);
                        updatedRecords++;
                    }
                    else
                    {

                        // Create new raw record
                        var rawRecord = new AttendanceRawRecord
                        {
                            EventId = eventId,
                            SubEventId = subEventId,
                            Email = email,
                            FullName = fullName,
                            SheetRow = row,
                            CheckType = checkType,
                            SubmittedAt = submittedAt,
                            IsGuest = user == null,
                            UserId = user?.UserId,
                            SyncedAt = syncedAt
                        };

                        await _unitOfWork.AttendanceRawRecords.AddAsync(rawRecord);

                        // Create sync log
                        var syncLog = new AttendanceSyncLog
                        {
                            EventId = eventId,
                            SubEventId = subEventId,
                            ParticipantKey = email ?? fullName ?? "",
                            FullName = fullName ?? "",
                            CheckType = checkType,
                            FormSubmittedAt = submittedAt,
                            SheetRowIndex = row,
                            SyncedAt = syncedAt,
                            Source = "EXCEL_FILE",
                            ExcelFileUrl = fileUrl
                        };

                        await _unitOfWork.AttendanceSyncLogs.AddAsync(syncLog);

                        // If user exists, create/update EventAttendance
                        if (user != null)
                        {
                            var existingAttendance = await _unitOfWork.EventAttendances.GetByEventAsync(eventId);
                            var attendance = existingAttendance.FirstOrDefault(a => a.UserId == user.UserId);

                            if (attendance == null)
                            {
                                attendance = new EventAttendance
                                {
                                    EventId = eventId,
                                    UserId = user.UserId,
                                    Method = "EXCEL_SYNC"
                                };
                                await _unitOfWork.EventAttendances.AddAsync(attendance);
                            }

                            // Update check-in/check-out times based on check type
                            if (checkType == "CHECK_IN" && !attendance.CheckinAt.HasValue)
                            {
                                attendance.CheckinAt = submittedAt;
                            }
                            else if (checkType == "CHECK_OUT")
                            {
                                attendance.CheckoutAt = submittedAt;
                            }
                        }

                        newRecords++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return new SyncAttendanceResponseDto
                {
                    Success = true,
                    Message = $"Successfully synced {newRecords} new records and updated {updatedRecords} existing records.",
                    NewRecordsCount = newRecords,
                    UpdatedRecordsCount = updatedRecords,
                    TotalRecordsCount = newRecords + updatedRecords,
                    SyncedAt = syncedAt
                };
            }
            catch (Exception ex)
            {
                return new SyncAttendanceResponseDto
                {
                    Success = false,
                    Message = $"Error processing Excel file: {ex.Message}",
                    NewRecordsCount = 0,
                    UpdatedRecordsCount = 0,
                    TotalRecordsCount = 0,
                    SyncedAt = DateTime.Now
                };
            }
        }

        public async Task<SyncAttendanceResponseDto> RefreshSyncAsync(int eventId, int? subEventId = null)
        {
            // Get the latest sync log to determine source
            var syncLogs = subEventId.HasValue
                ? await _unitOfWork.AttendanceSyncLogs.GetBySubEventAsync(subEventId.Value)
                : await _unitOfWork.AttendanceSyncLogs.GetByEventAsync(eventId);

            if (!syncLogs.Any())
            {
                return new SyncAttendanceResponseDto
                {
                    Success = false,
                    Message = "No sync configuration found. Please upload Excel file or configure Google Sheet first.",
                    NewRecordsCount = 0,
                    UpdatedRecordsCount = 0,
                    TotalRecordsCount = 0,
                    SyncedAt = DateTime.Now
                };
            }

            var latestSync = syncLogs.OrderByDescending(s => s.SyncedAt).First();

            if (latestSync.Source == "EXCEL_FILE" && !string.IsNullOrEmpty(latestSync.ExcelFileUrl))
            {
                return await SyncFromExcelFileAsync(eventId, subEventId, latestSync.ExcelFileUrl);
            }
            else if (latestSync.Source == "GOOGLE_FORM" && !string.IsNullOrEmpty(latestSync.GoogleSheetId))
            {
                return await SyncFromGoogleSheetAsync(new SyncAttendanceRequestDto
                {
                    EventId = eventId,
                    SubEventId = subEventId,
                    GoogleSheetId = latestSync.GoogleSheetId
                });
            }

            return new SyncAttendanceResponseDto
            {
                Success = false,
                Message = "Invalid sync configuration.",
                NewRecordsCount = 0,
                UpdatedRecordsCount = 0,
                TotalRecordsCount = 0,
                SyncedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Unified method to sync attendance data from either Excel file or Google Sheet.
        /// Priority: Excel file > Google Sheet
        /// If Google Sheet sync fails, returns error message requesting Excel file upload.
        /// </summary>
        public async Task<SyncAttendanceResponseDto> SyncAttendanceUnifiedAsync(
            SyncAttendanceUnifiedRequestDto request, 
            string? excelFileUrl = null)
        {
            // Priority 1: If Excel file URL is provided, use Excel sync
            if (!string.IsNullOrWhiteSpace(excelFileUrl))
            {
                return await SyncFromExcelFileAsync(request.EventId, request.SubEventId, excelFileUrl);
            }

            // Priority 2: If Google Sheet ID is provided, try Google Sheet sync
            if (!string.IsNullOrWhiteSpace(request.GoogleSheetId))
            {
                try
                {
                    var googleRequest = new SyncAttendanceRequestDto
                    {
                        EventId = request.EventId,
                        SubEventId = request.SubEventId,
                        GoogleSheetId = request.GoogleSheetId
                    };

                    return await SyncFromGoogleSheetAsync(googleRequest);
                }
                catch (NotImplementedException)
                {
                    // Google Sheets API is not implemented or not available
                    return new SyncAttendanceResponseDto
                    {
                        Success = false,
                        Message = "Không thể đồng bộ dữ liệu từ Google Sheet. " +
                                 "Vui lòng tải Google Sheet về máy dưới dạng file Excel và upload file Excel thay thế. " +
                                 "Sử dụng endpoint này với file Excel để đồng bộ dữ liệu.",
                        NewRecordsCount = 0,
                        UpdatedRecordsCount = 0,
                        TotalRecordsCount = 0,
                        SyncedAt = DateTime.Now
                    };
                }
                catch (Exception ex)
                {
                    // Other errors from Google Sheets API
                    return new SyncAttendanceResponseDto
                    {
                        Success = false,
                        Message = $"Lỗi khi đồng bộ từ Google Sheet: {ex.Message}. " +
                                 "Vui lòng tải Google Sheet về máy dưới dạng file Excel và upload file Excel thay thế.",
                        NewRecordsCount = 0,
                        UpdatedRecordsCount = 0,
                        TotalRecordsCount = 0,
                        SyncedAt = DateTime.Now
                    };
                }
            }

            // No valid input provided
            return new SyncAttendanceResponseDto
            {
                Success = false,
                Message = "Vui lòng cung cấp file Excel hoặc Google Sheet ID để đồng bộ dữ liệu.",
                NewRecordsCount = 0,
                UpdatedRecordsCount = 0,
                TotalRecordsCount = 0,
                SyncedAt = DateTime.Now
            };
        }
    }
}

