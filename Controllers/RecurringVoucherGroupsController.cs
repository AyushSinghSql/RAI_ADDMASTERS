using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecurringVoucherGroupsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public RecurringVoucherGroupsController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(RecurringVoucherGroup dto)
        {
            if (await _context.RecurringVoucherGroups.AnyAsync(x =>
                x.VoucherGroupCode == dto.VoucherGroupCode &&
                x.CompanyId == dto.CompanyId))
                return BadRequest("Duplicate record");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
        [HttpGet]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.RecurringVoucherGroups
                .Where(x => x.CompanyId == companyId)
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("{code}/{companyId}")]
        public async Task<IActionResult> GetById(string code, string companyId)
        {
            var data = await _context.RecurringVoucherGroups
                .FindAsync(code, companyId);

            if (data == null) return NotFound();

            return Ok(data);
        }
        [HttpPut]
        public async Task<IActionResult> Update(RecurringVoucherGroup dto)
        {
            var entity = await _context.RecurringVoucherGroups
                .FindAsync(dto.VoucherGroupCode, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.ModifiedBy = dto.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }
        [HttpDelete("{code}/{companyId}")]
        public async Task<IActionResult> Delete(string code, string companyId)
        {
            var entity = await _context.RecurringVoucherGroups
                .FindAsync(code, companyId);

            if (entity == null)
                return NotFound();

            //// Prevent delete if used (example)
            //var isUsed = await _context.Vouchers
            //    .AnyAsync(x => x.VoucherGroupCode == code);

            //if (isUsed)
            //    return BadRequest("Voucher group is used in transactions");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string companyId)
        {
            var data = await _context.RecurringVoucherGroups
                .Where(x => x.CompanyId == companyId)
                .Select(x => new {
                    value = x.VoucherGroupCode,
                    label = x.VoucherGroupCode
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
