using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.DisasterApp.Infrastructure.Data;

public partial class Chat
{
    public Guid ChatId { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
