using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

[Table("AuditLog")]
public partial class AuditLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("UserID")]
    public string UserId { get; set; }

    [StringLength(20)]
    public string Action { get; set; } = null!;

    [StringLength(100)]
    public string TableName { get; set; } = null!;

    [Column("RecordID")]
    public int RecordId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PerformedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    // Added '?' here so the API doesn't require the full User object in the JSON
    public virtual User? User { get; set; }
}