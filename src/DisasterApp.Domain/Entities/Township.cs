using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class Township
{
    public Guid TownshipId { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
