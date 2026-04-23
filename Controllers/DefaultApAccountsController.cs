using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/default-ap-accounts")]
    public class DefaultApAccountsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public DefaultApAccountsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? companyId,
            string? acctId,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.DefaultApAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(acctId))
                query = query.Where(x => x.AcctId.Contains(acctId));

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

        // ✅ GET BY KEY
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(decimal key)
        {
            var data = await _context.DefaultApAccounts.FindAsync(key);
            if (data == null) return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(DefaultApAccount input)
        {
            input.TimeStamp = DateTime.UtcNow;

            await _context.DefaultApAccounts.AddAsync(input);
            await _context.SaveChangesAsync();

            return Ok(input);
        }

        // ✅ UPDATE
        [HttpPut("{key}")]
        public async Task<IActionResult> Update(decimal key, DefaultApAccount input)
        {
            var existing = await _context.DefaultApAccounts.FindAsync(key);
            if (existing == null) return NotFound();

            _context.Entry(existing).CurrentValues.SetValues(input);
            existing.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(decimal key)
        {
            var entity = await _context.DefaultApAccounts.FindAsync(key);
            if (entity == null) return NotFound();

            _context.DefaultApAccounts.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
