using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/dflt-cash-accts")]
    public class DfltCashAcctsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public DfltCashAcctsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET with pagination + filtering
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? companyId,
            string? search,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.DfltCashAccts.AsQueryable();

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.AcctId.Contains(search) ||
                    x.CashAcctsDesc.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.SeqNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        // ✅ GET by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(decimal id)
        {
            var item = await _context.DfltCashAccts.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(DfltCashAcct model)
        {
            model.TimeStamp = DateTime.UtcNow;

            _context.DfltCashAccts.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(decimal id, DfltCashAcct model)
        {
            var existing = await _context.DfltCashAccts.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.AcctId = model.AcctId;
            existing.OrgId = model.OrgId;
            existing.Ref1Id = model.Ref1Id;
            existing.Ref2Id = model.Ref2Id;
            existing.SeqNo = model.SeqNo;
            existing.CompanyId = model.CompanyId;
            existing.CashAcctsDesc = model.CashAcctsDesc;
            existing.BankAcctAbbrv = model.BankAcctAbbrv;
            existing.ModifiedBy = model.ModifiedBy;
            existing.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(decimal id)
        {
            var entity = await _context.DfltCashAccts.Where(x => x.CashAcctsKey == id).FirstOrDefaultAsync();

            if (entity == null)
                return NotFound();

            _context.DfltCashAccts.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
