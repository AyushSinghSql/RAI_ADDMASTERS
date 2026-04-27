using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesAbbrvCdController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SalesAbbrvCdController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.SalesAbbrvCds.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(SalesAbbrvCd model)
        {
            _context.SalesAbbrvCds.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _context.Custs.AnyAsync(x => x.SalesAbbrvCd == id))
                return BadRequest("Used in Customer");

            var entity = await _context.SalesAbbrvCds.FindAsync(id);
            if (entity == null) return NotFound();

            _context.SalesAbbrvCds.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
