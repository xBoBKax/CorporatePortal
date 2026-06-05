using CorporatePortal.Api.DTOs;

namespace CorporatePortal.Api.Services;

public interface IPortalRepository
{
    IReadOnlyList<PortalPageDto> GetPages(bool includeInactive = false);

    PortalPageDto? GetPage(int id, bool includeInactive = false);

    PortalPageDto CreatePage(UpsertPortalPageRequest request);

    PortalPageDto? UpdatePage(int id, UpsertPortalPageRequest request);

    bool DeletePage(int id);

    LinkGroupDto? CreateGroup(int pageId, UpsertLinkGroupRequest request);

    LinkGroupDto? UpdateGroup(int pageId, int groupId, UpsertLinkGroupRequest request);

    bool DeleteGroup(int pageId, int groupId);

    PortalLinkDto? CreateLink(int pageId, int groupId, UpsertPortalLinkRequest request);

    PortalLinkDto? UpdateLink(int pageId, int groupId, int linkId, UpsertPortalLinkRequest request);

    bool DeleteLink(int pageId, int groupId, int linkId);
}
