using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itihas360.Models
{
    [Table("EmailTemplates")]
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string TemplateName { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string DefaultSubject { get; set; } = null!;

        [Required]
        public string HtmlBody { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;
    }
}