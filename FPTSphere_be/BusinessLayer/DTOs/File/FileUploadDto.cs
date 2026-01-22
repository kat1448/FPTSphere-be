using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.DTOs.File
{
    /// <summary>
    /// Response DTO for file upload
    /// </summary>
    public class FileUploadResponseDto
    {
        public string PublicId { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string SecureUrl { get; set; } = null!;
        public string Format { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public long Bytes { get; set; }
        public string ResourceType { get; set; } = null!;
    }

    public class UploadFileWithSettingsDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public string? Folder { get; set; }

        public string? Transformation { get; set; }
    }
    public class UploadMultipleFilesDto
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new();
        public string? Folder { get; set; }
    }
}

