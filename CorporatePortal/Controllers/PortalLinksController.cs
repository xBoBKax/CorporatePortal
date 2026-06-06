using CorporatePortal.Api.Data;
using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Controllers
{
    [ApiController]
    [Route("api/portal-links")]
    public class PortalLinksController : ControllerBase
    {
        private readonly PortalDbContext db;

        public PortalLinksController(PortalDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<PortalLinkDto>> GetLinks([FromQuery] int? groupId)
        {
            var query = db.PortalLinks.AsQueryable();
            if (groupId.HasValue)
            {
                query = query.Where(link => link.LinkGroupId == groupId.Value);
            }

            var links = await query.OrderBy(link => link.SortOrder)
                .ThenBy(link => link.Title)
                .ToArrayAsync();

            return links.Select(link => link.ToDto()).ToArray();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortalLinkDto>> GetLink(int id)
        {
            var link = await db.PortalLinks.FindAsync(id);
            return link is null ? NotFound() : link.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<PortalLinkDto>> CreateLink(UpsertPortalLinkRequest request)
        {
            var validation = await ValidateLinkRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var link = new PortalLink();
            Apply(request, link);
            db.PortalLinks.Add(link);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLink), new { id = link.Id }, link.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PortalLinkDto>> UpdateLink(int id, UpsertPortalLinkRequest request)
        {
            var link = await db.PortalLinks.FindAsync(id);
            if (link is null)
            {
                return NotFound();
            }

            var validation = await ValidateLinkRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            Apply(request, link);
            await db.SaveChangesAsync();
            return link.ToDto();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLink(int id)
        {
            var link = await db.PortalLinks.FindAsync(id);
            if (link is null)
            {
                return NotFound();
            }

            db.PortalLinks.Remove(link);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<ActionResult<PortalLinkDto>?> ValidateLinkRequest(UpsertPortalLinkRequest request)
        {
            if (!await db.LinkGroups.AnyAsync(group => group.Id == request.LinkGroupId))
            {
                return BadRequest("Группа ссылок не найдена.");
            }

            if (!IsValidUrl(request.Url))
            {
                return BadRequest("URL должен быть абсолютной ссылкой, относительным путем или якорем.");
            }

            return null;
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            var trimmed = url.Trim();
            return Uri.TryCreate(trimmed, UriKind.Absolute, out _)
                || trimmed.StartsWith('/', StringComparison.Ordinal)
                || trimmed.StartsWith('#', StringComparison.Ordinal);
        }

        private static void Apply(UpsertPortalLinkRequest request, PortalLink link)
        {
            link.LinkGroupId = request.LinkGroupId;
            link.Title = request.Title.Trim();
            link.Url = request.Url.Trim();
            link.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            link.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
            link.SortOrder = request.SortOrder;
            link.IsActive = request.IsActive;
            link.OpenInNewTab = request.OpenInNewTab;
        }
    }
}
