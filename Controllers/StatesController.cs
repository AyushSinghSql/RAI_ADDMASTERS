using System;
using System.Linq;
using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/states")]
    public sealed class StatesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public StatesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? countryCode)
        {
            IQueryable<State> query = _context.States.AsNoTracking();
            
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                query = query.Where(s => s.CountryCode == countryCode);
            }
            
            var states = await query.OrderBy(s => s.StateName).ToListAsync();
            return Ok(states);
        }

        [HttpGet("{countryCode}/{stateCode}")]
        public async Task<IActionResult> GetByKey(string countryCode, string stateCode)
        {
            var state = await _context.States
                .FirstOrDefaultAsync(s => s.CountryCode == countryCode && s.StateCode == stateCode);
            if (state == null)
            {
                return NotFound();
            }
            return Ok(state);
        }

        [HttpPost]
        public async Task<IActionResult> Create(State state)
        {
            if (string.IsNullOrWhiteSpace(state.CountryCode) || string.IsNullOrWhiteSpace(state.StateCode) || string.IsNullOrWhiteSpace(state.StateName))
            {
                return BadRequest("CountryCode, StateCode, and StateName are required.");
            }

            try
            {
                var exists = await _context.States
                    .AnyAsync(s => s.CountryCode == state.CountryCode && s.StateCode == state.StateCode);
                if (exists)
                {
                    return Conflict($"State with CountryCode '{state.CountryCode}' and StateCode '{state.StateCode}' already exists.");
                }

                state.ChangedBy = string.IsNullOrWhiteSpace(state.ChangedBy) ? "SYSTEM" : state.ChangedBy;
                state.ChangedDate = DateTime.UtcNow;

                await _context.States.AddAsync(state);
                await _context.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetByKey), new { countryCode = state.CountryCode, stateCode = state.StateCode }, state);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return BadRequest($"Parent Country with code '{state.CountryCode}' does not exist in country table.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{countryCode}/{stateCode}")]
        public async Task<IActionResult> Update(string countryCode, string stateCode, State state)
        {
            if (string.IsNullOrWhiteSpace(state.StateName))
            {
                return BadRequest("StateName is required.");
            }

            try
            {
                var dbState = await _context.States
                    .FirstOrDefaultAsync(s => s.CountryCode == countryCode && s.StateCode == stateCode);
                if (dbState == null)
                {
                    return NotFound($"State with CountryCode '{countryCode}' and StateCode '{stateCode}' not found.");
                }

                dbState.StateName = state.StateName;
                dbState.ChangedBy = string.IsNullOrWhiteSpace(state.ChangedBy) ? "SYSTEM" : state.ChangedBy;
                dbState.ChangedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{countryCode}/{stateCode}")]
        public async Task<IActionResult> Delete(string countryCode, string stateCode)
        {
            try
            {
                var dbState = await _context.States
                    .FirstOrDefaultAsync(s => s.CountryCode == countryCode && s.StateCode == stateCode);
                if (dbState == null)
                {
                    return NotFound($"State with CountryCode '{countryCode}' and StateCode '{stateCode}' not found.");
                }

                _context.States.Remove(dbState);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return Conflict("Cannot delete this state because it is referenced by one or more postal codes.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
