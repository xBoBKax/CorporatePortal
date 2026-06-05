using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CorporatePortal.Api.Controllers;

[ApiController]
[Route("api/portal-pages")]
public sealed class PortalPagesController : ControllerBase
{
    private readonly IPortalRepository portalRepository;

    public PortalPagesController(IPortalRepository portalRepository)
    {
        this.portalRepository = portalRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PortalPageDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<PortalPageDto>> GetPages([FromQuery] bool includeInactive = false)
    {
        return Ok(portalRepository.GetPages(includeInactive));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PortalPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PortalPageDto> GetPage(int id, [FromQuery] bool includeInactive = false)
    {
        var page = portalRepository.GetPage(id, includeInactive);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PortalPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PortalPageDto> CreatePage(UpsertPortalPageRequest request)
    {
        if (!IsValidRequiredText(request.Title) || !IsValidRequiredText(request.WelcomeText))
        {
            return BadRequest("Title and WelcomeText are required.");
        }

        var page = portalRepository.CreatePage(request);
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PortalPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PortalPageDto> UpdatePage(int id, UpsertPortalPageRequest request)
    {
        if (!IsValidRequiredText(request.Title) || !IsValidRequiredText(request.WelcomeText))
        {
            return BadRequest("Title and WelcomeText are required.");
        }

        var page = portalRepository.UpdatePage(id, request);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeletePage(int id)
    {
        return portalRepository.DeletePage(id) ? NoContent() : NotFound();
    }

    [HttpPost("{pageId:int}/groups")]
    [ProducesResponseType(typeof(LinkGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<LinkGroupDto> CreateGroup(int pageId, UpsertLinkGroupRequest request)
    {
        if (!IsValidRequiredText(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var group = portalRepository.CreateGroup(pageId, request);
        return group is null
            ? NotFound()
            : CreatedAtAction(nameof(GetPage), new { id = pageId }, group);
    }

    [HttpPut("{pageId:int}/groups/{groupId:int}")]
    [ProducesResponseType(typeof(LinkGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<LinkGroupDto> UpdateGroup(int pageId, int groupId, UpsertLinkGroupRequest request)
    {
        if (!IsValidRequiredText(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var group = portalRepository.UpdateGroup(pageId, groupId, request);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpDelete("{pageId:int}/groups/{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteGroup(int pageId, int groupId)
    {
        return portalRepository.DeleteGroup(pageId, groupId) ? NoContent() : NotFound();
    }

    [HttpPost("{pageId:int}/groups/{groupId:int}/links")]
    [ProducesResponseType(typeof(PortalLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PortalLinkDto> CreateLink(int pageId, int groupId, UpsertPortalLinkRequest request)
    {
        if (!IsValidRequiredText(request.Title) || !IsValidRequiredText(request.Url))
        {
            return BadRequest("Title and Url are required.");
        }

        var link = portalRepository.CreateLink(pageId, groupId, request);
        return link is null
            ? NotFound()
            : CreatedAtAction(nameof(GetPage), new { id = pageId }, link);
    }

    [HttpPut("{pageId:int}/groups/{groupId:int}/links/{linkId:int}")]
    [ProducesResponseType(typeof(PortalLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PortalLinkDto> UpdateLink(int pageId, int groupId, int linkId, UpsertPortalLinkRequest request)
    {
        if (!IsValidRequiredText(request.Title) || !IsValidRequiredText(request.Url))
        {
            return BadRequest("Title and Url are required.");
        }

        var link = portalRepository.UpdateLink(pageId, groupId, linkId, request);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpDelete("{pageId:int}/groups/{groupId:int}/links/{linkId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteLink(int pageId, int groupId, int linkId)
    {
        return portalRepository.DeleteLink(pageId, groupId, linkId) ? NoContent() : NotFound();
    }

    private static bool IsValidRequiredText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
