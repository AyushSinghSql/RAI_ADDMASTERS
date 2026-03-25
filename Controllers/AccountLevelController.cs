using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountLevelController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AccountLevelController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET: api/AcctLevels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcctLevel>>> GetAll()
        {
            return await _context.AcctLevels.ToListAsync();
        }

        // GET: api/AcctLevels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AcctLevel>> Get(int id)
        {
            var item = await _context.AcctLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/AcctLevels
        [HttpPost]
        public async Task<ActionResult<AcctLevel>> Create(AcctLevel model)
        {
            _context.AcctLevels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Level }, model);
        }

        // PUT: api/AcctLevels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AcctLevel model)
        {
            if (id != model.Level)
                return BadRequest();

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AcctLevels.Any(e => e.Level == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/AcctLevels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.AcctLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.AcctLevels.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
