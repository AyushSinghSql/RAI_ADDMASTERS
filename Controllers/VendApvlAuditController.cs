using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/vendor-approval-audit")]
    public class VendApvlAuditController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendApvlAuditController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ Insert audit record
        [HttpPost]
        public async Task<IActionResult> AddAudit(VendApvlAuditDto dto)
        {
            var entity = new VendApvlAuditHs
            {
                EntrUserId = dto.EntrUserId,
                CompanyId = dto.CompanyId,
                FrmVendApprvlCd = dto.FrmVendApprvlCd,
                ToVendApprvlCd = dto.ToVendApprvlCd,
                FrmVendId = dto.FrmVendId,
                FrmPayVendId = dto.FrmPayVendId,
                FrmPayApprvlCd = dto.FrmPayApprvlCd,
                ToPayApprvlCd = dto.ToPayApprvlCd,
                ModifiedBy = dto.EntrUserId,
                TimeStamp = DateTime.UtcNow,
                RowVersion = 1
            };

            _context.VendApvlAuditHs.Add(entity);
            await _context.SaveChangesAsync();

            return Ok("Audit record created");
        }

        // ✅ Get all by company
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetByCompany(string companyId)
        {
            var data = await _context.VendApvlAuditHs
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ Get history by vendor
        [HttpGet("vendor/{vendId}")]
        public async Task<IActionResult> GetByVendor(string vendId)
        {
            var data = await _context.VendApvlAuditHs
                .Where(x => x.FrmVendId == vendId)
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }
    }
}
