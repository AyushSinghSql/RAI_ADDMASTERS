using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/lv-table")]
    [ApiController]
    public class LvTableController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public LvTableController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.LvTables.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(LvTable model)
        {
            if (!await _context.LvTypes
                .AnyAsync(x => x.LvTypeCd == model.LvTypeCd))
                return BadRequest("Invalid LvType");

            _context.LvTables.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, LvTable model)
        {
            var entity = await _context.LvTables.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Description = model.Description;
            entity.TimeStamp = DateTime.UtcNow;
            entity.ModifiedBy = model.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.LvTables.FindAsync(id);
            if (entity == null) return NotFound();

            _context.LvTables.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
