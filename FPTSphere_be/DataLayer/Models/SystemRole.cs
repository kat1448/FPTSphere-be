using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class SystemRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
