using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorVatInfoController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorVatInfoController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorVatInfoDto dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorVatInfos.AnyAsync(x =>
                x.VendId == dto.VendId && x.CompanyId == dto.CompanyId);

            if (exists)
                return Conflict("VAT info already exists for this vendor + company");

            var entity = new VendorVatInfo
            {
                VendId = dto.VendId,
                CompanyId = dto.CompanyId,
                 DefaultTaxIdFl = dto.DefaultTaxIdFl,
                TaxId = dto.TaxId,
                TaxLocationCd = dto.TaxLocationCd,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.VendorVatInfos.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorVatInfos
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{vendId}/{companyId}")]
        public async Task<IActionResult> Get(string vendId, string companyId)
        {
            var entity = await _context.VendorVatInfos
                .FindAsync(vendId, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut("{vendId}/{companyId}")]
        public async Task<IActionResult> Update(string vendId, string companyId, VendorVatInfoDto dto)
        {
            var entity = await _context.VendorVatInfos
                .FindAsync(vendId, companyId);

            if (entity == null)
                return NotFound();

            entity.TaxId = dto.TaxId;
            entity.TaxLocationCd = dto.TaxLocationCd;
            entity.DefaultTaxIdFl = dto.DefaultTaxIdFl;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpDelete("{vendId}/{companyId}")]
        public async Task<IActionResult> Delete(string vendId, string companyId)
        {
            var entity = await _context.VendorVatInfos
                .FindAsync(vendId, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorVatInfos.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorVatQuery query)
        {
            var q = _context.VendorVatInfos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.VendId))
                q = q.Where(x => x.VendId == query.VendId);

            if (!string.IsNullOrWhiteSpace(query.CompanyId))
                q = q.Where(x => x.CompanyId == query.CompanyId);

            if (!string.IsNullOrWhiteSpace(query.TaxId))
                q = q.Where(x => x.TaxId == query.TaxId);

            var total = await q.CountAsync();

            var data = await q
                .OrderBy(x => x.VendId)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, query.Page, query.PageSize, data });
        }
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorVatInfoDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            input = input
                .GroupBy(x => new { x.VendId, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            var existing = await _context.VendorVatInfos.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.VendId}|{x.CompanyId}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.VendId}|{dto.CompanyId}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.TaxLocationCd = dto.TaxLocationCd;
                    db.TaxId = dto.TaxId;
                    db.DefaultTaxIdFl = dto.DefaultTaxIdFl;
                    db.TimeStamp = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorVatInfos.Add(new VendorVatInfo
                    {
                        VendId = dto.VendId,
                        CompanyId = dto.CompanyId,
                        TaxId = dto.TaxId,
                        TaxLocationCd = dto.TaxLocationCd,
                        DefaultTaxIdFl = dto.DefaultTaxIdFl,
                        TimeStamp = DateTime.UtcNow
                    });
                    insert++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { insert, update });
        }
    }
}
