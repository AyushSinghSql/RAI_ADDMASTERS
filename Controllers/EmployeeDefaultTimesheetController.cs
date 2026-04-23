using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/employee-default-timesheet")]
    public class EmployeeDefaultTimesheetController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeeDefaultTimesheetController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.EmployeeDefaultTimesheets.ToListAsync());
        }

        // ✅ GET BY ID
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> Get(string employeeId)
        {
            var data = await _context.EmployeeDefaultTimesheets.FindAsync(employeeId);
            if (data == null) return NotFound();

            return Ok(data);
        }

        // ✅ CREATE / UPSERT
        [HttpPost]
        public async Task<IActionResult> Upsert(EmployeeDefaultTimesheet dto)
        {
            if (string.IsNullOrEmpty(dto.EmployeeId))
                return BadRequest("EmployeeId is required");

            var existing = await _context.EmployeeDefaultTimesheets
                .FindAsync(dto.EmployeeId);

            if (existing == null)
            {
                dto.TimeStamp = DateTime.UtcNow;
                _context.EmployeeDefaultTimesheets.Add(dto);
            }
            else
            {
                // Update fields
                existing.AccountId = dto.AccountId;
                existing.ProjectId = dto.ProjectId;
                existing.GeneralLaborCategoryCode = dto.GeneralLaborCategoryCode;
                existing.WorkCompCode = dto.WorkCompCode;
                existing.PayType = dto.PayType;
                existing.RefStructure1Id = dto.RefStructure1Id;
                existing.RefStructure2Id = dto.RefStructure2Id;
                existing.ChargeOrgId = dto.ChargeOrgId;
                existing.LaborLocationCode = dto.LaborLocationCode;

                existing.ModifiedBy = dto.ModifiedBy;
                existing.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ DELETE
        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> Delete(string employeeId)
        {
            var entity = await _context.EmployeeDefaultTimesheets.FindAsync(employeeId);
            if (entity == null) return NotFound();

            _context.EmployeeDefaultTimesheets.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
