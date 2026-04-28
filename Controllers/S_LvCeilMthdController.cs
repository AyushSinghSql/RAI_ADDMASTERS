using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Helpers;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/lv-ceil-method")]
    [ApiController]
    public class SLvCeilMthdController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SLvCeilMthdController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.SLvCeilMthds.AsNoTracking().ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.SLvCeilMthds.FindAsync(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SLvCeilMthd model)
        {
            model.Code = model.Code.Trim().ToUpper();

            if (await _context.SLvCeilMthds.AnyAsync(x => x.Code == model.Code))
                return Conflict("Duplicate");

            AuditHelper.ApplyCreate(model);

            _context.SLvCeilMthds.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, SLvCeilMthd model)
        {
            var entity = await _context.SLvCeilMthds.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Description = model.Description;
            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.SLvCeilMthds.FindAsync(id);
            if (entity == null) return NotFound();

            _context.SLvCeilMthds.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}