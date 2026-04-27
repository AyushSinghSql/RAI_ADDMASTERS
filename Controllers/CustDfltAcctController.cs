using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustDfltAcctController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustDfltAcctController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.CustDfltAccts
                .Select(x => new
                {
                    x.CustId,
                    x.CompanyId,
                    x.SCustTrnType,
                    x.AcctId,
                    TrnTypeDesc = x.SCustTrnTypeNavigation.Description
                }).ToListAsync();

            return Ok(data);
        }

        // GET BY KEY
        [HttpGet("{custId}/{trnType}/{companyId}")]
        public async Task<IActionResult> Get(string custId, string trnType, string companyId)
        {
            var entity = await _context.CustDfltAccts.FindAsync(custId, trnType, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustDfltAcct model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.CustDfltAccts
                .AnyAsync(x => x.CustId == model.CustId &&
                               x.SCustTrnType == model.SCustTrnType &&
                               x.CompanyId == model.CompanyId);

            if (exists)
                return Conflict("Duplicate record");

            model.TimeStamp = DateTime.UtcNow;

            _context.CustDfltAccts.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // UPDATE
        [HttpPut("{custId}/{trnType}/{companyId}")]
        public async Task<IActionResult> Update(string custId, string trnType, string companyId, CustDfltAcct model)
        {
            var entity = await _context.CustDfltAccts.FindAsync(custId, trnType, companyId);

            if (entity == null)
                return NotFound();

            entity.AcctId = model.AcctId;
            entity.OrgId = model.OrgId;
            entity.ProjId = model.ProjId;
            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{trnType}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, string trnType, string companyId)
        {
            var entity = await _context.CustDfltAccts.FindAsync(custId, trnType, companyId);

            if (entity == null)
                return NotFound();

            _context.CustDfltAccts.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpGet("dropdown/by-customer/{custId}/{companyId}")]
        public async Task<IActionResult> GetByCustomer(string custId, string companyId)
        {
            var data = await _context.CustDfltAccts
                .Where(x => x.CustId == custId && x.CompanyId == companyId)
                .Select(x => new
                {
                    x.SCustTrnType,
                    x.AcctId,
                    Description = x.SCustTrnTypeNavigation.Description
                }).ToListAsync();

            return Ok(data);
        }
    }
}
