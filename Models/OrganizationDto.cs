namespace Itihas360.Models
{
    public class OrganizationDto
    {
        public int OrganizationId { get; set; }
        public string? OrganizationPhoto { get; set; }  // No length limit — accepts Base64
        public string OrganizationName { get; set; } = null!;
        public long? Mobile { get; set; }
        public long AlterMobile { get; set; }
        public string Email { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string Instagram { get; set; } = null!;
        public string Facebook { get; set; } = null!;
        public string LinkedIn { get; set; } = null!;
        public string X { get; set; } = null!;
    }
}