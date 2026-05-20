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
        public string? ThemeCss { get; set; }
        public List<WidgetConfig> Widgets { get; set; } = new();
        public List<LinkGroup> LinkGroups { get; set; } = new();
    }

    public class WidgetConfig
    {
        public int Id { get; set; }
        public string Type { get; set; } = "clock";
        public string Title { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? EmbedHtml { get; set; }
    }
}
