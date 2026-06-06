using CorporatePortal.Api.Data;
using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Controllers
{
    [ApiController]
    [Route("api/link-groups")]
    public class LinkGroupsController : ControllerBase
    {
        private readonly PortalDbContext db;

        public LinkGroupsController(PortalDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<LinkGroupDto>> GetGroups([FromQuery] int? pageId)
        {
            var query = db.LinkGroups.Include(group => group.Links).AsQueryable();
            if (pageId.HasValue)
            {
                query = query.Where(group => group.PortalPageId == pageId.Value);
            }

            var groups = await query.OrderBy(group => group.SortOrder)
                .ThenBy(group => group.Title)
                .ToArrayAsync();

            return groups.Select(group => group.ToDto()).ToArray();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LinkGroupDto>> GetGroup(int id)
        {
            var group = await db.LinkGroups.Include(group => group.Links).FirstOrDefaultAsync(group => group.Id == id);
            return group is null ? NotFound() : group.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<LinkGroupDto>> CreateGroup(UpsertLinkGroupRequest request)
        {
            if (!await db.PortalPages.AnyAsync(page => page.Id == request.PortalPageId))
            {
                return BadRequest("Страница портала не найдена.");
            }

            var group = new LinkGroup();
            Apply(request, group);
            db.LinkGroups.Add(group);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<LinkGroupDto>> UpdateGroup(int id, UpsertLinkGroupRequest request)
        {
            var group = await db.LinkGroups.Include(group => group.Links).FirstOrDefaultAsync(group => group.Id == id);
            if (group is null)
            {
                return NotFound();
            }

            if (!await db.PortalPages.AnyAsync(page => page.Id == request.PortalPageId))
            {
                return BadRequest("Страница портала не найдена.");
            }

            Apply(request, group);
            await db.SaveChangesAsync();
            return group.ToDto();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await db.LinkGroups.FindAsync(id);
            if (group is null)
            {
                return NotFound();
            }

            db.LinkGroups.Remove(group);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private static void Apply(UpsertLinkGroupRequest request, LinkGroup group)
        {
            group.PortalPageId = request.PortalPageId;
            group.Title = request.Title.Trim();
            group.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            group.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
            group.SortOrder = request.SortOrder;
            group.IsVisible = request.IsVisible;
        }
    }
}
