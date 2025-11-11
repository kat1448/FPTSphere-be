using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class ExternalLocation
{
    [Key]
    [Column("external_location_id")]
    public int ExternalLocationId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; } = null!;

    [Column("contact_person")]
    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [Column("contact_phone")]
    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [Column("cost", TypeName = "decimal(10, 2)")]
    public decimal? Cost { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("image_url")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [InverseProperty("ExternalLocation")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
