using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.Data;

public partial class Photo
{
    public Guid PhotoId { get; set; }

    public Guid ReportId { get; set; }

    public string Url { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual DisasterReport Report { get; set; } = null!;
}
