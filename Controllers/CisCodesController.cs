using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CisCodesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CisCodesController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(CisCode dto)
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(dto.CisCodeId) ||
                string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest("CIS Code and Company required");

            // Rate validation
            if (dto.WithholdingRate < 0 || dto.WithholdingRate > 1)
                return BadRequest("Withholding rate must be between 0 and 1");

            // Duplicate prevention
            var exists = await _context.CisCodes.AnyAsync(x =>
                x.CisCodeId == dto.CisCodeId &&
                x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Duplicate CIS Code");

            // Optional FK validations (recommended)
            if (!string.IsNullOrEmpty(dto.AccountId))
            {
                var acctExists = await _context.Accounts
                    .AnyAsync(x => x.AcctId == dto.AccountId);

                if (!acctExists)
                    return BadRequest("Invalid Account");
            }

            _context.CisCodes.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string companyId)
        {
            var data = await _context.CisCodes
                .Where(x => x.CompanyId == companyId)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{cisCode}/{companyId}")]
        public async Task<IActionResult> Get(string cisCode, string companyId)
        {
            var data = await _context.CisCodes
                .FindAsync(cisCode, companyId);

            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Update(CisCode dto)
        {
            var entity = await _context.CisCodes
                .FindAsync(dto.CisCodeId, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.Description = dto.Description;
            entity.WithholdingRate = dto.WithholdingRate;
            entity.AccountId = dto.AccountId;
            entity.OrganizationId = dto.OrganizationId;
            entity.Reference1Id = dto.Reference1Id;
            entity.Reference2Id = dto.Reference2Id;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{cisCode}/{companyId}")]
        public async Task<IActionResult> Delete(string cisCode, string companyId)
        {
            var entity = await _context.CisCodes
                .FindAsync(cisCode, companyId);

            if (entity == null)
                return NotFound();

            //// Optional: Prevent delete if used
            //var isUsed = await _context.Vendors
            //    .AnyAsync(x => x.CisCode == cisCode);

            //if (isUsed)
            //    return BadRequest("CIS Code is used in transactions");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string companyId)
        {
            var data = await _context.CisCodes
                .Where(x => x.CompanyId == companyId)
                .Select(x => new {
                    value = x.CisCodeId,
                    label = x.Description
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
