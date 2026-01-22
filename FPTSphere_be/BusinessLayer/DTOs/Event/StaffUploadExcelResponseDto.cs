using System.Collections.Generic;

namespace BusinessLayer.DTOs.Event
{
    public class StaffUploadExcelResponseDto
    {
        public string ExcelFileUrl { get; set; } = string.Empty;
        public string ExcelFilePublicId { get; set; } = string.Empty;
        public List<string> ExtractedEmails { get; set; } = new List<string>();
        public int EmailCount { get; set; }
    }
}

