using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class GoogleUserInfo
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string GoogleId { get; set; } = null!;
        public string? Picture { get; set; }
    }
}
