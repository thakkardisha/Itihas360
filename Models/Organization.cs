using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("Organization")]
[Index("Mobile", Name = "UQ__Organiza__6FAE0782A3AFDBF4", IsUnique = true)]
[Index("Email", Name = "UQ__Organiza__A9D105341909027A", IsUnique = true)]
public partial class Organization
{
    [Key]
    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    [Column("Organization_Photo")]
    [StringLength(200)]
    public string OrganizationPhoto { get; set; } = null!;

    [Column("Organization_Name")]
    [StringLength(100)]
    public string OrganizationName { get; set; } = null!;

    public long? Mobile { get; set; }

    [Column("Alter_Mobile")]
    public long AlterMobile { get; set; }

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("Company_Address")]
    [StringLength(500)]
    public string? CompanyAddress { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? State { get; set; }

    [StringLength(255)]
    public string Instagram { get; set; } = null!;

    [StringLength(255)]
    public string Facebook { get; set; } = null!;

    [StringLength(255)]
    public string LinkedIn { get; set; } = null!;

    [StringLength(255)]
    public string X { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
