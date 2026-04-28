using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArShipMthdController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ArShipMthdController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 100)
        {
            pageSize = Math.Min(pageSize, 500);

            var data = await _context.ArShipMthds
                .AsNoTracking()
                .OrderBy(x => x.ArShipMthdKey)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET ONE
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var entity = await _context.ArShipMthds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ArShipMthdKey == id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(ArShipMthd model)
        {
            try
            {
                Validate(model);

                var exists = await _context.ArShipMthds
                    .AnyAsync(x => x.ArShipMthdKey == model.ArShipMthdKey);

                if (exists)
                    return Conflict("Duplicate key.");

                var descExists = await _context.ArShipMthds
                    .AnyAsync(x => x.ShipMthdDc == model.ShipMthdDc);

                if (descExists)
                    return Conflict("Duplicate shipping method.");

                model.TimeStamp = DateTime.UtcNow;
                model.RowVersion = 1;

                _context.ArShipMthds.Add(model);
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ArShipMthd model)
        {
            try
            {
                if (id != model.ArShipMthdKey)
                    return BadRequest("Key mismatch.");

                Validate(model);

                var entity = await _context.ArShipMthds
                    .FirstOrDefaultAsync(x => x.ArShipMthdKey == id);

                if (entity == null)
                    return NotFound();

                // Concurrency check
                if (entity.RowVersion != model.RowVersion)
                    return Conflict("Record modified by another user.");

                var duplicate = await _context.ArShipMthds
                    .AnyAsync(x => x.ShipMthdDc == model.ShipMthdDc && x.ArShipMthdKey != id);

                if (duplicate)
                    return Conflict("Duplicate shipping method.");

                entity.ShipMthdDc = model.ShipMthdDc;
                entity.ModifiedBy = model.ModifiedBy;
                entity.TimeStamp = DateTime.UtcNow;
                entity.RowVersion++;

                await _context.SaveChangesAsync();

                return Ok(entity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.ArShipMthds
                .FirstOrDefaultAsync(x => x.ArShipMthdKey == id);

            if (entity == null)
                return NotFound();

            _context.ArShipMthds.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
        [NonAction]
        public static string Normalize(string val)
        => val?.Trim().ToUpperInvariant();

        [NonAction]
        public static void Validate(ArShipMthd m)
        {
            if (m == null)
                throw new ArgumentException("Request required.");

            m.ShipMthdDc = Normalize(m.ShipMthdDc);
            m.ModifiedBy = m.ModifiedBy?.Trim();

            if (m.ArShipMthdKey < 0)
                throw new ArgumentException("Invalid ar_ship_mthd_key.");

            if (string.IsNullOrWhiteSpace(m.ShipMthdDc) || m.ShipMthdDc.Length > 15)
                throw new ArgumentException("Invalid ship_mthd_dc.");

            if (string.IsNullOrWhiteSpace(m.ModifiedBy) || m.ModifiedBy.Length > 20)
                throw new ArgumentException("Invalid modified_by.");
        }

    }
}
