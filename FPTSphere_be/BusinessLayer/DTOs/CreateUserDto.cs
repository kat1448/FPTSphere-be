using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Department { get; set; }
    }
}
