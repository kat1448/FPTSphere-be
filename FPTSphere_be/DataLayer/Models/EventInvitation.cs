using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventInvitation
{
    public int InvitationId { get; set; }

    public int EventId { get; set; }

    public string ClassCode { get; set; } = null!;

    public int SentBy { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User SentByNavigation { get; set; } = null!;
}
