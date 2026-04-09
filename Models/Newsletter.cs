using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("Newsletter")]
public partial class Newsletter
{
    [Key]
    [Column("Subscriber_ID")]
    public int SubscriberId { get; set; }

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? SubscribedWhen { get; set; }
}
