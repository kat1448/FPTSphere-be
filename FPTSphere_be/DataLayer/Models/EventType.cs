using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Models;

public partial class EventType
{
    [Key]
    [Column("type_id")]
    public int TypeId { get; set; }

    [Column("type_name")]
    [StringLength(100)]
    public string TypeName { get; set; } = null!;

    [InverseProperty("Type")]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}

