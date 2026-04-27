using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArCrRatingController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ArCrRatingController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.ArCrRatings.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(ArCrRating model)
        {
            if (string.IsNullOrWhiteSpace(model.CrRatingCd))
                return BadRequest("Rating Value required");

            _context.ArCrRatings.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.Custs.AnyAsync(x => x.ArCrRatingKey == id))
                return BadRequest("Used in Customer");

            var entity = await _context.ArCrRatings.FindAsync(id);
            if (entity == null) return NotFound();

            _context.ArCrRatings.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
