using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssueByAddrController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public IssueByAddrController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.IssueByAddrs.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(IssueByAddr model)
        {
            _context.IssueByAddrs.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.IssueByAddrs.FindAsync(id);
            if (entity == null) return NotFound();

            _context.IssueByAddrs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
