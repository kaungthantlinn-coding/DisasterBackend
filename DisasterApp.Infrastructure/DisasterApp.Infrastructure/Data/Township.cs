using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.DisasterApp.Infrastructure.Data;

public partial class Township
{
    public Guid TownshipId { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
