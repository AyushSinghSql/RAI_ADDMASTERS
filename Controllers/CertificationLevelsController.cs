using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificationLevelsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CertificationLevelsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filter)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? certCd,
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.CertificationLevels.AsQueryable();

            if (!string.IsNullOrEmpty(certCd))
                query = query.Where(x => x.CertCd == certCd);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.CertLevelDesc.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CertLevelCd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY ID
        [HttpGet("{certCd}/{certLevelCd}")]
        public async Task<IActionResult> Get(string certCd, string certLevelCd)
        {
            var item = await _context.CertificationLevels
                .FindAsync(certLevelCd, certCd);

            if (item == null) return NotFound();

            return Ok(item);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CertificationLevel model)
        {
            model.ShowLookupFl = model.ShowLookupFl?.ToUpper().Substring(0, 1);
            model.TimeStamp = DateTime.UtcNow;

            _context.CertificationLevels.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{certCd}/{certLevelCd}")]
        public async Task<IActionResult> Update(string certCd, string certLevelCd, CertificationLevel model)
        {
            var db = await _context.CertificationLevels
                .FindAsync(certLevelCd, certCd);

            if (db == null) return NotFound();

            db.CertLevelDesc = model.CertLevelDesc;
            db.ShowLookupFl = model.ShowLookupFl?.ToUpper().Substring(0, 1);
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE
        [HttpDelete("{certCd}/{certLevelCd}")]
        public async Task<IActionResult> Delete(string certCd, string certLevelCd)
        {
            // 🔍 Check dependency
            var isUsed = await _context.VendorCertifications
                .AnyAsync(x => x.CertCd == certCd && x.CertLevelCd == certLevelCd);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete certification level. It is used in vendor certifications."
                });
            }

            var entity = await _context.CertificationLevels
                .FindAsync(certLevelCd, certCd);

            if (entity == null)
                return NotFound();

            _context.CertificationLevels.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Delete failed: This record is in use.");
            }

            return Ok("Deleted successfully");
        }
    }
}
