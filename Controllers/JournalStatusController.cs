using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/journal-status")]
    public class JournalStatusController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public JournalStatusController(MydatabaseContext context)
        {
            _context = context;
        }

        // 🔓 Open Journal
        [HttpPost("open")]
        public async Task<IActionResult> Open(JournalStatusDto dto)
        {
            return await UpsertStatus(dto, "Y");
        }

        // 🔒 Close Journal
        [HttpPost("close")]
        public async Task<IActionResult> Close(JournalStatusDto dto)
        {
            return await UpsertStatus(dto, "N");
        }

        private async Task<IActionResult> UpsertStatus(JournalStatusDto dto, string status)
        {
            // ✅ Validate period exists
            var periodExists = await _context.AccountingPeriods
                .AnyAsync(x => x.FyCd == dto.FyCd && x.PeriodNo == dto.PeriodNo);

            if (!periodExists)
                return BadRequest("Invalid fiscal year or period");

            var entity = await _context.JournalStatuses.FindAsync(
                dto.JournalCode, dto.FyCd, dto.PeriodNo, dto.CompanyId);

            if (entity == null)
            {
                entity = new JournalStatus
                {
                    JournalCode = dto.JournalCode,
                    FyCd = dto.FyCd,
                    PeriodNo = dto.PeriodNo,
                    CompanyId = dto.CompanyId,
                    IsOpen = status,
                    ModifiedBy = dto.ModifiedBy,
                    TimeStamp = DateTime.UtcNow
                };

                _context.JournalStatuses.Add(entity);
            }
            else
            {
                entity.IsOpen = status;
                entity.ModifiedBy = dto.ModifiedBy;
                entity.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                dto.JournalCode,
                dto.FyCd,
                dto.PeriodNo,
                dto.CompanyId,
                Status = status == "Y" ? "Opened" : "Closed"
            });
        }

        [HttpGet("{fyCd}/{periodNo}/{companyId}")]
        public async Task<IActionResult> GetStatus(string fyCd, int periodNo, string companyId)
        {
            var data = await _context.JournalStatuses
                .Where(x => x.FyCd == fyCd && x.PeriodNo == periodNo && x.CompanyId == companyId)
                .ToListAsync();

            return Ok(data);
        }
    }
}
