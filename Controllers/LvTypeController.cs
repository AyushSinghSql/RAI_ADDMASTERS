using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Helpers;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/lv-type")]
    [ApiController]
    public class LvTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public LvTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.LvTypes.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(LvType model)
        {
            if (!await _context.SLvCeilMthds
                .AnyAsync(x => x.Code == model.CeilMethodCd))
                return BadRequest("Invalid ceiling method");

            AuditHelper.ApplyCreate(model);

            _context.LvTypes.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, LvType model)
        {
            var entity = await _context.LvTypes.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Description = model.Description;
            entity.CeilMethodCd = model.CeilMethodCd;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.LvTypes.FindAsync(id);
            if (entity == null) return NotFound();

            _context.LvTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
