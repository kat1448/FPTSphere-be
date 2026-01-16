using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.EventTask
{
    public class UpdateTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Not Started|In Progress|Completed)$", 
            ErrorMessage = "Status must be one of: Not Started, In Progress, Completed")]
        public string Status { get; set; } = null!;
    }
    public class UpdateTaskReportDto
    {
        public string Report { get; set; }
    }
}
