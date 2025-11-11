using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventResource
{
    [Key]
    [Column("event_resource_id")]
    public int EventResourceId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("resource_id")]
    public int ResourceId { get; set; }

    [Column("quantity_used")]
    public int QuantityUsed { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventResources")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("ResourceId")]
    [InverseProperty("EventResources")]
    public virtual Resource Resource { get; set; } = null!;
}
