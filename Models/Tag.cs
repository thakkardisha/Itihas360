using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Index("TagName", Name = "UQ__Tags__BDE0FD1D94E207CC", IsUnique = true)]
[Index("TagSlug", Name = "UQ__Tags__F686A907F6716F7B", IsUnique = true)]
public partial class Tag
{
    [Key]
    [Column("TagID")]
    public int TagId { get; set; }

    [StringLength(80)]
    public string TagName { get; set; } = null!;

    [StringLength(80)]
    public string TagSlug { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
