using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/leave-balances")]
    public class EmployeeLeaveBalanceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeeLeaveBalanceController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET by employee + year
        [HttpGet("{employeeId}/{year}")]
        public async Task<IActionResult> Get(string employeeId, int year)
        {
            var data = await _context.EmployeeLeaveBalances
                .Where(x => x.EmployeeId == employeeId && x.LeaveYear == year)
                .ToListAsync();

            return Ok(data);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeLeaveBalance dto)
        {
            // Prevent duplicate
            var exists = await _context.EmployeeLeaveBalances
                .AnyAsync(x => x.EmployeeId == dto.EmployeeId
                            && x.LeaveYear == dto.LeaveYear
                            && x.LeaveTypeCode == dto.LeaveTypeCode);

            if (exists)
                return BadRequest("Leave balance already exists");

            // Validation
            if (dto.BeginBalanceHours < 0 || dto.BeginBalanceAmount < 0)
                return BadRequest("Beginning balance cannot be negative");

            dto.TimeStamp = DateTime.UtcNow;

            _context.EmployeeLeaveBalances.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(EmployeeLeaveBalance dto)
        {
            var entity = await _context.EmployeeLeaveBalances
                .FindAsync(dto.EmployeeId, dto.LeaveYear, dto.LeaveTypeCode);

            if (entity == null)
                return NotFound();

            entity.YtdAccruedHours = dto.YtdAccruedHours;
            entity.YtdUsedHours = dto.YtdUsedHours;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{employeeId}/{year}/{type}")]
        public async Task<IActionResult> Delete(string employeeId, int year, string type)
        {
            var entity = await _context.EmployeeLeaveBalances
                .FindAsync(employeeId, year, type);

            if (entity == null)
                return NotFound();

            _context.EmployeeLeaveBalances.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
