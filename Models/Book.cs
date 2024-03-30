using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public partial class Book
{
    public Guid Id { get; set; }
    [Display(Name = "Titulo")]
    public string Title { get; set; } = null!;
    [Display(Name = "Autor")]
    public string Author { get; set; } = null!;

    public virtual ICollection<Ownership> Ownerships { get; set; } = new List<Ownership>();
}
