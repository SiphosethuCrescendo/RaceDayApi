using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public CategoriesController(RaceDayDbContext context) { _context = context; }

        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            var list = await _context.Categories.Where(c => c.EventID == eventId).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            category.CategoryID = 0;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return NotFound(new { message = "Category not found" });
            _context.Categories.Remove(c);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted" });
        }
    }
}