using BusinessLayer.DTOs.File;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Upload file to Cloudinary
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="folder">Optional folder path in Cloudinary</param>
        /// <param name="transformation">Optional transformation string</param>
        /// <returns>File upload response with Cloudinary URL</returns>
        Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, string? folder = null, string? transformation = null);

        /// <summary>
        /// Delete file from Cloudinary by public ID
        /// </summary>
        /// <param name="publicId">Public ID of the file in Cloudinary</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string publicId);

        /// <summary>
        /// Get file URL from Cloudinary public ID
        /// </summary>
        /// <param name="publicId">Public ID of the file</param>
        /// <param name="transformation">Optional transformation string</param>
        /// <returns>File URL</returns>
        string GetFileUrl(string publicId, string? transformation = null);
    }
}

