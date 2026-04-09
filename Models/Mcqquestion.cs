using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // ⭐ ADD THIS

namespace Itihas360.Models;

[Table("MCQQuestions")]
public partial class Mcqquestion
{
    [Key]
    [Column("QuestionID")]
    public int QuestionId { get; set; }

    [StringLength(1000)]
    public string QuestionText { get; set; } = null!;

    [Column("PersonalityID")]
    public int? PersonalityId { get; set; }

    [Column("SectorID")]
    public int? SectorId { get; set; }

    public byte? DifficultyLevel { get; set; }

    [StringLength(1000)]
    public string? ExplanationText { get; set; }

    public bool? IsActive { get; set; }

    public string CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    // ⭐ FIX 1 (IMPORTANT)
    [ForeignKey("CreatedBy")]
    [InverseProperty("Mcqquestions")]
    [JsonIgnore]
    public virtual User? CreatedByNavigation { get; set; }

    // ⭐ FIX 2
    [InverseProperty("Question")]
    [JsonIgnore]
    public virtual ICollection<Mcqoption> Mcqoptions { get; set; } = new List<Mcqoption>();

    // ⭐ FIX 3
    [ForeignKey("PersonalityId")]
    [InverseProperty("Mcqquestions")]
    [JsonIgnore]
    public virtual Article? Personality { get; set; }

    // ⭐ FIX 4
    [ForeignKey("SectorId")]
    [InverseProperty("Mcqquestions")]
    [JsonIgnore]
    public virtual Category? Sector { get; set; }
}