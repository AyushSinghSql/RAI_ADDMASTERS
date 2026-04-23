using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/employee-allowances")]
    public class EmployeeAllowanceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeeAllowanceController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var data = await _context.EmployeeAllowances
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeAllowance model)
        {
            // 🔹 Validation
            if (model.AllowanceRate <= 0)
                return BadRequest("Allowance rate must be greater than 0.");

            if (model.EffectiveDate.HasValue && model.EndDate.HasValue &&
                model.EndDate < model.EffectiveDate)
                return BadRequest("End date cannot be before effective date.");

            var exists = await _context.EmployeeAllowances.AnyAsync(x =>
                x.EmployeeId == model.EmployeeId &&
                x.AllowanceCode == model.AllowanceCode);

            if (exists)
                return BadRequest("Allowance already assigned to employee.");

            model.TimeStamp = DateTime.UtcNow;

            _context.EmployeeAllowances.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(EmployeeAllowance model)
        {
            var entity = await _context.EmployeeAllowances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.AllowanceCode == model.AllowanceCode);

            if (entity == null)
                return NotFound();

            // 🔹 Validation
            if (model.AllowanceRate <= 0)
                return BadRequest("Allowance rate must be greater than 0.");

            if (model.EffectiveDate.HasValue && model.EndDate.HasValue &&
                model.EndDate < model.EffectiveDate)
                return BadRequest("End date cannot be before effective date.");

            // 🔹 Update fields
            entity.AccountId = model.AccountId;
            entity.ProjectId = model.ProjectId;
            entity.OrganizationId = model.OrganizationId;
            entity.AllowanceRate = model.AllowanceRate;
            entity.EffectiveDate = model.EffectiveDate;
            entity.EndDate = model.EndDate;
            entity.Ref1Id = model.Ref1Id;
            entity.Ref2Id = model.Ref2Id;
            entity.GeneralLaborCategory = model.GeneralLaborCategory;
            entity.BillingLaborCategory = model.BillingLaborCategory;
            entity.LaborLocationCode = model.LaborLocationCode;
            entity.WorkCompCode = model.WorkCompCode;
            entity.WhStateCode = model.WhStateCode;
            entity.ModifiedBy = model.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{employeeId}/{allowanceCode}")]
        public async Task<IActionResult> Delete(string employeeId, string allowanceCode)
        {
            var entity = await _context.EmployeeAllowances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.AllowanceCode == allowanceCode);

            if (entity == null)
                return NotFound();

            // 🔴 OPTIONAL VALIDATION (if used in payroll / transactions)
            var isUsed = false; // Replace with actual check
            if (isUsed)
                return BadRequest("Cannot delete. Allowance is used in transactions.");

            _context.EmployeeAllowances.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
