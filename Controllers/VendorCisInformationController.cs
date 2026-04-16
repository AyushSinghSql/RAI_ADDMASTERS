using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorCisInformationController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCisInformationController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorCisInformation dto)
        {
            if (dto == null)
                return BadRequest();

            var exists = await _context.VendorCisInformations.AnyAsync(x =>
                x.VendId == dto.VendId && x.CisCode == dto.CisCode);

            if (exists)
                return Conflict("Already exists");

            //var entity = new VendorCisInformation
            //{
            //    VendId = dto.VendId,
            //    CisCode = dto.CisCode,
            //    CisType = dto.CisType,
            //    CertificateRegistrationNo = dto.CertificateRegistrationNo,
            //    StartDate = dto.StartDate,
            //    ExpiryDate = dto.ExpiryDate,
            //    CompanyId = dto.CompanyId,
            //    ModifiedBy = dto.ModifiedBy,
            //    ModifiedTs = DateTime.UtcNow
            //};

            _context.VendorCisInformations.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendorCisInformations
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{vendId}")]
        public async Task<IActionResult> Get(string vendId)
        {
            var entity = await _context.VendorCisInformations
                .FirstOrDefaultAsync(x => x.VendId == vendId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }
        [HttpPut("{vendId}")]
        public async Task<IActionResult> Update(string vendId, VendorCisInformationDto dto)
        {
            var entity = await _context.VendorCisInformations
                .FirstOrDefaultAsync(x => x.VendId == vendId);

            if (entity == null)
                return NotFound();

            entity.CisType = dto.CisType;
            entity.CertificateRegistrationNo = dto.CertificateRegistrationNo;
            entity.StartDate = dto.StartDate;
            entity.ExpiryDate = dto.ExpiryDate;
            entity.CompanyId = dto.CompanyId;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpDelete("{vendId}")]
        public async Task<IActionResult> Delete(string vendId)
        {
            var entity = await _context.VendorCisInformations
                .FirstOrDefaultAsync(x => x.VendId == vendId);

            if (entity == null)
                return NotFound();

            _context.VendorCisInformations.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] VendorCisQuery query)
        {
            var q = _context.VendorCisInformations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.VendId))
                q = q.Where(x => x.VendId == query.VendId);

            if (!string.IsNullOrWhiteSpace(query.CisType))
                q = q.Where(x => x.CisType == query.CisType);

            if (!string.IsNullOrWhiteSpace(query.CompanyId))
                q = q.Where(x => x.CompanyId == query.CompanyId);

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
        public async Task<IActionResult> Sync(List<VendorCisInformationDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            input = input
                .GroupBy(x => new { x.VendId, x.CisCode })
                .Select(g => g.First())
                .ToList();

            var existing = await _context.VendorCisInformations.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.VendId}|{x.CisCode}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.VendId}|{dto.CisCode}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.CisType = dto.CisType;
                    db.CertificateRegistrationNo = dto.CertificateRegistrationNo;
                    db.StartDate = dto.StartDate;
                    db.ExpiryDate = dto.ExpiryDate;
                    db.ModifiedTs = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendorCisInformations.Add(new VendorCisInformation
                    {
                        VendId = dto.VendId,
                        CisCode = dto.CisCode,
                        CisType = dto.CisType,
                        CertificateRegistrationNo = dto.CertificateRegistrationNo,
                        StartDate = dto.StartDate,
                        ExpiryDate = dto.ExpiryDate,
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
