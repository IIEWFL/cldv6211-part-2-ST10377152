using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public partial class Event
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    public string EventName { get; set; } = null!;

    [Required(ErrorMessage = "Event date is required")]
    public string EventDate { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}


