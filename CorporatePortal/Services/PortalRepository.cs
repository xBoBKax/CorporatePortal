namespace CorporatePortal.Api.Services
{
    public class PortalRepository
    {
        private readonly List<PortalWidget> _widgets = new()
        {
            new PortalWidget
            {
                Title = "Компания TV",
                EmbedHtml = "<iframe src=\"https://example.com\" width=\"100%\" height=\"180\"></iframe>"
            }
        };

        public IReadOnlyList<PortalWidget> GetWidgets()
        {
            return _widgets;
        }
    }

    public class PortalWidget
    {
        public string Title { get; set; } = string.Empty;

        public string EmbedHtml { get; set; } = string.Empty;
    }
}
