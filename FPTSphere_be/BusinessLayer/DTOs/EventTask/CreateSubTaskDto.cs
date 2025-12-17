using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.EventTask
{
    public class CreateSubTaskDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int AssignedTo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
