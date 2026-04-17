using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VendorSecurityClearanceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorSecurityClearanceController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? vendEmplId,
            string? companyId,
            string? secClrCode,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.VendorSecurityClearances.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(vendEmplId))
                query = query.Where(x => x.VendEmplId == vendEmplId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(secClrCode))
                query = query.Where(x => x.SecClrCode == secClrCode);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.VendId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        // ✅ GET BY ID
        [HttpGet("{vendEmplId}/{vendId}/{secClrCode}/{companyId}")]
        public async Task<IActionResult> GetById(
            string vendEmplId,
            string vendId,
            string secClrCode,
            string companyId)
        {
            var entity = await _context.VendorSecurityClearances.FindAsync(
                vendEmplId, vendId, secClrCode, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorSecurityClearance model)
        {
            model.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorSecurityClearances.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(VendorSecurityClearance model)
        {
            var existing = await _context.VendorSecurityClearances.FindAsync(
                model.VendEmplId,
                model.VendId,
                model.SecClrCode,
                model.CompanyId);

            if (existing == null)
                return NotFound();

            existing.AgencyName = model.AgencyName;
            existing.RequestDate = model.RequestDate;
            existing.EffectiveDate = model.EffectiveDate;
            existing.ExpiryDate = model.ExpiryDate;
            existing.ReinvestigateDate = model.ReinvestigateDate;
            existing.InvestigateBy = model.InvestigateBy;
            existing.InvestigateType = model.InvestigateType;
            existing.InvestigateDate = model.InvestigateDate;

            existing.ModifiedBy = model.ModifiedBy;
            existing.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{secClrCode}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            string secClrCode,
            string companyId)
        {
            var entity = await _context.VendorSecurityClearances.FindAsync(
                vendEmplId, vendId, secClrCode, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorSecurityClearances.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
