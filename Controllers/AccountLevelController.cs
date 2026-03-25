using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
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
        [HttpGet("GetAllAccountLevels")]
        public async Task<ActionResult<IEnumerable<AcctLevel>>> GetAllAccountLevels()
        {
            return await _context.AcctLevels.ToListAsync();
        }

        [HttpGet("GetAllAccountLevelsV1")]
        public async Task<ActionResult<IEnumerable<LevelDto>>> GetAllAccountLevelsV1()
        {
            var result = await (
                from lvl in _context.AcctLevels
                join acc in _context.Accounts
                    on lvl.Level equals acc.LvlNo into accGroup
                select new LevelDto
                {
                    Level = lvl.Level,
                    Lenght = lvl.Lenght,
                    Count = accGroup.Count()
                }
            ).OrderBy(x => x.Level).ToListAsync();

            return result;
        }

        // GET: api/AcctLevels/5
        [HttpGet("GetAccountLevel/{id}")]
        public async Task<ActionResult<AcctLevel>> GetAccountLevel(int id)
        {
            var item = await _context.AcctLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/AcctLevels
        [HttpPost("CreateAccountLevel")]
        public async Task<ActionResult<AcctLevel>> CreateAccountLevel(AcctLevel model)
        {
            _context.AcctLevels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccountLevel), new { id = model.Level }, model);
        }

        // PUT: api/AcctLevels/5
        [HttpPut("UpdateAccountLevel/{id}")]
        public async Task<IActionResult> UpdateAccountLevel(int id, AcctLevel model)
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
        [HttpDelete("DeleteAccountLevel/{id}")]
        public async Task<IActionResult> DeleteAccountLevel(int id)
        {
            var item = await _context.AcctLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.AcctLevels.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/AcctLevels
        [HttpGet("GetAllOrgLevels")]
        public async Task<ActionResult<IEnumerable<OrgLevel>>> GetAllOrgLevels()
        {
            return await _context.OrgLevels.ToListAsync();
        }

        [HttpGet("GetAllOrgLevelsV1")]
        public async Task<ActionResult<IEnumerable<LevelDto>>> GetAllOrgLevelsV1()
        {
            var result = await (
                from lvl in _context.OrgLevels
                join org in _context.Organizations
                    on lvl.Level equals org.LvlNo into OrgGroup
                select new LevelDto
                {
                    Level = lvl.Level,
                    Lenght = lvl.Lenght,
                    Count = OrgGroup.Count()
                }
            ).OrderBy(x => x.Level).ToListAsync();

            return result;
        }

        // GET: api/AcctLevels/5
        [HttpGet("GetOrgLevel/{id}")]
        public async Task<ActionResult<OrgLevel>> GetOrgLevel(int id)
        {
            var item = await _context.OrgLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/AcctLevels
        [HttpPost("CreateOrgLevel")]
        public async Task<ActionResult<AcctLevel>> CreateOrgLevel(OrgLevel model)
        {
            _context.OrgLevels.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrgLevel), new { id = model.Level }, model);
        }

        // PUT: api/AcctLevels/5
        [HttpPut("UpdateOrgLevel/{id}")]
        public async Task<IActionResult> UpdateOrgLevel(int id, OrgLevel model)
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
        [HttpDelete("DeleteOrgLevel/{id}")]
        public async Task<IActionResult> DeleteOrgLevel(int id)
        {
            var item = await _context.OrgLevels.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.OrgLevels.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
