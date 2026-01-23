using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Event
{
    public class ChangeEventStatusDto
    {
        /// <summary>
        /// StatusId to change event status
        /// Allowed transitions:
        /// - Approved (3) -> In Progress (4) - by Event Manager or Director
        /// - In Progress (4) -> Completed (5) - by Event Manager or Director
        /// </summary>
        [Required(ErrorMessage = "StatusId is required")]
        [Range(1, 10, ErrorMessage = "StatusId must be between 1 and 10")]
        public int StatusId { get; set; }
    }
}
