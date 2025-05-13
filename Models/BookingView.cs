using System;
using System.Collections.Generic;

namespace EventEase.Models;

public partial class BookingView
{
    public int BookingId { get; set; }

    public string VenueName { get; set; } = null!;
    
    public string EventName { get; set; } = null!;

    public DateOnly BookingDate { get; set; }
}
