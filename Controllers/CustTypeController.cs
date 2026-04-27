using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.CustTypes.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var entity = await _context.CustTypes.FindAsync(id);
            return entity == null ? NotFound() : Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustType model)
        {
            if (await _context.CustTypes.AnyAsync(x => x.CustTypeDc == model.CustTypeDc))
                return BadRequest("Duplicate CustType");

            _context.CustTypes.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, CustType model)
        {
            var entity = await _context.CustTypes.FindAsync(id);
            if (entity == null) return NotFound();

            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.CustTypes.FindAsync(id);
            if (entity == null) return NotFound();

            // FK Validation
            if (await _context.Custs.AnyAsync(x => x.CustTypeDc == id))
                return BadRequest("Cannot delete. Used in CUST.");

            _context.CustTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
