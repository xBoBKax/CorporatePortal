namespace CorporatePortal.Api.Models
{
    public class PortalModule
    {
        public int Id { get; set; }

        public int PortalPageId { get; set; }

        public string Type { get; set; } = "Text";

        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? SettingsJson { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public PortalPage? PortalPage { get; set; }
    }
}
