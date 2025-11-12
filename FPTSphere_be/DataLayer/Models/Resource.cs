using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class Resource
{
    [Key]
    [Column("resource_id")]
    public int ResourceId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("type")]
    [StringLength(100)]
    public string? Type { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("image_url")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [InverseProperty("Resource")]
    public virtual ICollection<EventResource> EventResources { get; set; } = new List<EventResource>();
}
