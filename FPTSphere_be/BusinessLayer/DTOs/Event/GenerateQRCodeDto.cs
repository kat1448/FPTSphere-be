using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Event
{
    /// <summary>
    /// Request DTO for generating QR code for sub-event registration form
    /// </summary>
    public class GenerateQRCodeDto
    {
        /// <summary>
        /// Google Form URL that QR code will point to (provided by frontend)
        /// This is required - frontend must create Google Form first and provide the URL
        /// </summary>
        [Required(ErrorMessage = "Google Form URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string GoogleFormUrl { get; set; } = null!;
    }

    /// <summary>
    /// Response DTO for QR code generation
    /// </summary>
    public class GenerateQRCodeResponseDto
    {
        public int SubEventId { get; set; }
        public string QrCodeUrl { get; set; } = null!;
        public string QrCodeBase64 { get; set; } = null!;
        public string GoogleFormUrl { get; set; } = null!;
    }
}

