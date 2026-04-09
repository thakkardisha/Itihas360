using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Itihas360.Models;

[Table("Article")]
[Index("Slug", Name = "UQ__Article__BC7B5FB6D0298491", IsUnique = true)]
public partial class Article
{
    [Key]
    [Column("ArticleID")]
    public int ArticleId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(150)]
    public string Slug { get; set; } = null!;

    [Column("SectorID")]
    public int SectorId { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    [StringLength(500)]
    public string ShortBio { get; set; } = null!;

    public string DetailedBio { get; set; } = null!;

    [StringLength(160)]
    public string? MetaTitle { get; set; }

    [StringLength(320)]
    public string? MetaDescription { get; set; }

    public int? ViewCount { get; set; }

    public bool? IsPublished { get; set; }

    public bool? IsDeleted { get; set; }

    [Column("CreatedBy")]
    public string? CreatedBy { get; set; }

    [Column("UpdatedBy")]
    public string? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    // ✅ FIX: make nullable + remove = null!
    [JsonIgnore]
    [ForeignKey("CreatedBy")]
    //[InverseProperty("ArticleCreatedByNavigations")]
    public virtual Microsoft.AspNetCore.Identity.IdentityUser? CreatedByNavigation { get; set; }

    [JsonIgnore]
    [InverseProperty("Personality")]
    public virtual ICollection<Mcqquestion> Mcqquestions { get; set; } = new List<Mcqquestion>();

    [JsonIgnore]
    [ForeignKey("SectorId")]
    [InverseProperty("Articles")]
    public virtual Category? Sector { get; set; }

    [JsonIgnore]
    [ForeignKey("UpdatedBy")]
    //[InverseProperty("ArticleUpdatedByNavigations")]
    public virtual Microsoft.AspNetCore.Identity.IdentityUser? UpdatedByNavigation { get; set; }
}