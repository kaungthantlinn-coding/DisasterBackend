using System;
using System.Collections.Generic;

namespace DisasterApp.Infrastructure.Data;

public partial class Role
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
