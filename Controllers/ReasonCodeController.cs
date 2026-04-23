using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonCodeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ReasonCodeController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? companyId,
            string? usedCd,
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.ReasonCodes.AsQueryable();

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(usedCd))
                query = query.Where(x => x.SRsnWhUsedCd == usedCd);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x =>
                    x.RsnCd.Contains(search) ||
                    x.RsnDesc.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.RsnCd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{rsnCd}/{usedCd}/{companyId}")]
        public async Task<IActionResult> Get(
            string rsnCd,
            string usedCd,
            string companyId)
        {
            var data = await _context.ReasonCodes
                .FindAsync(rsnCd, usedCd, companyId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(ReasonCode model)
        {
            var exists = await _context.ReasonCodes.AnyAsync(x =>
                x.RsnCd == model.RsnCd &&
                x.SRsnWhUsedCd == model.SRsnWhUsedCd &&
                x.CompanyId == model.CompanyId);

            if (exists)
                return BadRequest("Reason Code already exists");

            // Normalize flags
            model.UpdLastCtDtFl = model.UpdLastCtDtFl == "Y" ? "Y" : "N";

            model.TimeStamp = DateTime.UtcNow;

            _context.ReasonCodes.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{rsnCd}/{usedCd}/{companyId}")]
        public async Task<IActionResult> Update(
            string rsnCd,
            string usedCd,
            string companyId,
            ReasonCode model)
        {
            var db = await _context.ReasonCodes
                .FindAsync(rsnCd, usedCd, companyId);

            if (db == null)
                return NotFound();

            db.RsnDesc = model.RsnDesc;
            db.UpdLastCtDtFl = model.UpdLastCtDtFl == "Y" ? "Y" : "N";
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE
        [HttpDelete("{rsnCd}/{usedCd}/{companyId}")]
        public async Task<IActionResult> Delete(
            string rsnCd,
            string usedCd,
            string companyId)
        {
            var db = await _context.ReasonCodes
                .FindAsync(rsnCd, usedCd, companyId);

            if (db == null)
                return NotFound();

            _context.ReasonCodes.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(
            string companyId,
            string? usedCd)
        {
            var query = _context.ReasonCodes
                .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(usedCd))
                query = query.Where(x => x.SRsnWhUsedCd == usedCd);

            var data = await query
                .Select(x => new
                {
                    value = x.RsnCd,
                    label = x.RsnDesc,
                    used = x.SRsnWhUsedCd
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
