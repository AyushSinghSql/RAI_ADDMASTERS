using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/ve-apvl-audit")]
    public class VeApvlAuditController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VeApvlAuditController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VeApvlAuditHs
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY VENDOR
        [HttpGet("vendor/{vendId}")]
        public async Task<IActionResult> GetByVendor(string vendId)
        {
            var data = await _context.VeApvlAuditHs
                .Where(x => x.VendId == vendId)
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ INSERT AUDIT
        [HttpPost]
        public async Task<IActionResult> Create(VeApvlAuditHs dto)
        {
            if (dto.AuditKey == 0)
                return BadRequest("AuditKey is required");

            dto.TimeStamp = DateTime.UtcNow;

            _context.VeApvlAuditHs.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
    }
}
