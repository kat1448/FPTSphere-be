using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class Location
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("capacity")]
    public int? Capacity { get; set; }

    [Column("building")]
    [StringLength(100)]
    public string? Building { get; set; }

    [Column("room_number")]
    [StringLength(50)]
    public string? RoomNumber { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("image_url")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [InverseProperty("Location")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
