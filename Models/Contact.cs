using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("Contact")]
[Index("Mobile", Name = "UQ__Contact__6FAE0782EA21640D", IsUnique = true)]
[Index("Email", Name = "UQ__Contact__A9D10534FB76540E", IsUnique = true)]
public partial class Contact
{
    [Key]
    [Column("Contact_ID")]
    public int ContactId { get; set; }

    [Column("Full_Name")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    public long? Mobile { get; set; }

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(200)]
    public string? Subject { get; set; }

    [StringLength(500)]
    public string Message { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? InquiryAt { get; set; }
}
