using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificationStatusController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CertificationStatusController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? certCd,
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.CertificationStatuses.AsQueryable();

            if (!string.IsNullOrEmpty(certCd))
                query = query.Where(x => x.CertCd == certCd);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.CertStatusDesc.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CertStatusCd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY ID
        [HttpGet("{certCd}/{certStatusCd}")]
        public async Task<IActionResult> Get(string certCd, string certStatusCd)
        {
            var item = await _context.CertificationStatuses
                .FindAsync(certStatusCd, certCd);

            if (item == null) return NotFound();

            return Ok(item);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CertificationStatus model)
        {
            model.ShowLookupFl = model.ShowLookupFl?.ToUpper().Substring(0, 1);
            model.TimeStamp = DateTime.UtcNow;

            _context.CertificationStatuses.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{certCd}/{certStatusCd}")]
        public async Task<IActionResult> Update(string certCd, string certStatusCd, CertificationStatus model)
        {
            var db = await _context.CertificationStatuses
                .FindAsync(certStatusCd, certCd);

            if (db == null) return NotFound();

            db.CertStatusDesc = model.CertStatusDesc;
            db.ShowLookupFl = model.ShowLookupFl?.ToUpper().Substring(0, 1);
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE with validation
        [HttpDelete("{certCd}/{certStatusCd}")]
        public async Task<IActionResult> Delete(string certCd, string certStatusCd)
        {
            // 🔒 Check usage in vendor_certifications
            var isUsed = await _context.VendorCertifications
                .AnyAsync(x => x.CertCd == certCd && x.CertStatusCd == certStatusCd);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete certification status. It is used in vendor certifications."
                });
            }

            var entity = await _context.CertificationStatuses
                .FindAsync(certStatusCd, certCd);

            if (entity == null)
                return NotFound();

            try
            {
                _context.CertificationStatuses.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Delete failed: record is in use.");
            }

            return Ok("Deleted successfully");
        }


    }
}
