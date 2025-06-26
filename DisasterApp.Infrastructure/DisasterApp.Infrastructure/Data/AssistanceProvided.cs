using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.DisasterApp.Infrastructure.Data;

public partial class AssistanceProvided
{
    public Guid AssistanceId { get; set; }

    public Guid RequestId { get; set; }

    public DateTime? ProvidedAt { get; set; }

    public string Status { get; set; } = null!;

    public Guid ProviderId { get; set; }

    public virtual User Provider { get; set; } = null!;

    public virtual SupportRequest Request { get; set; } = null!;
}
