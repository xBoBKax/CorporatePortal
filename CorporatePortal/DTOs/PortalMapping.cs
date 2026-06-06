using CorporatePortal.Api.Models;

namespace CorporatePortal.Api.DTOs
{
    public static class PortalMapping
    {
        public static PortalPageDto ToDto(this PortalPage page)
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
                    .OrderBy(group => group.SortOrder)
                    .ThenBy(group => group.Title)
                    .Select(group => group.ToDto())
                    .ToArray(),
                page.Modules
                    .OrderBy(module => module.SortOrder)
                    .ThenBy(module => module.Title)
                    .Select(module => module.ToDto())
                    .ToArray());
        }

        public static LinkGroupDto ToDto(this LinkGroup group)
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
                    .OrderBy(link => link.SortOrder)
                    .ThenBy(link => link.Title)
                    .Select(link => link.ToDto())
                    .ToArray());
        }

        public static PortalLinkDto ToDto(this PortalLink link)
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

        public static PortalModuleDto ToDto(this PortalModule module)
        {
            return new PortalModuleDto(
                module.Id,
                module.PortalPageId,
                module.Type,
                module.Title,
                module.Content,
                module.SettingsJson,
                module.SortOrder,
                module.IsVisible);
        }

        public static EmployeeBirthdayDto ToDto(this EmployeeBirthday employee)
        {
            return new EmployeeBirthdayDto(
                employee.Id,
                employee.FullName,
                employee.BirthDate,
                employee.Department,
                employee.IsActive);
        }
    }
}
