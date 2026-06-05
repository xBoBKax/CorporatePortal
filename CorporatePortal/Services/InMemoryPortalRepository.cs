using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;

namespace CorporatePortal.Api.Services;

public sealed class InMemoryPortalRepository : IPortalRepository
{
    private readonly object syncRoot = new();
    private readonly List<PortalPage> pages = new();
    private int nextPageId = 1;
    private int nextGroupId = 1;
    private int nextLinkId = 1;

    public InMemoryPortalRepository()
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

        group.Links.AddRange(
        [
            new PortalLink
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
            },
            new PortalLink
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
            }
        ]);

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
}
