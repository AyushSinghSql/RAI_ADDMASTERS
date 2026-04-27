using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustAddrCntactController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustAddrCntactController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Set<CustAddrCntact>()
                .Include(x => x.CustAddr)
                .ToListAsync();

            return Ok(data);
        }

        // GET BY KEY
        [HttpGet("{custId}/{addrDc}/{cntactId}/{companyId}")]
        public async Task<IActionResult> Get(string custId, string addrDc, string cntactId, string companyId)
        {
            var entity = await _context.Set<CustAddrCntact>()
                .FindAsync(custId, addrDc, cntactId, companyId);

            return entity == null ? NotFound() : Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustAddrCntactDto dto, string ModifiedBy)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // FK validation
            var addrExists = await _context.Set<CustAddr>()
                .AnyAsync(x =>
                    x.CustId == dto.CustId &&
                    x.AddrDc == dto.AddrDc &&
                    x.CompanyId == dto.CompanyId);

            if (!addrExists)
                return BadRequest("Invalid Customer Address");

            var entity = new CustAddrCntact
            {
                CustId = dto.CustId,
                AddrDc = dto.AddrDc,
                CntactId = dto.CntactId,
                CompanyId = dto.CompanyId,
                CntactFirstName = dto.CntactFirstName,
                CntactLastName = dto.CntactLastName,
                PhoneId = dto.PhoneId,
                CntactTitleName = dto.CntactTitleName,
                Notes = dto.Notes,
                EmailId = dto.EmailId,
                TimeStamp = DateTime.UtcNow,
                ModifiedBy = ModifiedBy
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // UPDATE
        [HttpPut("{custId}/{addrDc}/{cntactId}/{companyId}")]
        public async Task<IActionResult> Update(
            string custId, string addrDc, string cntactId, string companyId,
            CustAddrCntactDto dto, string ModifiedBy)
        {
            var entity = await _context.Set<CustAddrCntact>()
                .FindAsync(custId, addrDc, cntactId, companyId);

            if (entity == null)
                return NotFound();

            entity.CntactFirstName = dto.CntactFirstName;
            entity.CntactLastName = dto.CntactLastName;
            entity.PhoneId = dto.PhoneId;
            entity.CntactTitleName = dto.CntactTitleName;
            entity.Notes = dto.Notes;
            entity.EmailId = dto.EmailId;
            entity.TimeStamp = DateTime.UtcNow;
            entity.ModifiedBy = ModifiedBy;
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{addrDc}/{cntactId}/{companyId}")]
        public async Task<IActionResult> Delete(
            string custId, string addrDc, string cntactId, string companyId)
        {
            var entity = await _context.Set<CustAddrCntact>()
                .FindAsync(custId, addrDc, cntactId, companyId);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("dropdown/{custId}/{addrDc}/{companyId}")]
        public async Task<IActionResult> GetDropdown(string custId, string addrDc, string companyId)
        {
            var data = await _context.Set<CustAddrCntact>()
                .Where(x =>
                    x.CustId == custId &&
                    x.AddrDc == addrDc &&
                    x.CompanyId == companyId)
                .Select(x => new DropdownDto
                {
                    Value = x.CntactId,
                    Label = x.CntactFirstName + " " + x.CntactLastName
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            return Ok(data);
        }
    }
}
