namespace CorporatePortal.Api.DTOs;

public sealed record PortalPageDto(
    int Id,
    string Title,
    string WelcomeText,
    string? LogoUrl,
    string BackgroundType,
    string? BackgroundValue,
    bool IsActive,
    IReadOnlyList<LinkGroupDto> LinkGroups);

public sealed record LinkGroupDto(
    int Id,
    int PortalPageId,
    string Title,
    string? Description,
    string? Icon,
    int SortOrder,
    bool IsVisible,
    IReadOnlyList<PortalLinkDto> Links);

public sealed record PortalLinkDto(
    int Id,
    int LinkGroupId,
    string Title,
    string Url,
    string? Description,
    string? Icon,
    int SortOrder,
    bool IsActive,
    bool OpenInNewTab);

public sealed record UpsertPortalPageRequest(
    string Title,
    string WelcomeText,
    string? LogoUrl,
    string BackgroundType,
    string? BackgroundValue,
    bool IsActive);

public sealed record UpsertLinkGroupRequest(
    string Title,
    string? Description,
    string? Icon,
    int SortOrder,
    bool IsVisible);

public sealed record UpsertPortalLinkRequest(
    string Title,
    string Url,
    string? Description,
    string? Icon,
    int SortOrder,
    bool IsActive,
    bool OpenInNewTab);
