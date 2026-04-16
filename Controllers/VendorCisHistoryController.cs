using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorCisHistoryController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCisHistoryController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorCisHistoryDto dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorCisHistories.AnyAsync(x =>
                x.CisVoucherNo == dto.CisVoucherNo &&
                x.CisVoucherType == dto.CisVoucherType);

            if (exists)
                return Conflict("Already exists");

            var entity = new VendorCisHistory
            {
                CisVoucherNo = dto.CisVoucherNo,
                CisVoucherType = dto.CisVoucherType,
                PayVendorId = dto.PayVendorId,
                TaxableEntityId = dto.TaxableEntityId,
                CisPaymentAmount = dto.CisPaymentAmount,
                CisWithheldAmount = dto.CisWithheldAmount,
                TaxPeriodStartDate = dto.TaxPeriodStartDate,
                TaxPeriodEndDate = dto.TaxPeriodEndDate,
                SpoiledFlag = dto.SpoiledFlag,
                ModifiedBy = dto.ModifiedBy,
                ModifiedTs = DateTime.UtcNow
            };

            _context.VendorCisHistories.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorCisHistories
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{voucherNo}/{voucherType}")]
        public async Task<IActionResult> Get(string voucherNo, string voucherType)
        {
            var entity = await _context.VendorCisHistories
                .FindAsync(voucherNo, voucherType);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut("{voucherNo}/{voucherType}")]
        public async Task<IActionResult> Update(string voucherNo, string voucherType, VendorCisHistoryDto dto)
        {
            var entity = await _context.VendorCisHistories
                .FindAsync(voucherNo, voucherType);

            if (entity == null)
                return NotFound();

            entity.CisPaymentAmount = dto.CisPaymentAmount;
            entity.CisWithheldAmount = dto.CisWithheldAmount;
            entity.TaxPeriodStartDate = dto.TaxPeriodStartDate;
            entity.TaxPeriodEndDate = dto.TaxPeriodEndDate;
            entity.SpoiledFlag = dto.SpoiledFlag;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{voucherNo}/{voucherType}")]
        public async Task<IActionResult> Delete(string voucherNo, string voucherType)
        {
            var entity = await _context.VendorCisHistories
                .FindAsync(voucherNo, voucherType);

            if (entity == null)
                return NotFound();

            _context.VendorCisHistories.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorCisHistoryQuery query)
        {
            var q = _context.VendorCisHistories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.VendorId))
                q = q.Where(x => x.PayVendorId == query.VendorId);

            if (!string.IsNullOrWhiteSpace(query.CompanyId))
                q = q.Where(x => x.CompanyId == query.CompanyId);

            if (!string.IsNullOrWhiteSpace(query.VoucherType))
                q = q.Where(x => x.CisVoucherType == query.VoucherType);

            var total = await q.CountAsync();

            var data = await q
                .OrderByDescending(x => x.TaxPeriodStartDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, query.Page, query.PageSize, data });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCisHistoryDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            input = input
                .GroupBy(x => new { x.CisVoucherNo, x.CisVoucherType })
                .Select(g => g.First())
                .ToList();

            var existing = await _context.VendorCisHistories.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.CisVoucherNo}|{x.CisVoucherType}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.CisVoucherNo}|{dto.CisVoucherType}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.CisPaymentAmount = dto.CisPaymentAmount;
                    db.CisWithheldAmount = dto.CisWithheldAmount;
                    db.SpoiledFlag = dto.SpoiledFlag;
                    db.ModifiedTs = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorCisHistories.Add(new VendorCisHistory
                    {
                        CisVoucherNo = dto.CisVoucherNo,
                        CisVoucherType = dto.CisVoucherType,
                        PayVendorId = dto.PayVendorId,
                        CisPaymentAmount = dto.CisPaymentAmount,
                        ModifiedTs = DateTime.UtcNow
                    });
                    insert++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { insert, update });
        }
    }
}
