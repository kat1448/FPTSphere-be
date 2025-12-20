using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventInvitation
{
    [Key]
    [Column("invitation_id")]
    public int InvitationId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("class_code")]
    [StringLength(50)]
    public string ClassCode { get; set; } = null!;

    [Column("sent_by")]
    public int SentBy { get; set; }

    [Column("sent_at", TypeName = "datetime")]
    public DateTime? SentAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventInvitations")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("SentBy")]
    [InverseProperty("EventInvitations")]
    public virtual User SentByNavigation { get; set; } = null!;
}
