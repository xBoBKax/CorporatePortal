using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;

namespace CorporatePortal.Api.Services;

public sealed class PortalRepository : IPortalRepository
{
    private readonly object syncRoot = new();
    private readonly List<PortalPage> pages = new();
    private int nextPageId = 1;
    private int nextGroupId = 1;
    private int nextLinkId = 1;


    public PortalRepository()
    {
        Seed();
    }

    public IReadOnlyList<PortalPageDto> GetPages(bool includeInactive = false)
    {
        lock (syncRoot)
        {
            return pages
                .Where(page => includeInactive || page.IsActive)
                .OrderBy(page => page.Id)
                .Select(ToDto)
                .ToList();
        }
    }

    public PortalPageDto? GetPage(int id, bool includeInactive = false)
    {
        lock (syncRoot)
        {
            var page = pages.FirstOrDefault(page => page.Id == id && (includeInactive || page.IsActive));
            return page is null ? null : ToDto(page);
        }
    }

    public PortalPageDto CreatePage(UpsertPortalPageRequest request)
    {
        lock (syncRoot)
        {
            var page = new PortalPage
            {
                Id = nextPageId++,
                Title = request.Title.Trim(),
                WelcomeText = request.WelcomeText.Trim(),
                LogoUrl = NormalizeOptional(request.LogoUrl),
                BackgroundType = NormalizeBackgroundType(request.BackgroundType),
                BackgroundValue = NormalizeOptional(request.BackgroundValue),
                IsActive = request.IsActive
            };

            pages.Add(page);

            return ToDto(page);
        }
    }

    public PortalPageDto? UpdatePage(int id, UpsertPortalPageRequest request)
    {
        lock (syncRoot)
        {
            var page = pages.FirstOrDefault(page => page.Id == id);
            if (page is null)
            {
                return null;
            }

            page.Title = request.Title.Trim();
            page.WelcomeText = request.WelcomeText.Trim();
            page.LogoUrl = NormalizeOptional(request.LogoUrl);
            page.BackgroundType = NormalizeBackgroundType(request.BackgroundType);
            page.BackgroundValue = NormalizeOptional(request.BackgroundValue);
            page.IsActive = request.IsActive;

            return ToDto(page);
        }
    }

    public bool DeletePage(int id)
    {
        lock (syncRoot)
        {
            var page = pages.FirstOrDefault(page => page.Id == id);
            return page is not null && pages.Remove(page);
        }
    }

    public LinkGroupDto? CreateGroup(int pageId, UpsertLinkGroupRequest request)
    {
        lock (syncRoot)
        {
            var page = pages.FirstOrDefault(page => page.Id == pageId);
            if (page is null)
            {
                return null;
            }

            var group = new LinkGroup
            {
                Id = nextGroupId++,
                PortalPageId = pageId,
                Title = request.Title.Trim(),
                Description = NormalizeOptional(request.Description),
                Icon = NormalizeOptional(request.Icon),
                SortOrder = request.SortOrder,
                IsVisible = request.IsVisible,
                PortalPage = page
            };

            page.LinkGroups.Add(group);

            return ToDto(group);
        }
    }

    public LinkGroupDto? UpdateGroup(int pageId, int groupId, UpsertLinkGroupRequest request)
    {
        lock (syncRoot)
        {
            var group = FindGroup(pageId, groupId);
            if (group is null)
            {
                return null;
            }

            group.Title = request.Title.Trim();
            group.Description = NormalizeOptional(request.Description);
            group.Icon = NormalizeOptional(request.Icon);
            group.SortOrder = request.SortOrder;
            group.IsVisible = request.IsVisible;

            return ToDto(group);
        }
    }

    public bool DeleteGroup(int pageId, int groupId)
    {
        lock (syncRoot)
        {
            var page = pages.FirstOrDefault(page => page.Id == pageId);
            var group = page?.LinkGroups.FirstOrDefault(group => group.Id == groupId);
            return page is not null && group is not null && page.LinkGroups.Remove(group);
        }
    }

    public PortalLinkDto? CreateLink(int pageId, int groupId, UpsertPortalLinkRequest request)
    {
        lock (syncRoot)
        {
            var group = FindGroup(pageId, groupId);
            if (group is null)
            {
                return null;
            }

            var link = new PortalLink
            {
                Id = nextLinkId++,
                LinkGroupId = groupId,
                Title = request.Title.Trim(),
                Url = request.Url.Trim(),
                Description = NormalizeOptional(request.Description),
                Icon = NormalizeOptional(request.Icon),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                OpenInNewTab = request.OpenInNewTab,
                LinkGroup = group
            };

            group.Links.Add(link);

            return ToDto(link);
        }
    }

    public PortalLinkDto? UpdateLink(int pageId, int groupId, int linkId, UpsertPortalLinkRequest request)
    {
        lock (syncRoot)
        {
            var link = FindLink(pageId, groupId, linkId);
            if (link is null)
            {
                return null;
            }

            link.Title = request.Title.Trim();
            link.Url = request.Url.Trim();
            link.Description = NormalizeOptional(request.Description);
            link.Icon = NormalizeOptional(request.Icon);
            link.SortOrder = request.SortOrder;
            link.IsActive = request.IsActive;
            link.OpenInNewTab = request.OpenInNewTab;

            return ToDto(link);
        }
    }

    public bool DeleteLink(int pageId, int groupId, int linkId)
    {
        lock (syncRoot)
        {
            var group = FindGroup(pageId, groupId);
            var link = group?.Links.FirstOrDefault(link => link.Id == linkId);
            return group is not null && link is not null && group.Links.Remove(link);
        }
    }

    private static PortalPageDto ToDto(PortalPage page)
    {
        return new PortalPageDto(
            page.Id,
            page.Title,
            page.WelcomeText,
            page.LogoUrl,
            page.BackgroundType,
            page.BackgroundValue,
            page.IsActive,
            page.LinkGroups
                .Where(group => group.IsVisible)
                .OrderBy(group => group.SortOrder)
                .ThenBy(group => group.Title)
                .Select(ToDto)
                .ToList());
    }

    private static LinkGroupDto ToDto(LinkGroup group)
    {
        return new LinkGroupDto(
            group.Id,
            group.PortalPageId,
            group.Title,
            group.Description,
            group.Icon,
            group.SortOrder,
            group.IsVisible,
            group.Links
                .Where(link => link.IsActive)
                .OrderBy(link => link.SortOrder)
                .ThenBy(link => link.Title)
                .Select(ToDto)
                .ToList());
    }

    private static PortalLinkDto ToDto(PortalLink link)
    {
        return new PortalLinkDto(
            link.Id,
            link.LinkGroupId,
            link.Title,
            link.Url,
            link.Description,
            link.Icon,
            link.SortOrder,
            link.IsActive,
            link.OpenInNewTab);
    }

    private LinkGroup? FindGroup(int pageId, int groupId)
    {
        return pages
            .FirstOrDefault(page => page.Id == pageId)?
            .LinkGroups
            .FirstOrDefault(group => group.Id == groupId);
    }

    private PortalLink? FindLink(int pageId, int groupId, int linkId)
    {
        return FindGroup(pageId, groupId)?
            .Links
            .FirstOrDefault(link => link.Id == linkId);
    }

    private void Seed()
    {
        var page = new PortalPage
        {
            Id = nextPageId++,
            Title = "Corporate Portal",
            WelcomeText = "Добро пожаловать в корпоративный портал",
            BackgroundType = "color",
            BackgroundValue = "#f6f8fb",
            IsActive = true
        };

        var group = new LinkGroup
        {
            Id = nextGroupId++,
            PortalPageId = page.Id,
            Title = "Основные сервисы",
            Description = "Быстрый доступ к часто используемым системам",
            Icon = "apps",
            SortOrder = 10,
            IsVisible = true,
            PortalPage = page
        };

        group.Links.Add(new PortalLink
        {
            Id = nextLinkId++,
            LinkGroupId = group.Id,
            Title = "Документация",
            Url = "https://intranet.example.com/docs",
            Description = "Регламенты, инструкции и база знаний",
            Icon = "book",
            SortOrder = 10,
            IsActive = true,
            OpenInNewTab = true,
            LinkGroup = group
        });

        group.Links.Add(new PortalLink
        {
            Id = nextLinkId++,
            LinkGroupId = group.Id,
            Title = "Service Desk",
            Url = "https://intranet.example.com/helpdesk",
            Description = "Заявки в ИТ и административные службы",
            Icon = "support",
            SortOrder = 20,
            IsActive = true,
            OpenInNewTab = true,
            LinkGroup = group
        });

        page.LinkGroups.Add(group);
        pages.Add(page);
    }

    private static string NormalizeBackgroundType(string? backgroundType)
    {
        return string.IsNullOrWhiteSpace(backgroundType) ? "color" : backgroundType.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    public IReadOnlyList<PortalPage> GetPages(){lock(_sync){return _pages.Select(ClonePage).ToList();}}
    public PortalPage? GetPage(int id){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);return p is null?null:ClonePage(p);}}
    public PortalPage CreatePage(PortalPage request){lock(_sync){var p=new PortalPage{Id=++_pageId,Title=request.Title,WelcomeText=request.WelcomeText,LogoUrl=request.LogoUrl,BackgroundType=request.BackgroundType,BackgroundValue=request.BackgroundValue,IsActive=request.IsActive,ThemeCss=request.ThemeCss};_pages.Add(p);return ClonePage(p);}}
    public PortalPage? UpdatePage(int id, PortalPage request){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);if(p is null)return null;p.Title=request.Title;p.WelcomeText=request.WelcomeText;p.LogoUrl=request.LogoUrl;p.BackgroundType=request.BackgroundType;p.BackgroundValue=request.BackgroundValue;p.IsActive=request.IsActive;p.ThemeCss=request.ThemeCss;p.Widgets=request.Widgets.Select(CloneWidget).OrderBy(x=>x.SortOrder).ToList();return ClonePage(p);}}
    public bool DeletePage(int id){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==id);return p is not null&&_pages.Remove(p);}}
    public LinkGroup? AddGroup(int pageId, LinkGroup group){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==pageId);if(p is null)return null;var i=new LinkGroup{Id=++_groupId,PortalPageId=pageId,Title=group.Title,Description=group.Description,Icon=group.Icon,SortOrder=group.SortOrder,IsVisible=group.IsVisible};p.LinkGroups.Add(i);return CloneGroup(i);}}
    public PortalLink? AddLink(int pageId, int groupId, PortalLink link){lock(_sync){var g=_pages.FirstOrDefault(p=>p.Id==pageId)?.LinkGroups.FirstOrDefault(g=>g.Id==groupId);if(g is null)return null;var i=new PortalLink{Id=++_linkId,LinkGroupId=groupId,Title=link.Title,Url=link.Url,Description=link.Description,Icon=link.Icon,SortOrder=link.SortOrder,IsActive=link.IsActive,OpenInNewTab=link.OpenInNewTab};g.Links.Add(i);return CloneLink(i);}}
    public PortalPage? ReorderGroups(int pageId, List<int> groupIds){lock(_sync){var p=_pages.FirstOrDefault(x=>x.Id==pageId);if(p is null)return null;for(int i=0;i<groupIds.Count;i++){var g=p.LinkGroups.FirstOrDefault(x=>x.Id==groupIds[i]);if(g is not null)g.SortOrder=i+1;}p.LinkGroups=p.LinkGroups.OrderBy(x=>x.SortOrder).ToList();return ClonePage(p);}}
    public PortalPage? ReorderLinks(int pageId,int groupId,List<int> linkIds){lock(_sync){var g=_pages.FirstOrDefault(p=>p.Id==pageId)?.LinkGroups.FirstOrDefault(x=>x.Id==groupId);if(g is null)return null;for(int i=0;i<linkIds.Count;i++){var l=g.Links.FirstOrDefault(x=>x.Id==linkIds[i]);if(l is not null)l.SortOrder=i+1;}g.Links=g.Links.OrderBy(x=>x.SortOrder).ToList();return ClonePage(_pages.First(p=>p.Id==pageId));}}
    private static PortalPage ClonePage(PortalPage s)=>new(){Id=s.Id,Title=s.Title,WelcomeText=s.WelcomeText,LogoUrl=s.LogoUrl,BackgroundType=s.BackgroundType,BackgroundValue=s.BackgroundValue,IsActive=s.IsActive,ThemeCss=s.ThemeCss,Widgets=s.Widgets.Select(CloneWidget).ToList(),LinkGroups=s.LinkGroups.Select(CloneGroup).ToList()};
    private static LinkGroup CloneGroup(LinkGroup s)=>new(){Id=s.Id,PortalPageId=s.PortalPageId,Title=s.Title,Description=s.Description,Icon=s.Icon,SortOrder=s.SortOrder,IsVisible=s.IsVisible,Links=s.Links.Select(CloneLink).ToList()};
    private static PortalLink CloneLink(PortalLink s)=>new(){Id=s.Id,LinkGroupId=s.LinkGroupId,Title=s.Title,Url=s.Url,Description=s.Description,Icon=s.Icon,SortOrder=s.SortOrder,IsActive=s.IsActive,OpenInNewTab=s.OpenInNewTab};
    private static WidgetConfig CloneWidget(WidgetConfig s)=>new(){Id=s.Id,Type=s.Type,Title=s.Title,SortOrder=s.SortOrder,IsVisible=s.IsVisible,EmbedHtml=s.EmbedHtml};
}
