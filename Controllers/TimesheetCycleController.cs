using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimesheetCycleController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public TimesheetCycleController(MydatabaseContext context)
        {
            _context = context;
        }


        // ✅ Create
        [HttpPost]
        public async Task<IActionResult> Create(TimesheetCycleDto dto)
        {

            if (dto == null)
            {
                return BadRequest("Request body cannot be empty.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var exists = await _context.TimesheetCycles.AnyAsync(x => x.TimesheetCycleId == dto.TimesheetCycleId);

                if (exists)
                {
                    return Conflict($"Timesheet Cycle with ID '{dto.TimesheetCycleId}' already exists.");
                }

                string currentUser = "SystemUser";
                var entity = dto.ToEntity(currentUser);

                _context.TimesheetCycles.Add(entity);
                await _context.SaveChangesAsync();

                return Ok("Timesheet Cycle created successfully.");
            }
                 catch (DbUpdateException ex)
                {
                    return StatusCode(500, $"A database error occurred while saving the entity: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.TimesheetCycles.AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.TimesheetCycleId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.ToDto())
                .ToListAsync();

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount,
                data
            });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get(string timesheetCycleId)
        {
            var data = await _context.TimesheetCycles.Where(x => x.TimesheetCycleId == timesheetCycleId).Select(x => x.ToDto()).FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound("Timesheet Cycle not found.");
            }

            return Ok(data);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TimesheetCycleDto dto, [FromQuery] string updatedBy)
        {
            if (id != dto.TimesheetCycleId)
            {
                return BadRequest();
            }
            var existing = await _context.TimesheetCycles.FirstOrDefaultAsync(x => x.TimesheetCycleId == dto.TimesheetCycleId);
            if (existing == null)
            {
                return NotFound("Timesheet Cycle not found.");
            }

            existing.Description = dto.Description;
            existing.Frequency = dto.Frequency;
            existing.IsActive = dto.IsActive;

            existing.UpdatedBy = dto.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
 

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Timesheet Cycle updated successfully." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "A database error occurred during the update.");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var cycleId = await _context.TimesheetCycles.FindAsync(id);

            if (cycleId == null)
            {
                return NotFound();
            }

            _context.TimesheetCycles.Remove(cycleId);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //// ✅ Soft Delete

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    var entity = await _context.TimesheetCycles.FindAsync(id);

        //    if (entity == null)
        //        return NotFound();

        //    entity.IsActive = false;
        //    entity.UpdatedAt = DateTime.UtcNow;
        //    entity.UpdatedBy = User?.Identity?.Name ?? "system";

        //    await _context.SaveChangesAsync();

        //    return Ok("Deactivated successfully");
        //}
    }
}