using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class ImpactDetail
{
    public Guid ImpactId { get; set; }

    public Guid ReportId { get; set; }

    public string ImpactType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual DisasterReport Report { get; set; } = null!;
}
