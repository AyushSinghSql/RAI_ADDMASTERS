using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/employee-pay-types")]
    public class EmployeeDefaultPayTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeeDefaultPayTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET by employee
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {

            //var data = await _context.EmployeeDefaultPayTypes.Where(x => x.EmployeeId == employeeId).ToListAsync();

            var data = await _context.EmployeeDefaultPayTypes
                .Where(x => x.EmployeeId == employeeId)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.PayType,
                    Description = x.PayTypeNavigation.Description,
                    x.ModifiedBy,
                    x.TimeStamp
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ ADD PAY TYPE
        [HttpPost]
        public async Task<IActionResult> Add(EmployeeDefaultPayType dto)
        {
            if (string.IsNullOrEmpty(dto.EmployeeId) || string.IsNullOrEmpty(dto.PayType))
                return BadRequest("EmployeeId & PayType required");

            var empExists = await _context.Empls
                .AnyAsync(x => x.EmplId == dto.EmployeeId);

            if (!empExists)
                return BadRequest("Invalid Employee");

            // Prevent duplicate
            var exists = await _context.EmployeeDefaultPayTypes
                .AnyAsync(x => x.EmployeeId == dto.EmployeeId && x.PayType == dto.PayType);

            if (exists)
                return BadRequest("Pay type already assigned");

            dto.TimeStamp = DateTime.UtcNow;

            _context.EmployeeDefaultPayTypes.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ REMOVE PAY TYPE
        [HttpDelete("{employeeId}/{payType}")]
        public async Task<IActionResult> Delete(string employeeId, string payType)
        {
            var entity = await _context.EmployeeDefaultPayTypes
                .FindAsync(employeeId, payType);

            if (entity == null)
                return NotFound();

            _context.EmployeeDefaultPayTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
