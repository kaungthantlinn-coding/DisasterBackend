using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class SupportRequest
{
    public Guid RequestId { get; set; }

    public Guid ReportId { get; set; }

    public string SupportType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? QuantityNeeded { get; set; }

    public string Urgency { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual ICollection<AssistanceProvided> AssistanceProvideds { get; set; } = new List<AssistanceProvided>();

    public virtual DisasterReport Report { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
