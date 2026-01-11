using BusinessLayer.DTOs;
using BusinessLayer.DTOs.File;
using BusinessLayer.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly CloudinarySettings _cloudinarySettings;
        private readonly Cloudinary _cloudinary;

        public FileService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings.Value;

            // Initialize Cloudinary account
            var account = new Account(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, string? folder = null, string? transformation = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required and cannot be empty");

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

            try
            {
                // Convert IFormFile to byte array
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var bytes = stream.ToArray();

                var resourceType = GetResourceType(fileExtension);
                FileUploadResponseDto result;

                if (resourceType == "raw")
                {
                    // Upload as raw file (PDF, DOC, etc.)
                    var rawUploadParams = new RawUploadParams()
                    {
                        File = new FileDescription(file.FileName, new MemoryStream(bytes)),
                        Folder = folder,
                        Overwrite = false
                    };

                    var rawUploadResult = await _cloudinary.UploadAsync(rawUploadParams);

                    if (rawUploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception($"Failed to upload file to Cloudinary: {rawUploadResult.Error?.Message}");

                    result = new FileUploadResponseDto
                    {
                        PublicId = rawUploadResult.PublicId,
                        Url = rawUploadResult.Url?.ToString() ?? string.Empty,
                        SecureUrl = rawUploadResult.SecureUrl?.ToString() ?? string.Empty,
                        Format = rawUploadResult.Format ?? string.Empty,
                        Width = 0,
                        Height = 0,
                        Bytes = rawUploadResult.Bytes,
                        ResourceType = "raw"
                    };
                }
                else
                {
                    // Upload as image
                    var imageUploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, new MemoryStream(bytes)),
                        Folder = folder,
                        Overwrite = false
                    };

                    if (!string.IsNullOrWhiteSpace(transformation))
                    {
                        imageUploadParams.Transformation = new Transformation().RawTransformation(transformation);
                    }

                    var imageUploadResult = await _cloudinary.UploadAsync(imageUploadParams);

                    if (imageUploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception($"Failed to upload file to Cloudinary: {imageUploadResult.Error?.Message}");

                    result = new FileUploadResponseDto
                    {
                        PublicId = imageUploadResult.PublicId,
                        Url = imageUploadResult.Url?.ToString() ?? string.Empty,
                        SecureUrl = imageUploadResult.SecureUrl?.ToString() ?? string.Empty,
                        Format = imageUploadResult.Format ?? string.Empty,
                        Width = imageUploadResult.Width,
                        Height = imageUploadResult.Height,
                        Bytes = imageUploadResult.Bytes,
                        ResourceType = "image"
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("Public ID is required");

            try
            {
                // Try to delete as image first
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                // If not found as image, try as raw
                if (result.Result != "ok")
                {
                    deleteParams.ResourceType = ResourceType.Raw;
                    result = await _cloudinary.DestroyAsync(deleteParams);
                }

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file: {ex.Message}", ex);
            }
        }

        public string GetFileUrl(string publicId, string? transformation = null)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("Public ID is required");

            try
            {
                var url = _cloudinary.Api.UrlImgUp;
                
                if (!string.IsNullOrWhiteSpace(transformation))
                {
                    url = url.Transform(new Transformation().RawTransformation(transformation));
                }

                return url.BuildUrl(publicId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating file URL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determine resource type based on file extension
        /// </summary>
        private string GetResourceType(string fileExtension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            return imageExtensions.Contains(fileExtension.ToLowerInvariant()) ? "image" : "raw";
        }
    }
}

