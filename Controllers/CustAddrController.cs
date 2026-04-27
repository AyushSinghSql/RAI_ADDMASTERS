using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustAddrController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustAddrController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Set<CustAddr>()
                .Include(x => x.Cust)
                .ToListAsync();

            return Ok(data);
        }

        // GET BY KEY
        [HttpGet("{custId}/{addrDc}/{companyId}")]
        public async Task<IActionResult> Get(string custId, string addrDc, string companyId)
        {
            var entity = await _context.Set<CustAddr>()
                .FindAsync(custId, addrDc, companyId);

            return entity == null ? NotFound() : Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustAddrDto dto, string ModifiedBy)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // FK validation
            var custExists = await _context.Custs
                .AnyAsync(x => x.CustId == dto.CustId && x.CompanyId == dto.CompanyId);

            if (!custExists)
                return BadRequest("Invalid Customer");

            var entity = new CustAddr
            {
                CustId = dto.CustId,
                AddrDc = dto.AddrDc,
                CompanyId = dto.CompanyId,
                PhoneId = dto.PhoneId,
                Ln1Addr = dto.Ln1Addr,
                CityName = dto.CityName,
                PostalCd = dto.PostalCd,
                TimeStamp = DateTime.UtcNow,
                ModifiedBy = ModifiedBy
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // UPDATE
        [HttpPut("{custId}/{addrDc}/{companyId}")]
        public async Task<IActionResult> Update(string custId, string addrDc, string companyId, CustAddrDto dto, string ModifiedBy)
        {
            var entity = await _context.Set<CustAddr>()
                .FindAsync(custId, addrDc, companyId);

            if (entity == null)
                return NotFound();

            entity.PhoneId = dto.PhoneId;
            entity.Ln1Addr = dto.Ln1Addr;
            entity.CityName = dto.CityName;
            entity.PostalCd = dto.PostalCd;
            entity.TimeStamp = DateTime.UtcNow;
            entity.ModifiedBy = ModifiedBy;
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{addrDc}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, string addrDc, string companyId)
        {
            var entity = await _context.Set<CustAddr>()
                .FindAsync(custId, addrDc, companyId);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("dropdown/{custId}/{companyId}")]
        public async Task<IActionResult> GetDropdown(string custId, string companyId)
        {
            var data = await _context.Set<CustAddr>()
                .Where(x => x.CustId == custId && x.CompanyId == companyId)
                .Select(x => new DropdownDto
                {
                    Value = x.AddrDc,
                    Label = x.AddrDc + " - " + x.CityName
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
