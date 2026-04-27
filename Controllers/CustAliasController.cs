using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustAliasController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustAliasController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Set<CustAlias>()
                .Include(x => x.Cust)
                .ToListAsync();

            return Ok(data);
        }

        // GET BY KEY
        [HttpGet("{custId}/{aliasKey}/{companyId}")]
        public async Task<IActionResult> Get(string custId, int aliasKey, string companyId)
        {
            var entity = await _context.Set<CustAlias>()
                .FindAsync(custId, aliasKey, companyId);

            return entity == null ? NotFound() : Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustAliasDto dto, string ModifiedBy)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var custExists = await _context.Custs
                .AnyAsync(x => x.CustId == dto.CustId && x.CompanyId == dto.CompanyId);

            if (!custExists)
                return BadRequest("Invalid Customer");

            var entity = new CustAlias
            {
                CustId = dto.CustId,
                CustAliasKey = dto.CustAliasKey,
                CompanyId = dto.CompanyId,
                CustAliasName = dto.CustAliasName,
                ModifiedBy = ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // UPDATE
        [HttpPut("{custId}/{aliasKey}/{companyId}")]
        public async Task<IActionResult> Update(string custId, int aliasKey, string companyId, CustAliasDto dto, string ModifiedBy)
        {
            var entity = await _context.Set<CustAlias>()
                .FindAsync(custId, aliasKey, companyId);

            if (entity == null)
                return NotFound();

            entity.CustAliasName = dto.CustAliasName;
            entity.TimeStamp = DateTime.UtcNow;
            entity.ModifiedBy = ModifiedBy;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{aliasKey}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, int aliasKey, string companyId)
        {
            var entity = await _context.Set<CustAlias>()
                .FindAsync(custId, aliasKey, companyId);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        [HttpGet("dropdown/{custId}/{companyId}")]
        public async Task<IActionResult> GetDropdown(string custId, string companyId)
        {
            var data = await _context.Set<CustAlias>()
                .Where(x => x.CustId == custId && x.CompanyId == companyId)
                .Select(x => new DropdownDto
                {
                    Value = x.CustAliasKey.ToString(),
                    Label = x.CustAliasName
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            return Ok(data);
        }
    }
}
