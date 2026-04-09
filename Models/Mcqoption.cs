using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("MCQOptions")]
public partial class Mcqoption
{
    [Key]
    [Column("OptionID")]
    public int OptionId { get; set; }

    [Column("QuestionID")]
    public int QuestionId { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string OptionLabel { get; set; } = null!;

    [StringLength(500)]
    public string OptionText { get; set; } = null!;

    public bool? IsCorrect { get; set; }

    
    [ForeignKey("QuestionId")]
    [InverseProperty("Mcqoptions")]
    public virtual Mcqquestion? Question { get; set; }
}