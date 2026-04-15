using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorTermsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorTermsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(VendorTermWithSchedulesDto dto)
        {
            if (dto?.Term == null || string.IsNullOrWhiteSpace(dto.Term.TermsDc))
                return BadRequest("Invalid payload");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;

                var term = await _context.VendorTerms
                    .Include(x => x.Schedules)
                    .FirstOrDefaultAsync(x => x.TermsDc == dto.Term.TermsDc);

                if (term == null)
                {
                    // ➕ CREATE
                    term = new VendorTerm
                    {
                        TermsDc = dto.Term.TermsDc,
                        DiscPctRt = dto.Term.DiscPctRt,
                        DiscDaysNo = dto.Term.DiscDaysNo,
                        STermsBasisCd = dto.Term.STermsBasisCd,
                        SDueDateCd = dto.Term.SDueDateCd,
                        NoDaysNo = dto.Term.NoDaysNo,
                        DayOfMthDueNo = dto.Term.DayOfMthDueNo,
                        ModifiedBy = dto.Term.ModifiedBy,
                        TimeStamp = now
                    };

                    await _context.VendorTerms.AddAsync(term);
                }
                else
                {
                    // 🔄 UPDATE
                    term.DiscPctRt = dto.Term.DiscPctRt;
                    term.DiscDaysNo = dto.Term.DiscDaysNo;
                    term.STermsBasisCd = dto.Term.STermsBasisCd;
                    term.SDueDateCd = dto.Term.SDueDateCd;
                    term.NoDaysNo = dto.Term.NoDaysNo;
                    term.DayOfMthDueNo = dto.Term.DayOfMthDueNo;
                    term.ModifiedBy = dto.Term.ModifiedBy;
                    term.TimeStamp = now;
                }

                // =========================
                // 🔥 HANDLE SCHEDULES
                // =========================
                var incoming = dto.Schedules ?? new List<VendorTermScheduleDto>();

                // Remove duplicates
                incoming = incoming
                    .GroupBy(x => x.VendTermsSchKey)
                    .Select(g => g.First())
                    .ToList();

                var existingSchedules = term.Schedules ?? new List<VendorTermSchedule>();

                var existingDict = existingSchedules.ToDictionary(x => x.VendTermsSchKey);

                // UPSERT
                foreach (var sch in incoming)
                {
                    if (existingDict.TryGetValue(sch.VendTermsSchKey, out var db))
                    {
                        // 🔄 UPDATE
                        db.DueDayNo = sch.DueDayNo;
                        db.FromNo = sch.FromNo;
                        db.ToNo = sch.ToNo;
                        db.SCurNextMthCd = sch.SCurNextMthCd;
                        db.ModifiedBy = sch.ModifiedBy;
                        db.TimeStamp = now;
                    }
                    else
                    {
                        // ➕ INSERT
                        term.Schedules ??= new List<VendorTermSchedule>();

                        term.Schedules.Add(new VendorTermSchedule
                        {
                            TermsDc = term.TermsDc,
                            VendTermsSchKey = sch.VendTermsSchKey,
                            DueDayNo = sch.DueDayNo,
                            FromNo = sch.FromNo,
                            ToNo = sch.ToNo,
                            SCurNextMthCd = sch.SCurNextMthCd,
                            ModifiedBy = sch.ModifiedBy,
                            TimeStamp = now
                        });
                    }
                }

                // ❌ DELETE (sync behavior)
                var keySet = incoming.Select(x => x.VendTermsSchKey).ToHashSet();

                var toDelete = existingSchedules
                    .Where(x => !keySet.Contains(x.VendTermsSchKey))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorTermSchedules.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "Saved successfully",
                    schedulesInsertedOrUpdated = incoming.Count,
                    schedulesDeleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorTerms
                .Include(x => x.Schedules)
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{termsDc}")]
        public async Task<IActionResult> Get(string termsDc)
        {
            var data = await _context.VendorTerms
                .Include(x => x.Schedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TermsDc == termsDc);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpDelete("{termsDc}")]
        public async Task<IActionResult> Delete(string termsDc)
        {
            var entity = await _context.VendorTerms.FindAsync(termsDc);

            if (entity == null)
                return NotFound();

            _context.VendorTerms.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
