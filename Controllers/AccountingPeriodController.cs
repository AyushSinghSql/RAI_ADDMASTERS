using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using System.ComponentModel.Design;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/accounting-period")]
    public class AccountingPeriodController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AccountingPeriodController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AccountingPeriods
                .OrderBy(x => x.FyCd)
                .ThenBy(x => x.PeriodNo)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY FY
        [HttpGet("{fyCd}")]
        public async Task<IActionResult> GetByFy(string fyCd, string CompanyId)
        {
            var data = await _context.AccountingPeriods
                .Where(x => x.FyCd == fyCd && x.CompanyId == CompanyId)
                .OrderBy(x => x.PeriodNo)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(AccountingPeriodDto dto)
        {
            var exists = await _context.AccountingPeriods
                .AnyAsync(x => x.FyCd == dto.FyCd && x.PeriodNo == dto.PeriodNo && x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Period already exists");

            var entity = new AccountingPeriod
            {
                FyCd = dto.FyCd,
                PeriodNo = dto.PeriodNo,
                StatusCd = dto.StatusCd,
                PeriodEndDate = dto.PeriodEndDate,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow,
                IsAdjustment = dto.IsAdjustment,
                AdjustmentCode = dto.AdjustmentCode,
                AdjustmentEndDate = dto.AdjustmentEndDate,
                CompanyId = dto.CompanyId
            };

            _context.AccountingPeriods.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ UPDATE
        [HttpPut("{fyCd}/{periodNo}")]
        public async Task<IActionResult> Update(string fyCd, int periodNo, AccountingPeriodDto dto)
        {
            var entity = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.StatusCd = dto.StatusCd;
            entity.PeriodEndDate = dto.PeriodEndDate;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.IsAdjustment = dto.IsAdjustment;
            entity.AdjustmentCode = dto.AdjustmentCode;
            entity.AdjustmentEndDate = dto.AdjustmentEndDate;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{fyCd}/{periodNo}/{companyId}")]
        public async Task<IActionResult> Delete(string fyCd, int periodNo, string companyId)
        {
            var entity = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, companyId);

            if (entity == null)
                return NotFound();

            _context.AccountingPeriods.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<AccountingPeriodDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided");

            var sql = @"
        INSERT INTO public.accounting_period (
            fy_cd, period_no, status_cd, period_end_date,
            modified_by, time_stamp,
            is_adjustment, adjustment_code, adjustment_end_date
        )
        SELECT 
            x.fy_cd, x.period_no, x.status_cd, x.period_end_date,
            x.modified_by, NOW(),
            x.is_adjustment, x.adjustment_code, x.adjustment_end_date
        FROM jsonb_to_recordset(@data) AS x(
            fy_cd VARCHAR(6),
            period_no INT,
            status_cd VARCHAR(1),
            period_end_date DATE,
            modified_by VARCHAR(20),
            is_adjustment VARCHAR(1),
            adjustment_code VARCHAR(1),
            adjustment_end_date DATE
        )
        ON CONFLICT (fy_cd, period_no)
        DO UPDATE SET
            status_cd = EXCLUDED.status_cd,
            period_end_date = EXCLUDED.period_end_date,
            modified_by = EXCLUDED.modified_by,
            time_stamp = NOW(),
            is_adjustment = EXCLUDED.is_adjustment,
            adjustment_code = EXCLUDED.adjustment_code,
            adjustment_end_date = EXCLUDED.adjustment_end_date;
    ";

            var jsonData = System.Text.Json.JsonSerializer.Serialize(dtos);

            await _context.Database.ExecuteSqlRawAsync(
                sql,
                new Npgsql.NpgsqlParameter("@data", jsonData)
            );

            return Ok("Bulk upsert completed");
        }

        [HttpGet("calendar/{fyCd}/{companyId}")]
        public async Task<IActionResult> GetCalendar(string fyCd, string companyId)
        {
            var data = await _context.AccountingPeriods
                .Where(x => x.FyCd == fyCd && x.CompanyId == companyId)
                .OrderBy(x => x.PeriodNo)
                .Select(x => new
                {
                    x.FyCd,
                    x.PeriodNo,
                    x.PeriodEndDate,
                    x.StatusCd,
                    x.IsAdjustment
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("lock")]
        public async Task<IActionResult> LockPeriod(string fyCd, int periodNo, string modifiedBy, string companyId)
        {
            var entity = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, companyId);

            if (entity == null)
                return NotFound();

            entity.StatusCd = "C";
            entity.ModifiedBy = modifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Period locked");
        }

        [HttpPost("unlock")]
        public async Task<IActionResult> UnlockPeriod(string fyCd, int periodNo, string modifiedBy, string companyId)
        {
            var entity = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, companyId);

            if (entity == null)
                return NotFound();

            entity.StatusCd = "O";
            entity.ModifiedBy = modifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Period unlocked");
        }

        [HttpPost("close-period")]
        public async Task<IActionResult> ClosePeriod(string fyCd, int periodNo, string companyId)
        {
            var period = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, companyId);

            if (period == null)
                return NotFound();

            period.StatusCd = "C";

            // 🔥 Close sub-periods
            var subPeriods = await _context.SubPeriods
                .Where(x => x.FyCd == fyCd && x.PeriodNo == periodNo)
                .ToListAsync();

            foreach (var sp in subPeriods)
                sp.StatusCd = "C";

            await _context.SaveChangesAsync();

            return Ok("Period locked");
        }
        [HttpPost("close-and-open-next")]
        public async Task<IActionResult> CloseAndOpenNext(string fyCd, int periodNo, string modifiedBy, string companyId)
        {
            var current = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo, companyId);

            var next = await _context.AccountingPeriods
                .FindAsync(fyCd, periodNo + 1);

            if (current == null || next == null)
                return BadRequest("Invalid periods");

            // Close current
            current.StatusCd = "C";

            // Open next
            next.StatusCd = "O";

            current.ModifiedBy = modifiedBy;
            next.ModifiedBy = modifiedBy;

            current.TimeStamp = DateTime.UtcNow;
            next.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Period rolled forward");
        }

        [HttpPost("set-journals-status")]
        public async Task<IActionResult> SetAllJournalStatus(
    string fyCd, int periodNo, string companyId, string status, string modifiedBy)
        {
            var journals = await _context.JournalStatuses
                .Where(x => x.FyCd == fyCd &&
                            x.PeriodNo == periodNo &&
                            x.CompanyId == companyId)
                .ToListAsync();

            foreach (var j in journals)
            {
                j.IsOpen = status;
                j.ModifiedBy = modifiedBy;
                j.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok($"{journals.Count} journals updated");
        }
        [NonAction]
        public async Task<bool> IsPeriodOpen(string fyCd, int periodNo)
        {
            return await _context.AccountingPeriods
                .AnyAsync(x =>
                    x.FyCd == fyCd &&
                    x.PeriodNo == periodNo &&
                    x.StatusCd == "O");
        }

    }
}
