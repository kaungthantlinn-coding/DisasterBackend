using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.Data;

public partial class DisasterType
{
    public Guid DisasterTypeId { get; set; }

    public string Category { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();
}
