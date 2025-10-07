using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class OauthLogin
{
    public int OauthId { get; set; }

    public int? UserId { get; set; }

    public string? Provider { get; set; }

    public string? ProviderUid { get; set; }

    public virtual User? User { get; set; }
}
