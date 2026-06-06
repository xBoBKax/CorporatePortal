using CorporatePortal.Api.Data;
using CorporatePortal.Api.DTOs;
using CorporatePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Controllers
{
    [ApiController]
    [Route("api/portal-modules")]
    public class PortalModulesController : ControllerBase
    {
        private readonly PortalDbContext db;

        public PortalModulesController(PortalDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<PortalModuleDto>> GetModules([FromQuery] int? pageId)
        {
            var query = db.PortalModules.AsQueryable();
            if (pageId.HasValue)
            {
                query = query.Where(module => module.PortalPageId == pageId.Value);
            }

            var modules = await query.OrderBy(module => module.SortOrder)
                .ThenBy(module => module.Title)
                .ToArrayAsync();

            return modules.Select(module => module.ToDto()).ToArray();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortalModuleDto>> GetModule(int id)
        {
            var module = await db.PortalModules.FindAsync(id);
            return module is null ? NotFound() : module.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<PortalModuleDto>> CreateModule(UpsertPortalModuleRequest request)
        {
            if (!await db.PortalPages.AnyAsync(page => page.Id == request.PortalPageId))
            {
                return BadRequest("Страница портала не найдена.");
            }

            var module = new PortalModule();
            Apply(request, module);
            db.PortalModules.Add(module);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetModule), new { id = module.Id }, module.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PortalModuleDto>> UpdateModule(int id, UpsertPortalModuleRequest request)
        {
            var module = await db.PortalModules.FindAsync(id);
            if (module is null)
            {
                return NotFound();
            }

            if (!await db.PortalPages.AnyAsync(page => page.Id == request.PortalPageId))
            {
                return BadRequest("Страница портала не найдена.");
            }

            Apply(request, module);
            await db.SaveChangesAsync();
            return module.ToDto();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            var module = await db.PortalModules.FindAsync(id);
            if (module is null)
            {
                return NotFound();
            }

            db.PortalModules.Remove(module);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private static void Apply(UpsertPortalModuleRequest request, PortalModule module)
        {
            module.PortalPageId = request.PortalPageId;
            module.Type = request.Type.Trim();
            module.Title = request.Title.Trim();
            module.Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();
            module.SettingsJson = string.IsNullOrWhiteSpace(request.SettingsJson) ? null : request.SettingsJson.Trim();
            module.SortOrder = request.SortOrder;
            module.IsVisible = request.IsVisible;
        }
    }
}
