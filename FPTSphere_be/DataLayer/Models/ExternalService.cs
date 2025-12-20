using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class ExternalService
{
    [Key]
    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("provider_name")]
    [StringLength(255)]
    public string ProviderName { get; set; } = null!;

    [Column("resource_type")]
    [StringLength(100)]
    public string? ResourceType { get; set; }

    [Column("cost", TypeName = "decimal(10, 2)")]
    public decimal? Cost { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("ExternalServices")]
    public virtual Event Event { get; set; } = null!;
}
