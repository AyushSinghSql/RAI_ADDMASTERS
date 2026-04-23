using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecurringVoucherPeriodsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public RecurringVoucherPeriodsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RecurringVoucherPeriod dto)
        {

            // Required fields
            if (string.IsNullOrWhiteSpace(dto.VoucherGroupCode) ||
                string.IsNullOrWhiteSpace(dto.FiscalYearCode) ||
                string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest("Required fields missing");

            // Period validation
            if (dto.PeriodNo <= 0 || dto.SubPeriodNo <= 0)
                return BadRequest("Invalid period values");

            // FK validation
            var groupExists = await _context.RecurringVoucherGroups.AnyAsync(x =>
                x.VoucherGroupCode == dto.VoucherGroupCode &&
                x.CompanyId == dto.CompanyId);

            if (!groupExists)
                return BadRequest("Invalid Voucher Group");

            // Duplicate prevention
            var exists = await _context.RecurringVoucherPeriods.AnyAsync(x =>
                x.VoucherGroupCode == dto.VoucherGroupCode &&
                x.FiscalYearCode == dto.FiscalYearCode &&
                x.PeriodNo == dto.PeriodNo &&
                x.SubPeriodNo == dto.SubPeriodNo &&
                x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Duplicate period record");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
        [HttpGet]
        public async Task<IActionResult> Get(string voucherGroupCode, string companyId)
        {
            var data = await _context.RecurringVoucherPeriods
                .Where(x => x.VoucherGroupCode == voucherGroupCode &&
                            x.CompanyId == companyId)
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("{group}/{fy}/{pd}/{sub}/{companyId}")]
        public async Task<IActionResult> GetById(string group, string fy, int pd, int sub, string companyId)
        {
            var data = await _context.RecurringVoucherPeriods
                .FindAsync(group, fy, pd, sub, companyId);

            if (data == null) return NotFound();

            return Ok(data);
        }
        [HttpPut]
        public async Task<IActionResult> Update(RecurringVoucherPeriod dto)
        {
            var entity = await _context.RecurringVoucherPeriods
                .FindAsync(dto.VoucherGroupCode, dto.FiscalYearCode,
                           dto.PeriodNo, dto.SubPeriodNo, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.ModifiedBy = dto.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }
        [HttpDelete("{group}/{fy}/{pd}/{sub}/{companyId}")]
        public async Task<IActionResult> Delete(string group, string fy, int pd, int sub, string companyId)
        {
            var entity = await _context.RecurringVoucherPeriods
                .FindAsync(group, fy, pd, sub, companyId);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string group, string companyId)
        {
            var data = await _context.RecurringVoucherPeriods
                .Where(x => x.VoucherGroupCode == group &&
                            x.CompanyId == companyId)
                .Select(x => new {
                    value = $"{x.FiscalYearCode}-{x.PeriodNo}-{x.SubPeriodNo}",
                    label = $"{x.FiscalYearCode} / P{x.PeriodNo}-{x.SubPeriodNo}"
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
