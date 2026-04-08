using PlanningAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcctGrpController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AcctGrpController(MydatabaseContext context)
        {
            _context = context;
        }

        // =========================
        // ✅ CREATE
        // =========================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AcctGrpCd model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var exists = await _context.AcctGrps.AnyAsync(x =>
                    x.AcctGrpCode == model.AcctGrpCode);

                if (exists)
                {
                    return BadRequest(new
                    {
                        message = "Account Group already exists for this company."
                    });
                }

                _context.AcctGrps.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account Group created successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // ✅ UPDATE
        // =========================
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] AcctGrpCd model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var existing = await _context.AcctGrps.FirstOrDefaultAsync(x =>
                    x.AcctGrpCode == model.AcctGrpCode);

                if (existing == null)
                    return NotFound(new { message = "Account Group not found." });

                // Update fields (skip PK)
                existing.AcctGrpDesc = model.AcctGrpDesc;
                existing.ModifiedBy = model.ModifiedBy;
                existing.TimeStamp = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Account Group updated successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // ✅ DELETE
        // =========================
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string acctGrpCd)
        {
            if (string.IsNullOrWhiteSpace(acctGrpCd))
                return BadRequest(new { message = "Account Group Code and Company Id are required." });

            try
            {
                var existing = await _context.AcctGrps.FirstOrDefaultAsync(x =>
                    x.AcctGrpCode == acctGrpCd);

                if (existing == null)
                    return NotFound(new { message = "Account Group not found." });

                _context.AcctGrps.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account Group deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    message = "Cannot delete this record as it is referenced in other data."
                });
            }
        }

        // =========================
        // ✅ GET ALL
        // =========================
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AcctGrps
                .OrderBy(x => x.AcctGrpCode)
                .ToListAsync();

            return Ok(new { data });
        }

        // =========================
        // ✅ GET BY ID (Composite Key)
        // =========================
        [HttpGet("get")]
        public async Task<IActionResult> Get(string acctGrpCd, string companyId)
        {
            var data = await _context.AcctGrps.FirstOrDefaultAsync((System.Linq.Expressions.Expression<Func<AcctGrpCd, bool>>)(x =>
                x.AcctGrpCode == acctGrpCd &&
                x.CompanyId == companyId));

            if (data == null)
                return NotFound(new { message = "Account Group not found." });

            return Ok(new { data });
        }

        // =========================
        // ✅ SEARCH
        // =========================
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? acctGrpCd,
            string? acctGrpDesc,
            string? companyId)
        {
            var query = _context.AcctGrps.AsQueryable();

            if (!string.IsNullOrWhiteSpace(acctGrpCd))
                query = query.Where(x => x.AcctGrpCode.Contains(acctGrpCd));

            if (!string.IsNullOrWhiteSpace(acctGrpDesc))
                query = query.Where(x => x.AcctGrpDesc.Contains(acctGrpDesc));

            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var result = await query.ToListAsync();

            return Ok(new { data = result });
        }


        // =========================
        // ✅ SEARCH with Pagination
        // =========================
        [HttpGet("search_paged")]
        public async Task<IActionResult> Search(
        string? acctGrpCd,
        string? acctGrpDesc,
        string? companyId,
        int pageNumber = 1,
        int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest(new { message = "Invalid pagination parameters." });

            var query = _context.AcctGrps.AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(acctGrpCd))
                query = query.Where(x => x.AcctGrpCode.Contains(acctGrpCd));

            if (!string.IsNullOrWhiteSpace(acctGrpDesc))
                query = query.Where(x => x.AcctGrpDesc.Contains(acctGrpDesc));

            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.AcctGrpCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
        // ✅ ERROR HANDLER
        // =========================
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "23505": // unique_violation
                        return "Duplicate record exists. Please use a different value.";

                    case "23503": // foreign_key_violation
                        return "Cannot delete or update this record because it is referenced elsewhere.";

                    case "23502": // not_null_violation
                        return "Required field is missing. Please check your input.";

                    case "22001": // string_data_right_truncation
                        return "Input value is too long for the field.";

                    case "22003": // numeric_value_out_of_range
                        return "Numeric value is out of allowed range.";

                    case "22007": // invalid_datetime_format
                        return "Invalid date or time format.";

                    case "22008": // datetime_field_overflow
                        return "Date value is out of valid range.";

                    default:
                        return $"Database error occurred. {pgEx.MessageText}";
                }
            }

            // Generic fallback
            return "An error occurred while saving data.";
        }
    }
}
