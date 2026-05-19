namespace CorporatePortal.Api.Models
{
    public class PortalPage
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string WelcomeText { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        public string BackgroundType { get; set; } = "color";

        public string? BackgroundValue { get; set; }

        public bool IsActive { get; set; } = true;

        public List<LinkGroup> LinkGroups { get; set; } = new List<LinkGroup>();
    }
}