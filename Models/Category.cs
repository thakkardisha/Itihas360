using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Itihas360.Models;

[Table("Category")]
[Index("CategoryName", Name = "UQ__Category__8517B2E0DA134BD6", IsUnique = true)]
[Index("CategorySlug", Name = "UQ__Category__BDD4F1B29FCF9AD6", IsUnique = true)]
public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; }

    [StringLength(100)]
    public string CategorySlug { get; set; }

    public string? Description { get; set; }

    public byte? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    // ✅ Ignore navigation properties
    [JsonIgnore]
    [InverseProperty("Sector")]
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    [JsonIgnore]
    [InverseProperty("Sector")]
    public virtual ICollection<Mcqquestion> Mcqquestions { get; set; } = new List<Mcqquestion>();

    [JsonIgnore]
    [InverseProperty("RelatedSector")]
    public virtual ICollection<NewsFeedCache> NewsFeedCaches { get; set; } = new List<NewsFeedCache>();
}