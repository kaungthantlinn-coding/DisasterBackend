using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.DisasterApp.Infrastructure.Data;

public partial class DisasterReport
{
    public Guid ReportId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public string Severity { get; set; } = null!;

    public string Status { get; set; } = null!;

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid UserId { get; set; }

    public Guid DisasterTypeId { get; set; }

    public virtual DisasterType DisasterType { get; set; } = null!;

    public virtual ICollection<ImpactDetail> ImpactDetails { get; set; } = new List<ImpactDetail>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }
}
