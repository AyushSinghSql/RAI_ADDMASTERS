using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ModulesController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET: api/modules
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Modules
                .Select(x => new ModuleDto
                {
                    ModuleCd = x.ModuleCd,
                    CompanyId = x.CompanyId,
                    Name = x.Name,
                    Domain = x.Domain
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET: api/modules/{moduleCd}/{companyId}
        [HttpGet("{moduleCd}/{companyId}")]
        public async Task<IActionResult> Get(string moduleCd, string companyId)
        {
            var data = await _context.Modules
                .Where(x => x.ModuleCd == moduleCd && x.CompanyId == companyId)
                .Select(x => new ModuleDto
                {
                    ModuleCd = x.ModuleCd,
                    CompanyId = x.CompanyId,
                    Name = x.Name,
                    Domain = x.Domain
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Module not found." });

            return Ok(data);
        }

        // ✅ POST: api/modules
        [HttpPost]
        public async Task<IActionResult> Create(ModuleDto dto)
        {
            var entity = new Module
            {
                ModuleCd = dto.ModuleCd,
                CompanyId = dto.CompanyId,
                Name = dto.Name,
                Domain = dto.Domain,
                ModifiedBy = "system", // replace with logged-in user
                TimeStamp = DateTime.UtcNow
            };

            _context.Modules.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return CreatedAtAction(nameof(Get),
                new { moduleCd = entity.ModuleCd, companyId = entity.CompanyId },
                dto);
        }

        // ✅ PUT: api/modules/{moduleCd}/{companyId}
        [HttpPut("{moduleCd}/{companyId}")]
        public async Task<IActionResult> Update(string moduleCd, string companyId, ModuleDto dto)
        {
            var entity = await _context.Modules
                .FirstOrDefaultAsync(x => x.ModuleCd == moduleCd && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Module not found." });

            entity.Name = dto.Name;
            entity.Domain = dto.Domain;
            entity.ModifiedBy = "system"; // replace with user
            entity.TimeStamp = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Module updated successfully." });
        }

        // ✅ DELETE: api/modules/{moduleCd}/{companyId}
        [HttpDelete("{moduleCd}/{companyId}")]
        public async Task<IActionResult> Delete(string moduleCd, string companyId)
        {
            var entity = await _context.Modules
                .FirstOrDefaultAsync(x => x.ModuleCd == moduleCd && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Module not found." });

            _context.Modules.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Module deleted successfully." });
        }

        // ✅ Friendly Error Handler
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                    return "Duplicate value already exists.";

                if (pgEx.SqlState == "23503")
                    return "This record is referenced in other data.";

                if (pgEx.SqlState == "23502")
                    return "A required field is missing.";
            }

            return "An error occurred while saving data.";
        }
    }
}
