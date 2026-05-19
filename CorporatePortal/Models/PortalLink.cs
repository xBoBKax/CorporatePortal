namespace CorporatePortal.Api.Models
{
    public class PortalLink
    {
        public int Id { get; set; }

        public int LinkGroupId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool OpenInNewTab { get; set; } = true;

        public LinkGroup? LinkGroup { get; set; }
    }
}