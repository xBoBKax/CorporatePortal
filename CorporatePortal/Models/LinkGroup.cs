namespace CorporatePortal.Api.Models
{
    public class LinkGroup
    {
        public int Id { get; set; }

        public int PortalPageId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public PortalPage? PortalPage { get; set; }

        public List<PortalLink> Links { get; set; } = new List<PortalLink>();
    }
}