using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("NewsFeedCache")]
[Index("ExternalArticleId", Name = "UQ__NewsFeed__FB0EAED5AC835D1B", IsUnique = true)]
public partial class NewsFeedCache
{
    [Key]
    [Column("NewsCacheID")]
    public int NewsCacheId { get; set; }

    [Column("ExternalArticleID")]
    [StringLength(150)]
    public string? ExternalArticleId { get; set; }

    [StringLength(400)]
    public string Headline { get; set; } = null!;

    [StringLength(2000)]
    public string? Summary { get; set; }

    [StringLength(150)]
    public string? SourceName { get; set; }

    [Column("SourceURL")]
    [StringLength(1000)]
    public string? SourceUrl { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PublishedAt { get; set; }

    [Column("RelatedSectorID")]
    public int? RelatedSectorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FetchedAt { get; set; }

    public bool? IsVisible { get; set; }

    [ForeignKey("RelatedSectorId")]
    [InverseProperty("NewsFeedCaches")]
    public virtual Category? RelatedSector { get; set; }
}
