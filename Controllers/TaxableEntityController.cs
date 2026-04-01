using PlanningAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxableEntityController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public TaxableEntityController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] TaxableEntityDto dto)
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
                var exists = await _context.TaxableEntites.AnyAsync(x => x.TaxableId == dto.TaxableId);

                if (exists)
                {
                    return Conflict($"Taxable Entity with ID '{dto.TaxableId}' already exists.");
                }

                var duplicateTax = await _context.TaxableEntites.AnyAsync(x => x.TaxId == dto.TaxId && x.CompanyId == dto.CompanyId);

                if (duplicateTax)
                {
                    return BadRequest($"Tax ID '{dto.TaxId}' is already registered for Company '{dto.CompanyId}'.");
                }

                string currentUser = "SystemUser";
                var entity = dto.ToEntity(currentUser);

                _context.TaxableEntites.Add(entity);
                await _context.SaveChangesAsync();

                return Ok("Taxable Entity created successfully.");
            }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, "A database error occurred while saving the entity.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.TaxableEntites.AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.TaxableId)
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
        public async Task<IActionResult> Get(int taxableId)
        {
            var data = await _context.TaxableEntites.Where(x => x.TaxableId == taxableId).Select(x => x.ToDto()).FirstOrDefaultAsync();

            if(data == null)
            {
                return NotFound("Taxable Entity not found.");
            }

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaxableEntityDto dto, [FromQuery] string modifiedBy)
        {
            if (id != dto.TaxableId)
            {
                return BadRequest();
            }
            var existing = await _context.TaxableEntites.FirstOrDefaultAsync(x => x.TaxableId == dto.TaxableId);
            if(existing == null)
            {
                return NotFound("Taxable entity not found.");
            }

            if(existing.TaxId != dto.TaxId || existing.CompanyId != dto.CompanyId)
            {
                var duplicate = await _context.TaxableEntites.AnyAsync(x =>
                    x.TaxId == dto.TaxId &&
                    x.CompanyId == dto.CompanyId &&
                    x.TaxId != dto.TaxId
                );

                if (duplicate)
                {
                    return BadRequest("This Tax ID is already registered for this Company. ");
                }
            }

            existing.TaxableName = dto.TaxableName;
            existing.TaxId = dto.TaxId;
            existing.CompanyId = dto.CompanyId;
            existing.ActiveFlag = dto.ActiveFlag;

            existing.ModifiedBy = modifiedBy;
            existing.ModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Taxable Entity updated successfully." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "A database error occurred during the update.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxableEntity(int id)
        {
            var taxability = await _context.TaxableEntites.FindAsync(id);

            if(taxability == null)
            {
                return NotFound();
            }

            _context.TaxableEntites.Remove(taxability);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
