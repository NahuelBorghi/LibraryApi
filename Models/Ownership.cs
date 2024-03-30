using System;
using System.Collections.Generic;

namespace LibraryApi.Models;

public partial class Ownership
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BookId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
