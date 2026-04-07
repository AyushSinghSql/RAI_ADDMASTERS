using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiscalYearController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public FiscalYearController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.FiscalYears
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY ID
        [HttpGet("{fyCd}")]
        public async Task<IActionResult> Get(string fyCd, string CompanyId)
        {
            var data = await _context.FiscalYears.FindAsync(fyCd, CompanyId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(FiscalYearDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            if (string.IsNullOrEmpty(dto.FyCd) || string.IsNullOrEmpty(dto.CompanyId))
                return BadRequest("FyCd and CompanyId are required");

            var exists = await _context.FiscalYears
                .AnyAsync(x => x.FyCd == dto.FyCd && x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Fiscal Year already exists");

            var entity = new FiscalYear
            {
                FyCd = dto.FyCd,
                StatusCd = dto.StatusCd,
                FyDesc = dto.FyDesc,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow,
                CloseActTgtCd = dto.CloseActTgtCd,
                CompanyId = dto.CompanyId
            };

            _context.FiscalYears.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ UPDATE
        [HttpPut("{fyCd}")]
        public async Task<IActionResult> Update(string fyCd, FiscalYearDto dto)
        {
            var entity = await _context.FiscalYears.FindAsync(fyCd,dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.StatusCd = dto.StatusCd;
            entity.FyDesc = dto.FyDesc;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.CloseActTgtCd = dto.CloseActTgtCd;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{fyCd}")]
        public async Task<IActionResult> Delete(string fyCd, string CompanyId)
        {
            var entity = await _context.FiscalYears.FindAsync(fyCd, CompanyId);

            if (entity == null)
                return NotFound();

            _context.FiscalYears.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpPost("year-end-close")]
        public async Task<IActionResult> YearEndClose(YearEndCloseDto dto)
        {
            // 1️⃣ Validate all periods closed
            var openPeriods = await _context.AccountingPeriods
                .AnyAsync(x => x.FyCd == dto.FyCd && x.StatusCd != "C" && x.CompanyId == dto.CompanyId);

            if (openPeriods)
                return BadRequest("Not all periods are closed");

            // 2️⃣ Validate all journals closed
            var openJournals = await _context.JournalStatuses
                .AnyAsync(x => x.FyCd == dto.FyCd &&
                               x.CompanyId == dto.CompanyId &&
                               x.IsOpen == "Y");

            if (openJournals)
                return BadRequest("Some journals are still open");

            // 3️⃣ Close Fiscal Year
            var fy = await _context.FiscalYears.FindAsync(dto.FyCd, dto.CompanyId);
            if (fy == null) return NotFound("Fiscal year not found");

            fy.StatusCd = "C"; // ✅ use existing column
            fy.ModifiedBy = dto.ModifiedBy;
            fy.TimeStamp = DateTime.UtcNow;

            // 4️⃣ Carry forward balances
            var sql = @"
        INSERT INTO public.gl_balance (account_id, fy_cd, opening_balance, company_id)
        SELECT account_id, @nextFy, closing_balance, company_id
        FROM public.gl_balance
        WHERE fy_cd = @currentFy AND company_id = @companyId
    ";

            await _context.Database.ExecuteSqlRawAsync(
                sql,
                new Npgsql.NpgsqlParameter("@nextFy", dto.NextFyCd),
                new Npgsql.NpgsqlParameter("@currentFy", dto.FyCd),
                new Npgsql.NpgsqlParameter("@companyId", dto.CompanyId)
            );

            // 5️⃣ Open next FY first period
            var nextPeriod = await _context.AccountingPeriods
                .FirstOrDefaultAsync(x => x.FyCd == dto.NextFyCd && x.PeriodNo == 1);

            if (nextPeriod != null)
            {
                nextPeriod.StatusCd = "O";
                nextPeriod.ModifiedBy = dto.ModifiedBy;
                nextPeriod.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok("Year-end closing completed successfully");
        }

        [HttpGet("year-end-validation/{fyCd}/{companyId}")]
        public async Task<IActionResult> ValidateYearEnd(string fyCd, string companyId)
        {
            var openPeriods = await _context.AccountingPeriods
                .Where(x => x.FyCd == fyCd && x.StatusCd != "C")
                .ToListAsync();

            var openJournals = await _context.JournalStatuses
                .Where(x => x.FyCd == fyCd && x.CompanyId == companyId && x.IsOpen == "Y")
                .ToListAsync();

            return Ok(new
            {
                IsValid = !openPeriods.Any() && !openJournals.Any(),
                OpenPeriods = openPeriods,
                OpenJournals = openJournals
            });
        }

        [HttpPost("create-next-fy-periods")]
        public async Task<IActionResult> CreateNextFY(string currentFy, string nextFy, string user)
        {
            var periods = await _context.AccountingPeriods
                .Where(x => x.FyCd == currentFy)
                .ToListAsync();

            var newPeriods = periods.Select(p => new AccountingPeriod
            {
                FyCd = nextFy,
                PeriodNo = p.PeriodNo,
                StatusCd = "C", // initially closed
                PeriodEndDate = p.PeriodEndDate.AddYears(1),
                ModifiedBy = user,
                TimeStamp = DateTime.UtcNow,
                IsAdjustment = p.IsAdjustment,
                AdjustmentCode = p.AdjustmentCode,
                CompanyId = p.CompanyId
            });

            await _context.AccountingPeriods.AddRangeAsync(newPeriods);
            await _context.SaveChangesAsync();

            return Ok("Next FY created");
        }

        [HttpPost("close-fy")]
        public async Task<IActionResult> CloseFY(string fyCd, string user, string CompanyId)
        {
            var fy = await _context.FiscalYears.FindAsync(fyCd, CompanyId);
            if (fy == null) return NotFound();

            fy.StatusCd = "C";

            // 🔥 Cascade close periods
            var periods = await _context.AccountingPeriods
                .Where(x => x.FyCd == fyCd && x.CompanyId == CompanyId)
                .ToListAsync();

            foreach (var p in periods)
                p.StatusCd = "C";

            // 🔥 Cascade close sub-periods
            var subPeriods = await _context.SubPeriods
                .Where(x => x.FyCd == fyCd && x.CompanyId == CompanyId)
                .ToListAsync();

            foreach (var sp in subPeriods)
                sp.StatusCd = "C";

            await _context.SaveChangesAsync();

            return Ok("Fiscal year locked completely");
        }

        [HttpPost("control")]
        public async Task<IActionResult> ControlJournal(JournalControlDto dto)
        {
            var now = DateTime.UtcNow;

            // 🔥 1. SUB PERIOD + JOURNAL (Most Specific)
            if (dto.SubPeriodNo.HasValue && dto.JournalCode != null)
            {
                var records = await _context.SubPeriodJournalStatuses
                    .Where(x =>
                        x.FyCd == dto.FyCd &&
                        x.PeriodNo == dto.PeriodNo &&
                        x.SubPeriodNo == dto.SubPeriodNo &&
                        x.CompanyId == dto.CompanyId &&
                        x.JournalCode == dto.JournalCode)
                    .ToListAsync();

                foreach (var r in records)
                {
                    r.IsOpen = dto.Status;
                    r.ModifiedBy = dto.ModifiedBy;
                    r.TimeStamp = now;
                }

                await _context.SaveChangesAsync();

                return Ok("Updated: SubPeriod + Journal");
            }

            // 🔥 2. PERIOD + JOURNAL
            if (dto.PeriodNo.HasValue && dto.JournalCode != null)
            {
                var records = await _context.JournalStatuses
                    .Where(x =>
                        x.FyCd == dto.FyCd &&
                        x.PeriodNo == dto.PeriodNo &&
                        x.CompanyId == dto.CompanyId &&
                        x.JournalCode == dto.JournalCode)
                    .ToListAsync();

                foreach (var r in records)
                {
                    r.IsOpen = dto.Status;
                    r.ModifiedBy = dto.ModifiedBy;
                    r.TimeStamp = now;
                }

                await _context.SaveChangesAsync();

                return Ok("Updated: Period + Journal");
            }

            // 🔥 3. SUB PERIOD (ALL JOURNALS)
            if (dto.SubPeriodNo.HasValue)
            {
                var records = await _context.SubPeriodJournalStatuses
                    .Where(x =>
                        x.FyCd == dto.FyCd &&
                        x.PeriodNo == dto.PeriodNo &&
                        x.SubPeriodNo == dto.SubPeriodNo &&
                        x.CompanyId == dto.CompanyId)
                    .ToListAsync();

                foreach (var r in records)
                {
                    r.IsOpen = dto.Status;
                    r.ModifiedBy = dto.ModifiedBy;
                    r.TimeStamp = now;
                }

                await _context.SaveChangesAsync();

                return Ok("Updated: SubPeriod (All Journals)");
            }

            // 🔥 4. PERIOD (ALL JOURNALS)
            if (dto.PeriodNo.HasValue)
            {
                var records = await _context.JournalStatuses
                    .Where(x =>
                        x.FyCd == dto.FyCd &&
                        x.PeriodNo == dto.PeriodNo &&
                        x.CompanyId == dto.CompanyId)
                    .ToListAsync();

                foreach (var r in records)
                {
                    r.IsOpen = dto.Status;
                    r.ModifiedBy = dto.ModifiedBy;
                    r.TimeStamp = now;
                }

                await _context.SaveChangesAsync();

                return Ok("Updated: Period (All Journals)");
            }

            // 🔥 5. FISCAL YEAR (ALL)
            var fyPeriods = await _context.AccountingPeriods
                .Where(x =>
                    x.FyCd == dto.FyCd &&
                    x.CompanyId == dto.CompanyId)
                .ToListAsync();

            foreach (var p in fyPeriods)
            {
                p.StatusCd = dto.Status;
                p.ModifiedBy = dto.ModifiedBy;
                p.TimeStamp = now;
            }

            await _context.SaveChangesAsync();

            return Ok("Updated: Fiscal Year");
        }
    }
}
