using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisaTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VisaTypeController(MydatabaseContext context)
        {
            _context = context;
        }


        // ✅ Create
        [HttpPost]
        public async Task<IActionResult> Create(VisaTypeDto dto)
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
                var exists = await _context.VisaTypes.AnyAsync(x => x.VisaTypeCode == dto.VisaTypeCode);

                if (exists)
                {
                    return Conflict($"Visa Type with ID '{dto.VisaTypeCode}' already exists.");
                }

                string currentUser = "SystemUser";
                var entity = dto.ToEntity(currentUser);

                _context.VisaTypes.Add(entity);
                await _context.SaveChangesAsync();

                return Ok("Visa Type created successfully.");
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
            var query = _context.VisaTypes.AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.VisaTypeCode)
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
        public async Task<IActionResult> Get(string visaTypeCode)
        {
            var data = await _context.VisaTypes.Where(x => x.VisaTypeCode == visaTypeCode).Select(x => x.ToDto()).FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound("Visa Type not found.");
            }

            return Ok(data);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] VisaTypeDto dto, [FromQuery] string updatedBy)
        {
            if (id != dto.VisaTypeCode)
            {
                return BadRequest();
            }
            var existing = await _context.VisaTypes.FirstOrDefaultAsync(x => x.VisaTypeCode == dto.VisaTypeCode);
            if (existing == null)
            {
                return NotFound("Visa Type not found.");
            }

            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;

            existing.UpdatedBy = dto.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Visa Type updated successfully." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "A database error occurred during the update.");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var visaId = await _context.VisaTypes.FindAsync(id);

            if (visaId == null)
            {
                return NotFound();
            }

            _context.VisaTypes.Remove(visaId);
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