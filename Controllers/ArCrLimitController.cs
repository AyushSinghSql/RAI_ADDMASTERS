using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArCrLimitController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ArCrLimitController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.ArCrLimits.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(ArCrLimit model)
        {
            if (model.LimitAmt <= 0)
                return BadRequest("Limit must be > 0");

            _context.ArCrLimits.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ArCrLimit model)
        {
            var entity = await _context.ArCrLimits.FindAsync(id);
            if (entity == null) return NotFound();

            entity.LimitAmt = model.LimitAmt;
            entity.ModifiedBy = model.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.Custs.AnyAsync(x => x.ArCrLimitKey == id))
                return BadRequest("In use by Customer");

            var entity = await _context.ArCrLimits.FindAsync(id);
            if (entity == null) return NotFound();

            _context.ArCrLimits.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
