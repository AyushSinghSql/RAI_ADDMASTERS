using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorCheckHistoryController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCheckHistoryController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorCheckHistoryDto dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorCheckHistories.AnyAsync(x =>
                x.CheckNumber == dto.CheckNumber &&
                x.PayVendorId == dto.PayVendorId);

            if (exists)
                return Conflict("Already exists");

            var entity = new VendorCheckHistory
            {
                CheckNumber = dto.CheckNumber,
                PayVendorId = dto.PayVendorId,
                CheckAmount = dto.CheckAmount,
                CheckDate = dto.CheckDate,
                StatusCode = dto.StatusCode,
                PaymentUserId = dto.PaymentUserId,
                ModifiedBy = dto.ModifiedBy,
                ModifiedTs = DateTime.UtcNow
            };

            _context.VendorCheckHistories.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorCheckHistories
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{checkNumber}/{vendorId}")]
        public async Task<IActionResult> Get(decimal checkNumber, string vendorId)
        {
            var entity = await _context.VendorCheckHistories
                .FindAsync(checkNumber, vendorId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut("{checkNumber}/{vendorId}")]
        public async Task<IActionResult> Update(decimal checkNumber, string vendorId, VendorCheckHistoryDto dto)
        {
            var entity = await _context.VendorCheckHistories
                .FindAsync(checkNumber, vendorId);

            if (entity == null)
                return NotFound();

            entity.CheckAmount = dto.CheckAmount;
            entity.CheckDate = dto.CheckDate;
            entity.StatusCode = dto.StatusCode;
            entity.PaymentUserId = dto.PaymentUserId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{checkNumber}/{vendorId}")]
        public async Task<IActionResult> Delete(decimal checkNumber, string vendorId)
        {
            var entity = await _context.VendorCheckHistories
                .FindAsync(checkNumber, vendorId);

            if (entity == null)
                return NotFound();

            _context.VendorCheckHistories.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorCheckHistoryQuery query)
        {
            var q = _context.VendorCheckHistories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.PayVendorId))
                q = q.Where(x => x.PayVendorId == query.PayVendorId);

            if (!string.IsNullOrWhiteSpace(query.StatusCode))
                q = q.Where(x => x.StatusCode == query.StatusCode);

            var total = await q.CountAsync();

            var data = await q
                .OrderByDescending(x => x.CheckDate)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, query.Page, query.PageSize, data });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorCheckHistoryDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            var existing = await _context.VendorCheckHistories.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.CheckNumber}|{x.PayVendorId}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.CheckNumber}|{dto.PayVendorId}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.CheckAmount = dto.CheckAmount;
                    db.StatusCode = dto.StatusCode;
                    db.ModifiedTs = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorCheckHistories.Add(new VendorCheckHistory
                    {
                        CheckNumber = dto.CheckNumber,
                        PayVendorId = dto.PayVendorId,
                        CheckAmount = dto.CheckAmount,
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
