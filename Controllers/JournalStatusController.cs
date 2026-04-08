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

        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsertStatus(List<JournalStatusDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided");

            // 🔥 1. Validate Periods (single query)
            var periods = await _context.AccountingPeriods
                .Where(x => dtos.Select(d => d.FyCd).Contains(x.FyCd)
                         && dtos.Select(d => d.PeriodNo).Contains(x.PeriodNo))
                .Select(x => new { x.FyCd, x.PeriodNo })
                .ToListAsync();

            var validPeriods = periods
                .Select(x => $"{x.FyCd}-{x.PeriodNo}")
                .ToHashSet();

            var invalid = dtos
                .Where(d => !validPeriods.Contains($"{d.FyCd}-{d.PeriodNo}"))
                .ToList();

            if (invalid.Any())
            {
                return BadRequest(new
                {
                    message = "Invalid fiscal year / period",
                    invalid
                });
            }

            // 🔥 2. Load existing records in ONE query
            var keys = dtos.Select(d => new
            {
                d.JournalCode,
                d.FyCd,
                d.PeriodNo,
                d.CompanyId
            }).ToList();
            var journalCodes = dtos.Select(x => x.JournalCode).Distinct().ToList();
            var fyCds = dtos.Select(x => x.FyCd).Distinct().ToList();

            var existing = await _context.JournalStatuses
                .Where(x =>
                    journalCodes.Contains(x.JournalCode) &&
                    fyCds.Contains(x.FyCd))
                .ToListAsync();

            //var existingDict = existing.ToDictionary(
            //    x => $"{x.JournalCode}|{x.FyCd}|{x.PeriodNo}|{x.CompanyId}");
            // 🔥 Convert to lookup for fast match
            var existingDict = existing.ToDictionary(
                x => $"{x.JournalCode}-{x.FyCd}-{x.PeriodNo}-{x.CompanyId}");

            var toInsert = new List<JournalStatus>();
            var toUpdate = new List<JournalStatus>();

            // 🔥 3. Process
            foreach (var dto in dtos)
            {
                var key = $"{dto.JournalCode}-{dto.FyCd}-{dto.PeriodNo}-{dto.CompanyId}";

                if (existingDict.TryGetValue(key, out var entity))
                {
                    // UPDATE
                    entity.IsOpen = dto.IsOpen;
                    entity.ModifiedBy = dto.ModifiedBy;
                    entity.TimeStamp = DateTime.UtcNow;

                    toUpdate.Add(entity);
                }
                else
                {
                    // INSERT
                    toInsert.Add(new JournalStatus
                    {
                        JournalCode = dto.JournalCode,
                        FyCd = dto.FyCd,
                        PeriodNo = dto.PeriodNo,
                        CompanyId = dto.CompanyId,
                        IsOpen = dto.IsOpen,
                        ModifiedBy = dto.ModifiedBy,
                        TimeStamp = DateTime.UtcNow
                    });
                }
            }

            // 🔥 4. Save in batch
            if (toInsert.Any())
                await _context.JournalStatuses.AddRangeAsync(toInsert);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Bulk upsert successful",
                inserted = toInsert.Count,
                updated = toUpdate.Count
            });
        }

        [HttpGet("{fyCd}/{periodNo}/{companyId}")]
        public async Task<IActionResult> GetStatus(string fyCd, int periodNo, string companyId)
        {
            var data = await _context.JournalStatuses
                .Where(x => x.FyCd == fyCd &&
                            x.PeriodNo == periodNo &&
                            x.CompanyId == companyId)
                .Select(x => new JournalStatusDto
                {
                    JournalCode = x.JournalCode,
                    FyCd = x.FyCd,
                    PeriodNo = x.PeriodNo,
                    CompanyId = x.CompanyId,
                    IsOpen = x.IsOpen,
                    JournalDesc = x.JournalCodeRef.JournalDesc
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
