using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustLimitCrncyController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustLimitCrncyController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.CustLimitCrncies.ToListAsync();
            return Ok(data);
        }

        // GET BY KEY
        [HttpGet("{custId}/{currency}/{type}/{companyId}")]
        public async Task<IActionResult> Get(
            string custId, string currency, string type, string companyId)
        {
            var entity = await _context.CustLimitCrncies.FindAsync(
                custId, currency, type, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustLimitCrncyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.CustLimitCrncies.AnyAsync(x =>
                x.CustId == dto.CustId &&
                x.SCrncyCd == dto.SCrncyCd &&
                x.CrncyTypeCd == dto.CrncyTypeCd &&
                x.CompanyId == dto.CompanyId);

            if (exists)
                return Conflict("Record already exists");

            var entity = new CustLimitCrncy
            {
                CustId = dto.CustId,
                SCrncyCd = dto.SCrncyCd,
                CrncyTypeCd = dto.CrncyTypeCd,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = dto.TimeStamp,
                CompanyId = dto.CompanyId
            };

            _context.CustLimitCrncies.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(CustLimitCrncyDto dto)
        {
            var entity = await _context.CustLimitCrncies.FindAsync(
                dto.CustId, dto.SCrncyCd, dto.CrncyTypeCd, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = dto.TimeStamp;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{currency}/{type}/{companyId}")]
        public async Task<IActionResult> Delete(
            string custId, string currency, string type, string companyId)
        {
            var entity = await _context.CustLimitCrncies.FindAsync(
                custId, currency, type, companyId);

            if (entity == null)
                return NotFound();

            _context.CustLimitCrncies.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpGet("dropdown/currency/{custId}/{companyId}")]
        public async Task<IActionResult> GetCurrencies(string custId, string companyId)
        {
            var data = await _context.CustLimitCrncies
                .Where(x => x.CustId == custId && x.CompanyId == companyId)
                .Select(x => new
                {
                    x.SCrncyCd,
                    x.CrncyTypeCd
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
