using System.Collections.Generic;

namespace Itihas360.Models
{
    public class SendCampaignModel
    {
        public int TemplateId { get; set; }
        public bool SendToAll { get; set; }
        public List<string>? TargetEmails { get; set; }
        public string? CustomNotes { get; set; }
    }
}