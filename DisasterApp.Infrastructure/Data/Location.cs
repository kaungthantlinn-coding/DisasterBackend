using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.Data;

public partial class Location
{
    public Guid LocationId { get; set; }

    public Guid ReportId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string Address { get; set; } = null!;

    public string? GooglePlaceId { get; set; }

    public Guid? TownshipId { get; set; }

    public virtual DisasterReport Report { get; set; } = null!;

    public virtual Township? Township { get; set; }
}
