using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventRegistration
{
    public int RegistrationId { get; set; }

    public int? EventId { get; set; }

    public int? UserId { get; set; }

    public DateTime? CheckinTime { get; set; }

    public DateTime? CheckoutTime { get; set; }

    public virtual Event? Event { get; set; }

    public virtual User? User { get; set; }
}
