using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Itihas360.Models;

[Index("Email", Name = "UQ__Users__A9D1053457E742A1", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public string UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    // Role: 1 for Admin, 2 for User (As per your DB design)
    public byte? Role { get; set; }

    public string? CreatedBy { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties ko JSON ignore kiya hai taaki API response clean rahe
    //[JsonIgnore]
    //[InverseProperty("CreatedByNavigation")]
    //public virtual ICollection<Article> ArticleCreatedByNavigations { get; set; } = new List<Article>();

    //[JsonIgnore]
    //[InverseProperty("UpdatedByNavigation")]
    //public virtual ICollection<Article> ArticleUpdatedByNavigations { get; set; } = new List<Article>();

    [JsonIgnore]
    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [JsonIgnore]
    [ForeignKey("CreatedBy")]
    [InverseProperty("InverseCreatedByNavigation")]
    public virtual User? CreatedByNavigation { get; set; }

    [JsonIgnore]
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    [JsonIgnore]
    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Mcqquestion> Mcqquestions { get; set; } = new List<Mcqquestion>();
}