using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorCheckVoucherDetailController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCheckVoucherDetailController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorCheckVoucherDetailDto dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorCheckVoucherDetails.AnyAsync(x =>
                x.CheckNumber == dto.CheckNumber &&
                x.VoucherKey == dto.VoucherKey);

            if (exists)
                return Conflict("Already exists");

            var entity = new VendorCheckVoucherDetail
            {
                CheckNumber = dto.CheckNumber,
                VoucherKey = dto.VoucherKey,
                PaidAmount = dto.PaidAmount,
                DiscountTakenAmount = dto.DiscountTakenAmount,
                ExchangeRate = dto.ExchangeRate,
                VoucherVendorId = dto.VoucherVendorId,
                ModifiedBy = dto.ModifiedBy,
                ModifiedTs = DateTime.UtcNow
            };

            _context.VendorCheckVoucherDetails.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorCheckVoucherDetails
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{checkNumber}/{voucherKey}")]
        public async Task<IActionResult> Get(decimal checkNumber, decimal voucherKey)
        {
            var entity = await _context.VendorCheckVoucherDetails
                .FindAsync(checkNumber, voucherKey);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut("{checkNumber}/{voucherKey}")]
        public async Task<IActionResult> Update(decimal checkNumber, decimal voucherKey, VendorCheckVoucherDetailDto dto)
        {
            var entity = await _context.VendorCheckVoucherDetails
                .FindAsync(checkNumber, voucherKey);

            if (entity == null)
                return NotFound();

            entity.PaidAmount = dto.PaidAmount;
            entity.DiscountTakenAmount = dto.DiscountTakenAmount;
            entity.ExchangeRate = dto.ExchangeRate;
            entity.VoucherVendorId = dto.VoucherVendorId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{checkNumber}/{voucherKey}")]
        public async Task<IActionResult> Delete(decimal checkNumber, decimal voucherKey)
        {
            var entity = await _context.VendorCheckVoucherDetails
                .FindAsync(checkNumber, voucherKey);

            if (entity == null)
                return NotFound();

            _context.VendorCheckVoucherDetails.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorCheckVoucherQuery query)
        {
            var q = _context.VendorCheckVoucherDetails.AsQueryable();

            if (query.CheckNumber.HasValue)
                q = q.Where(x => x.CheckNumber == query.CheckNumber);

            if (!string.IsNullOrWhiteSpace(query.VendorId))
                q = q.Where(x => x.VoucherVendorId == query.VendorId);

            var total = await q.CountAsync();

            var data = await q
                .OrderByDescending(x => x.CheckNumber)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, query.Page, query.PageSize, data });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCheckVoucherDetailDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            var existing = await _context.VendorCheckVoucherDetails.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.CheckNumber}|{x.VoucherKey}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.CheckNumber}|{dto.VoucherKey}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.PaidAmount = dto.PaidAmount;
                    db.ExchangeRate = dto.ExchangeRate;
                    db.ModifiedTs = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorCheckVoucherDetails.Add(new VendorCheckVoucherDetail
                    {
                        CheckNumber = dto.CheckNumber,
                        VoucherKey = dto.VoucherKey,
                        PaidAmount = dto.PaidAmount,
                        ExchangeRate = dto.ExchangeRate,
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
