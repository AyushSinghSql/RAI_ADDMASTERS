using PlanningAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public RaceController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] RaceDto dto)
        {
            if(dto == null)
            {
                return BadRequest("Request body cannot be empty.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var exists = await _context.Race.AnyAsync(x => x.RaceId == dto.RaceId);

                if (exists)
                {
                    return Conflict($"Race with ID '{dto.RaceId}' already exists.");
                }

                string currentUser = "SystemUser";
                var entity = dto.ToEntity(currentUser);

                _context.Race.Add(entity);
                await _context.SaveChangesAsync();

                return Ok("Race created successfully. ");

            } catch (DbUpdateException ex)
            {
                return StatusCode(500, "A database error occurred while saving the entity.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error: {ex.Message}");
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Race.AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query.OrderBy(x => x.RaceId).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => x.ToDto()).ToListAsync();

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount,
                data
            });
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get(string raceId)
        {
            var data = await _context.Race.Where(x => x.RaceId == raceId).Select(XmlConfigurationExtensions => XmlConfigurationExtensions.ToDto()).FirstOrDefaultAsync();

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RaceDto dto, [FromQuery] string modifiedBy)
        {
            if(id != dto.RaceId)
            {
                return BadRequest();
            }

            var existing = await _context.Race.FirstOrDefaultAsync(x => x.RaceId == dto.RaceId);

            if(existing == null)
            {
                return NotFound("Race not found");
            }

            existing.Description = dto.Description;
            existing.ActiveFlag = dto.ActiveFlag;
            existing.ModifiedBy = modifiedBy;
            existing.ModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Race updated successfully. ");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "A database error occured during the update");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeteteRace(string id)
        {
            var exisits = await _context.Race.FindAsync(id);

            if(exisits == null)
            {
                return NotFound();
            }

            _context.Race.Remove(exisits);
            await _context.SaveChangesAsync();


            return NoContent();
        }
    }
}
