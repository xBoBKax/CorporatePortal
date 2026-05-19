using CorporatePortal.Api.Models;

namespace CorporatePortal.Api.Services;

public class PortalRepository
{
    private readonly object _sync = new();
    private readonly List<PortalPage> _pages;
    private int _pageId = 2;
    private int _groupId = 3;
    private int _linkId = 6;

    public PortalRepository()
    {
        _pages =
        [
            new PortalPage
            {
                Id = 1,
                Title = "Общий портал",
                WelcomeText = "Добро пожаловать на корпоративный портал компании.",
                BackgroundType = "color",
                BackgroundValue = "#f2f4f8",
                LinkGroups =
                [
                    new LinkGroup
                    {
                        Id = 1,
                        PortalPageId = 1,
                        Title = "Корпоративные сервисы",
                        SortOrder = 1,
                        Links =
                        [
                            new PortalLink { Id = 1, LinkGroupId = 1, Title = "Почта", Url = "https://mail.example.com", SortOrder = 1 },
                            new PortalLink { Id = 2, LinkGroupId = 1, Title = "База знаний", Url = "https://wiki.example.com", SortOrder = 2 }
                        ]
                    },
                    new LinkGroup
                    {
                        Id = 2,
                        PortalPageId = 1,
                        Title = "Работа и коммуникации",
                        SortOrder = 2,
                        Links =
                        [
                            new PortalLink { Id = 3, LinkGroupId = 2, Title = "Задачи", Url = "https://tasks.example.com", SortOrder = 1 },
                            new PortalLink { Id = 4, LinkGroupId = 2, Title = "Телефонный справочник", Url = "https://phonebook.example.com", SortOrder = 2 }
                        ]
                    }
                ]
            },
            new PortalPage
            {
                Id = 2,
                Title = "IT-страница",
                WelcomeText = "Ресурсы для IT-отдела.",
                BackgroundType = "gradient",
                BackgroundValue = "linear-gradient(120deg,#0f172a,#1d4ed8)",
                LinkGroups =
                [
                    new LinkGroup
                    {
                        Id = 3,
                        PortalPageId = 2,
                        Title = "Инфраструктура",
                        SortOrder = 1,
                        Links =
                        [
                            new PortalLink { Id = 5, LinkGroupId = 3, Title = "Мониторинг", Url = "https://zabbix.example.com", SortOrder = 1 },
                            new PortalLink { Id = 6, LinkGroupId = 3, Title = "GitLab", Url = "https://gitlab.example.com", SortOrder = 2 }
                        ]
                    }
                ]
            }
        ];
    }

    public IReadOnlyList<PortalPage> GetPages()
    {
        lock (_sync)
        {
            return _pages.Select(ClonePage).ToList();
        }
    }

    public PortalPage? GetPage(int id)
    {
        lock (_sync)
        {
            var page = _pages.FirstOrDefault(x => x.Id == id);
            return page is null ? null : ClonePage(page);
        }
    }

    public PortalPage CreatePage(PortalPage request)
    {
        lock (_sync)
        {
            var page = new PortalPage
            {
                Id = ++_pageId,
                Title = request.Title,
                WelcomeText = request.WelcomeText,
                LogoUrl = request.LogoUrl,
                BackgroundType = request.BackgroundType,
                BackgroundValue = request.BackgroundValue,
                IsActive = request.IsActive
            };
            _pages.Add(page);
            return ClonePage(page);
        }
    }

    public PortalPage? UpdatePage(int id, PortalPage request)
    {
        lock (_sync)
        {
            var page = _pages.FirstOrDefault(x => x.Id == id);
            if (page is null) return null;
            page.Title = request.Title;
            page.WelcomeText = request.WelcomeText;
            page.LogoUrl = request.LogoUrl;
            page.BackgroundType = request.BackgroundType;
            page.BackgroundValue = request.BackgroundValue;
            page.IsActive = request.IsActive;
            return ClonePage(page);
        }
    }

    public bool DeletePage(int id)
    {
        lock (_sync)
        {
            var page = _pages.FirstOrDefault(x => x.Id == id);
            return page is not null && _pages.Remove(page);
        }
    }

    public LinkGroup? AddGroup(int pageId, LinkGroup group)
    {
        lock (_sync)
        {
            var page = _pages.FirstOrDefault(x => x.Id == pageId);
            if (page is null) return null;
            var item = new LinkGroup
            {
                Id = ++_groupId,
                PortalPageId = pageId,
                Title = group.Title,
                Description = group.Description,
                Icon = group.Icon,
                SortOrder = group.SortOrder,
                IsVisible = group.IsVisible
            };
            page.LinkGroups.Add(item);
            return CloneGroup(item);
        }
    }

    public PortalLink? AddLink(int pageId, int groupId, PortalLink link)
    {
        lock (_sync)
        {
            var group = _pages.FirstOrDefault(p => p.Id == pageId)?.LinkGroups.FirstOrDefault(g => g.Id == groupId);
            if (group is null) return null;
            var item = new PortalLink
            {
                Id = ++_linkId,
                LinkGroupId = groupId,
                Title = link.Title,
                Url = link.Url,
                Description = link.Description,
                Icon = link.Icon,
                SortOrder = link.SortOrder,
                IsActive = link.IsActive,
                OpenInNewTab = link.OpenInNewTab
            };
            group.Links.Add(item);
            return CloneLink(item);
        }
    }

    private static PortalPage ClonePage(PortalPage source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        WelcomeText = source.WelcomeText,
        LogoUrl = source.LogoUrl,
        BackgroundType = source.BackgroundType,
        BackgroundValue = source.BackgroundValue,
        IsActive = source.IsActive,
        LinkGroups = source.LinkGroups.Select(CloneGroup).ToList()
    };

    private static LinkGroup CloneGroup(LinkGroup source) => new()
    {
        Id = source.Id,
        PortalPageId = source.PortalPageId,
        Title = source.Title,
        Description = source.Description,
        Icon = source.Icon,
        SortOrder = source.SortOrder,
        IsVisible = source.IsVisible,
        Links = source.Links.Select(CloneLink).ToList()
    };

    private static PortalLink CloneLink(PortalLink source) => new()
    {
        Id = source.Id,
        LinkGroupId = source.LinkGroupId,
        Title = source.Title,
        Url = source.Url,
        Description = source.Description,
        Icon = source.Icon,
        SortOrder = source.SortOrder,
        IsActive = source.IsActive,
        OpenInNewTab = source.OpenInNewTab
    };
}
