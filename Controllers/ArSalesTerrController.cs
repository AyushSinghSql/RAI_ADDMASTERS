using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArSalesTerrController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ArSalesTerrController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.ArSalesTerrs.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(ArSalesTerr model)
        {
            _context.ArSalesTerrs.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.Custs.AnyAsync(x => x.SalesTerrKey == id))
                return BadRequest("Used in Customer");

            var entity = await _context.ArSalesTerrs.FindAsync(id);
            if (entity == null) return NotFound();

            _context.ArSalesTerrs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
