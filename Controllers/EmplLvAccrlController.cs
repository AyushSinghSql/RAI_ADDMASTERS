using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/empl-lv-accrl")]
    [ApiController]
    public class EmplLvAccrlController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmplLvAccrlController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmplLvAccrl model)
        {
            var exists = await _context.EmplLvAccrls
                .AnyAsync(x => x.EmplId == model.EmplId &&
                               x.LvTypeCd == model.LvTypeCd);

            if (exists)
                return Conflict("Already exists");

            _context.EmplLvAccrls.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }


        [HttpDelete("{emplId}/{lvTypeCd}")]
        public async Task<IActionResult> Delete(string emplId, string lvTypeCd)
        {
            var entity = await _context.EmplLvAccrls
                .FindAsync(emplId, lvTypeCd);

            if (entity == null) return NotFound();

            _context.EmplLvAccrls.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
