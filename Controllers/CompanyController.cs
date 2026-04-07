using PlanningAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CompanyController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpGet("audit")]
        public async Task<IActionResult> GetAudit(string tableName)
        {
            var data = await _context.AuditLogs
                .Where(x => x.TableName == tableName)
                .OrderByDescending(x => x.TimeStamp)
                .Take(100)
                .ToListAsync();

            return Ok(data);
        }
        // =========================
        // CREATE
        // =========================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CompanyDto dto, [FromQuery] string modifiedBy)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var exists = await _context.Companies
                    .AnyAsync(x => x.CompanyId == dto.CompanyId);

                if (exists)
                    return BadRequest(new { message = "Company already exists." });

                var entity = dto.ToEntity(modifiedBy);

                _context.Companies.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Company created successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // UPDATE
        // =========================
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CompanyDto dto, [FromQuery] string modifiedBy)
        {
            var existing = await _context.Companies
                .FirstOrDefaultAsync(x => x.CompanyId == dto.CompanyId);

            if (existing == null)
                return NotFound(new { message = "Company not found." });

            existing.CompanyName = dto.CompanyName;
            existing.CompanyShortName = dto.CompanyShortName;
            existing.ActiveFlag = dto.ActiveFlag;
            existing.ModifiedBy = modifiedBy;
            existing.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Company updated successfully." });
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string companyId)
        {
            var existing = await _context.Companies
                .FirstOrDefaultAsync(x => x.CompanyId == companyId);

            if (existing == null)
                return NotFound(new { message = "Company not found." });

            try
            {
                _context.Companies.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Company deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // GET ALL (Pagination)
        // =========================
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Companies.AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CompanyId)
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

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("get")]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.Companies.Include(p=>p.TaxableEntity)
                .Where(x => x.CompanyId == companyId)
                .Select(x => x.ToDto())
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Company not found." });

            return Ok(data);
        }

        // =========================
        // SEARCH
        // =========================
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? companyId,
            string? companyName,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId.Contains(companyId));

            if (!string.IsNullOrWhiteSpace(companyName))
                query = query.Where(x => x.CompanyName.Contains(companyName));

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CompanyId)
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

        // =========================
        // ERROR HANDLER
        // =========================
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505") // Unique violation
                {
                    return pgEx.ConstraintName switch
                    {
                        "uq_company_companyid" => "Company ID already exists.",
                        "uq_company_name" => "Company name already exists.",
                        "uq_company_email" => "Email already exists.",
                        "taxable_entity_tax_id_key" => "Taxable Entity already Mapped with some Other Company.",
                        _ => "Duplicate value already exists."
                    };
                }

                return pgEx.SqlState switch
                {
                    "23503" => "This record is referenced in other data.",
                    "23502" => "A required field is missing.",
                    _ => "A database error occurred."
                };
            }

            return "An error occurred while saving data.";
        }
    }
}
