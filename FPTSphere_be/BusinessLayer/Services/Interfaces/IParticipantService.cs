using System.Threading.Tasks;
using BusinessLayer.DTOs.Event;

namespace BusinessLayer.Services.Interfaces
{
    public interface IParticipantService
    {
        Task<ParticipantListResponseDto> GetParticipantsByEventAsync(int eventId, int? subEventId = null, string? searchTerm = null, string? statusFilter = null);
        Task<SyncAttendanceResponseDto> SyncFromGoogleSheetAsync(SyncAttendanceRequestDto request);
        Task<SyncAttendanceResponseDto> SyncFromExcelFileAsync(int eventId, int? subEventId, string fileUrl);
        Task<SyncAttendanceResponseDto> RefreshSyncAsync(int eventId, int? subEventId = null);
        Task<SyncAttendanceResponseDto> SyncAttendanceUnifiedAsync(SyncAttendanceUnifiedRequestDto request, string? excelFileUrl = null);
    }
}

