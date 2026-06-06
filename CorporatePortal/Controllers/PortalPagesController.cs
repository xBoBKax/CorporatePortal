using CorporatePortal.Api.Data;
using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Controllers
{
    [ApiController]
    [Route("api/portal-pages")]
    public class PortalPagesController : ControllerBase
    {
        private readonly PortalDbContext db;

        public PortalPagesController(PortalDbContext db)
        {
            this.db = db;
        }

        [HttpGet("active")]
        public async Task<ActionResult<PortalPageDto>> GetActivePage()
        {
            var page = await QueryPages()
                .Where(page => page.IsActive)
                .OrderBy(page => page.Id)
                .FirstOrDefaultAsync();

            return page is null ? NotFound() : page.ToDto();
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<PortalPageDto>> GetPages()
        {
            var pages = await QueryPages()
                .OrderBy(page => page.Id)
                .ToArrayAsync();

            return pages.Select(page => page.ToDto()).ToArray();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortalPageDto>> GetPage(int id)
        {
            var page = await QueryPages().FirstOrDefaultAsync(page => page.Id == id);
            return page is null ? NotFound() : page.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<PortalPageDto>> CreatePage(UpsertPortalPageRequest request)
        {
            var page = new PortalPage();
            Apply(request, page);
            db.PortalPages.Add(page);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PortalPageDto>> UpdatePage(int id, UpsertPortalPageRequest request)
        {
            var page = await db.PortalPages.FindAsync(id);
            if (page is null)
            {
                return NotFound();
            }

            Apply(request, page);
            await db.SaveChangesAsync();
            return page.ToDto();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePage(int id)
        {
            var page = await db.PortalPages.FindAsync(id);
            if (page is null)
            {
                return NotFound();
            }

            db.PortalPages.Remove(page);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private IQueryable<PortalPage> QueryPages()
        {
            return db.PortalPages
                .Include(page => page.LinkGroups)
                    .ThenInclude(group => group.Links)
                .Include(page => page.Modules)
                .AsSplitQuery();
        }

        private static void Apply(UpsertPortalPageRequest request, PortalPage page)
        {
            page.Title = request.Title.Trim();
            page.WelcomeText = request.WelcomeText.Trim();
            page.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
            page.BackgroundType = request.BackgroundType.Trim();
            page.BackgroundValue = string.IsNullOrWhiteSpace(request.BackgroundValue) ? null : request.BackgroundValue.Trim();
            page.IsActive = request.IsActive;
        }
    }
}
