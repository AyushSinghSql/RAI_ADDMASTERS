using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/sub-period-journal-status")]
    public class SubPeriodJournalStatusController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubPeriodJournalStatusController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET
        [HttpGet("{fyCd}/{periodNo}/{subPeriodNo}/{companyId}")]
        public async Task<IActionResult> Get(string fyCd, int periodNo, int subPeriodNo, string companyId)
        {
            var data = await _context.SubPeriodJournalStatuses
                .Where(x =>
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.SubPeriodNo == subPeriodNo &&
                    x.CompanyId == companyId)
                    .Select(x => new SubPeriodJournalStatusDto
                    {
                        JournalCode = x.JournalCode,
                        FyCd = x.FyCd,
                        PeriodNo = x.PeriodNo,
                        SubPeriodNo = x.SubPeriodNo,
                        IsOpen = x.IsOpen,
                        JournalDesc = x.JournalCodeRef.JournalDesc
                    })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(string companyId, SubPeriodJournalStatusDto dto)
        {
            var exists = await _context.SubPeriodJournalStatuses.AnyAsync(x =>
                x.JournalCode == dto.JournalCode &&
                x.FyCd == dto.FyCd &&
                x.PeriodNo == dto.PeriodNo &&
                x.SubPeriodNo == dto.SubPeriodNo &&
                x.CompanyId == companyId);

            if (exists)
                return BadRequest("Already exists");

            var entity = new SubPeriodJournalStatus
            {
                JournalCode = dto.JournalCode,
                FyCd = dto.FyCd,
                PeriodNo = dto.PeriodNo,
                SubPeriodNo = dto.SubPeriodNo,
                CompanyId = companyId,
                IsOpen = dto.IsOpen,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.SubPeriodJournalStatuses.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // 🚀 BULK UPSERT
        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(string companyId, List<SubPeriodJournalStatusBulkDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data");

            var sql = @"
            INSERT INTO public.sub_period_journal_status (
                journal_code, fy_cd, period_no, sub_period_no, company_id,
                is_open, modified_by, time_stamp
            )
            SELECT 
                x.journal_code, x.fy_cd, x.period_no, x.sub_period_no, @companyId,
                x.is_open, x.modified_by, NOW()
            FROM jsonb_to_recordset(@data) AS x(
                journal_code VARCHAR(3),
                fy_cd VARCHAR(6),
                period_no INT,
                sub_period_no INT,
                is_open VARCHAR(1),
                modified_by VARCHAR(20)
            )
            ON CONFLICT (journal_code, fy_cd, period_no, sub_period_no, company_id)
            DO UPDATE SET
                is_open = EXCLUDED.is_open,
                modified_by = EXCLUDED.modified_by,
                time_stamp = NOW();
        ";

            var jsonData = System.Text.Json.JsonSerializer.Serialize(dtos);

            await _context.Database.ExecuteSqlRawAsync(
                sql,
                new Npgsql.NpgsqlParameter("@data", jsonData),
                new Npgsql.NpgsqlParameter("@companyId", companyId)
            );

            return Ok("Bulk upsert done");
        }

        // 🔒 SET STATUS (Single Journal)
        [HttpPost("set-status")]
        public async Task<IActionResult> SetStatus(string companyId, SubPeriodJournalStatusDto dto)
        {
            var records = await _context.SubPeriodJournalStatuses
                .Where(x =>
                    x.FyCd == dto.FyCd &&
                    x.PeriodNo == dto.PeriodNo &&
                    x.SubPeriodNo == dto.SubPeriodNo &&
                    x.CompanyId == companyId &&
                    x.JournalCode == dto.JournalCode)
                .ToListAsync();

            if (!records.Any())
                return NotFound();

            foreach (var r in records)
            {
                r.IsOpen = dto.IsOpen;
                r.ModifiedBy = dto.ModifiedBy;
                r.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok($"{records.Count} updated");
        }

        // 🔥 SET ALL JOURNALS FOR SUB-PERIOD
        [HttpPost("set-all")]
        public async Task<IActionResult> SetAll(
            string fyCd,
            int periodNo,
            int subPeriodNo,
            string companyId,
            string status,
            string modifiedBy)
        {
            var records = await _context.SubPeriodJournalStatuses
                .Where(x =>
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.SubPeriodNo == subPeriodNo &&
                    x.CompanyId == companyId)
                .ToListAsync();

            foreach (var r in records)
            {
                r.IsOpen = status;
                r.ModifiedBy = modifiedBy;
                r.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok($"{records.Count} journals updated");
        }
    }
}
