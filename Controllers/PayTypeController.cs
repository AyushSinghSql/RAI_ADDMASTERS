using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/pay-types")]
    public class PayTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public PayTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.PayTypes.ToListAsync());
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(PayType dto)
        {
            // Duplicate check
            if (await _context.PayTypes.AnyAsync(x => x.PayTypeCode == dto.PayTypeCode))
                return BadRequest("Pay Type already exists");

            // Validation
            if (dto.Factor <= 0)
                return BadRequest("Factor must be > 0");

            if (dto.Amount < 0)
                return BadRequest("Amount cannot be negative");

            if (dto.AllowRecast == "Y" && string.IsNullOrEmpty(dto.RecastPayType))
                return BadRequest("Recast Pay Type required");

            if (dto.PayTypeCode == dto.RecastPayType)
                return BadRequest("Cannot recast to same pay type");

            var used = await _context.EmployeeDefaultPayTypes
                .AnyAsync(x => x.PayType == dto.PayTypeCode);

            if (used)
                return BadRequest("Cannot delete: Pay Type in use");

            if (dto.ApplySalary == "Y" && dto.ApplyToExempt == "N")
                return BadRequest("Salary must apply to exempt");

            dto.TimeStamp = DateTime.UtcNow;

            _context.PayTypes.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, PayType dto)
        {
            var entity = await _context.PayTypes.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Description = dto.Description;
            entity.Amount = dto.Amount;
            entity.Factor = dto.Factor;
            entity.Active = dto.Active;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _context.PayTypes.FindAsync(id);
            if (entity == null) return NotFound();

            // Soft delete recommended
            entity.Active = "N";
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
