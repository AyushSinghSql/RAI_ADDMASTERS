using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/sub-period")]
    public class SubPeriodController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubPeriodController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET
        [HttpGet("{fyCd}/{periodNo}")]
        public async Task<IActionResult> Get(string fyCd, int periodNo)
        {
            var data = await _context.SubPeriods
                .Where(x => x.FyCd == fyCd && x.PeriodNo == periodNo)
                .OrderBy(x => x.SubPeriodNo)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(SubPeriodDto dto)
        {
            var exists = await _context.SubPeriods.AnyAsync(x =>
                x.FyCd == dto.FyCd &&
                x.PeriodNo == dto.PeriodNo &&
                x.SubPeriodNo == dto.SubPeriodNo);

            if (exists)
                return BadRequest("Sub-period already exists");

            var entity = new SubPeriod
            {
                FyCd = dto.FyCd,
                PeriodNo = dto.PeriodNo,
                SubPeriodNo = dto.SubPeriodNo,
                SubPeriodEndDate = dto.SubPeriodEndDate,
                StatusCd = dto.StatusCd,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow,
                IsAdjustment = dto.IsAdjustment,
                AdjustmentCode = dto.AdjustmentCode,
                AdjustmentEndDate = dto.AdjustmentEndDate
            };

            _context.SubPeriods.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ UPDATE
        [HttpPut("{fyCd}/{periodNo}/{subPeriodNo}")]
        public async Task<IActionResult> Update(string fyCd, int periodNo, int subPeriodNo, SubPeriodDto dto)
        {
            var entity = await _context.SubPeriods
                .FindAsync(fyCd, periodNo, subPeriodNo);

            if (entity == null)
                return NotFound();

            entity.SubPeriodEndDate = dto.SubPeriodEndDate;
            entity.StatusCd = dto.StatusCd;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.IsAdjustment = dto.IsAdjustment;
            entity.AdjustmentCode = dto.AdjustmentCode;
            entity.AdjustmentEndDate = dto.AdjustmentEndDate;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{fyCd}/{periodNo}/{subPeriodNo}")]
        public async Task<IActionResult> Delete(string fyCd, int periodNo, int subPeriodNo)
        {
            var entity = await _context.SubPeriods
                .FindAsync(fyCd, periodNo, subPeriodNo);

            if (entity == null)
                return NotFound();

            _context.SubPeriods.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<SubPeriodDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data");

            var sql = @"
        INSERT INTO public.sub_period (
            fy_cd, period_no, sub_period_no,
            sub_period_end_date, status_cd,
            modified_by, time_stamp,
            is_adjustment, adjustment_code, adjustment_end_date
        )
        SELECT 
            x.fy_cd, x.period_no, x.sub_period_no,
            x.sub_period_end_date, x.status_cd,
            x.modified_by, NOW(),
            x.is_adjustment, x.adjustment_code, x.adjustment_end_date
        FROM jsonb_to_recordset(@data) AS x(
            fy_cd VARCHAR(6),
            period_no INT,
            sub_period_no INT,
            sub_period_end_date DATE,
            status_cd VARCHAR(1),
            modified_by VARCHAR(20),
            is_adjustment VARCHAR(1),
            adjustment_code VARCHAR(1),
            adjustment_end_date DATE
        )
        ON CONFLICT (fy_cd, period_no, sub_period_no)
        DO UPDATE SET
            sub_period_end_date = EXCLUDED.sub_period_end_date,
            status_cd = EXCLUDED.status_cd,
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
        public async Task<bool> IsSubPeriodOpen(string fyCd, int periodNo, int subPeriodNo)
        {
            return await _context.SubPeriods.AnyAsync(x =>
                x.FyCd == fyCd &&
                x.PeriodNo == periodNo &&
                x.SubPeriodNo == subPeriodNo &&
                x.StatusCd == "O");
        }
    }
}
