using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs.Event
{
    public class EventFilterDto
    {
        public int? StatusId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? LocationId { get; set; }

        public int? ExternalLocationId { get; set; }

        public int? CreatedBy { get; set; }

        public int? MinAttendees { get; set; }

        public int? MaxAttendees { get; set; }

        public decimal? MinCost { get; set; }

        public decimal? MaxCost { get; set; }

        public bool IncludeDeleted { get; set; } = false;   
        public bool OnlyMainEvents { get; set; } = false; 
        public bool OnlySubEvents { get; set; } = false;
    }
}
