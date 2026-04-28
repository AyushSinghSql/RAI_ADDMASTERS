using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
namespace PlanningAPI.Controllers
{

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    public class CustNotesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustNotesController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (with basic protection)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustNotes>>> GetAll(int page = 1, int pageSize = 100)
        {
            page = Math.Max(1, page);
            pageSize = Math.Min(pageSize, 500);

            var data = await _context.CustNotes
                .AsNoTracking()
                .OrderBy(x => x.CustId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET ONE
        [HttpGet("{custId}/{companyId}")]
        public async Task<ActionResult<CustNotes>> Get(string custId, string companyId)
        {
            custId = Normalize(custId);
            companyId = Normalize(companyId);

            var entity = await _context.CustNotes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustId == custId && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustNotes model)
        {
            try
            {
                ValidateForCreate(model);

                var exists = await _context.CustNotes
                    .AnyAsync(x => x.CustId == model.CustId && x.CompanyId == model.CompanyId);

                if (exists)
                    return Conflict(new { message = "Duplicate record." });

                model.TimeStamp = DateTime.UtcNow;
                model.RowVersion = 1;

                _context.CustNotes.Add(model);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Get),
                    new { custId = model.CustId, companyId = model.CompanyId },
                    model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ UPDATE (strict ERP rules)
        [HttpPut("{custId}/{companyId}")]
        public async Task<IActionResult> Update(string custId, string companyId, CustNotes model)
        {
            try
            {
                custId = Normalize(custId);
                companyId = Normalize(companyId);

                ValidateForUpdate(custId, companyId, model);

                var entity = await _context.CustNotes
                    .FirstOrDefaultAsync(x => x.CustId == custId && x.CompanyId == companyId);

                if (entity == null)
                    return NotFound(new { message = "Record not found." });

                // 🔒 Concurrency check
                if (entity.RowVersion != model.RowVersion)
                    return Conflict(new { message = "Record has been modified by another user." });

                // Update fields
                entity.NotesTx = model.NotesTx;
                entity.ModifiedBy = model.ModifiedBy;
                entity.TimeStamp = DateTime.UtcNow;
                entity.RowVersion = (entity.RowVersion ?? 0) + 1;

                await _context.SaveChangesAsync();

                return Ok(entity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ DELETE (safe delete)
        [HttpDelete("{custId}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, string companyId)
        {
            custId = Normalize(custId);
            companyId = Normalize(companyId);

            var entity = await _context.CustNotes
                .FirstOrDefaultAsync(x => x.CustId == custId && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            _context.CustNotes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully." });
        }
        [NonAction]
        public string Normalize(string value)
    => value?.Trim().ToUpperInvariant();

        [NonAction]
        public void ValidateForCreate(CustNotes model)
        {
            if (model == null)
                throw new ArgumentException("Request body is required.");

            model.CustId = Normalize(model.CustId);
            model.CompanyId = Normalize(model.CompanyId);
            model.ModifiedBy = model.ModifiedBy?.Trim();

            if (string.IsNullOrWhiteSpace(model.CustId))
                throw new ArgumentException("cust_id is required.");

            if (model.CustId.Length > 12)
                throw new ArgumentException("cust_id max length is 12.");

            if (string.IsNullOrWhiteSpace(model.CompanyId))
                throw new ArgumentException("company_id is required.");

            if (model.CompanyId.Length > 10)
                throw new ArgumentException("company_id max length is 10.");

            if (string.IsNullOrWhiteSpace(model.ModifiedBy))
                throw new ArgumentException("modified_by is required.");

            if (model.ModifiedBy.Length > 20)
                throw new ArgumentException("modified_by max length is 20.");

            if (model.NotesTx?.Length > 1000000) // safeguard
                throw new ArgumentException("notes_tx too large.");
        }
        [NonAction]
        public void ValidateForUpdate(string custId, string companyId, CustNotes model)
        {
            if (model == null)
                throw new ArgumentException("Request body is required.");

            if (custId != model.CustId || companyId != model.CompanyId)
                throw new ArgumentException("Primary key cannot be modified.");

            ValidateForCreate(model);
        }
    }
}
