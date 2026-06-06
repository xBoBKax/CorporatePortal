using System.ComponentModel.DataAnnotations;

namespace CorporatePortal.Api.DTOs
{
    public record PortalPageDto(
        int Id,
        string Title,
        string WelcomeText,
        string? LogoUrl,
        string BackgroundType,
        string? BackgroundValue,
        bool IsActive,
        IReadOnlyCollection<LinkGroupDto> LinkGroups,
        IReadOnlyCollection<PortalModuleDto> Modules);

    public class UpsertPortalPageRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string WelcomeText { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? LogoUrl { get; set; }

        [Required, MaxLength(50)]
        public string BackgroundType { get; set; } = "decorative";

        [MaxLength(1000)]
        public string? BackgroundValue { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public record LinkGroupDto(
        int Id,
        int PortalPageId,
        string Title,
        string? Description,
        string? Icon,
        int SortOrder,
        bool IsVisible,
        IReadOnlyCollection<PortalLinkDto> Links);

    public class UpsertLinkGroupRequest
    {
        [Required]
        public int PortalPageId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;
    }

    public record PortalLinkDto(
        int Id,
        int LinkGroupId,
        string Title,
        string Url,
        string? Description,
        string? Icon,
        int SortOrder,
        bool IsActive,
        bool OpenInNewTab);

    public class UpsertPortalLinkRequest
    {
        [Required]
        public int LinkGroupId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool OpenInNewTab { get; set; } = true;
    }

    public record PortalModuleDto(
        int Id,
        int PortalPageId,
        string Type,
        string Title,
        string? Content,
        string? SettingsJson,
        int SortOrder,
        bool IsVisible);

    public class UpsertPortalModuleRequest
    {
        [Required]
        public int PortalPageId { get; set; }

        [Required, MaxLength(80)]
        public string Type { get; set; } = "Text";

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? SettingsJson { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;
    }

    public record EmployeeBirthdayDto(
        int Id,
        string FullName,
        DateOnly BirthDate,
        string? Department,
        bool IsActive);

    public class UpsertEmployeeBirthdayRequest
    {
        [Required, MaxLength(250)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateOnly BirthDate { get; set; }

        [MaxLength(250)]
        public string? Department { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
