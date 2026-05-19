using CorporatePortal.Api.Models;
using CorporatePortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CorporatePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortalController(PortalRepository repository) : ControllerBase
{
    [HttpGet("pages")]
    public ActionResult<IEnumerable<PortalPage>> GetPages() => Ok(repository.GetPages());

    [HttpGet("pages/{id:int}")]
    public ActionResult<PortalPage> GetPage(int id)
    {
        var page = repository.GetPage(id);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPost("pages")]
    public ActionResult<PortalPage> CreatePage([FromBody] PortalPage page)
    {
        var created = repository.CreatePage(page);
        return CreatedAtAction(nameof(GetPage), new { id = created.Id }, created);
    }

    [HttpPut("pages/{id:int}")]
    public ActionResult<PortalPage> UpdatePage(int id, [FromBody] PortalPage page)
    {
        var updated = repository.UpdatePage(id, page);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("pages/{id:int}")]
    public IActionResult DeletePage(int id) => repository.DeletePage(id) ? NoContent() : NotFound();

    [HttpPost("pages/{pageId:int}/groups")]
    public ActionResult<LinkGroup> AddGroup(int pageId, [FromBody] LinkGroup group)
    {
        var added = repository.AddGroup(pageId, group);
        return added is null ? NotFound() : Ok(added);
    }

    [HttpPost("pages/{pageId:int}/groups/{groupId:int}/links")]
    public ActionResult<PortalLink> AddLink(int pageId, int groupId, [FromBody] PortalLink link)
    {
        var added = repository.AddLink(pageId, groupId, link);
        return added is null ? NotFound() : Ok(added);
    }
}
