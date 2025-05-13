using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public partial class Venue
{
    public int VenueId { get; set; }

    [Required(ErrorMessage = "Venue name is required")]
    public string VenueName { get; set; } = null!;

    [Required(ErrorMessage = "Location is required")]
    public string Location { get; set; } = null!;

    public int Capacity { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
