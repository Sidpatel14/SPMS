using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Notification
{
    public long NotificationId { get; set; }

    public long UserId { get; set; }

    public long? ApplicationId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public virtual Application? Application { get; set; }

    public virtual User User { get; set; } = null!;
}
