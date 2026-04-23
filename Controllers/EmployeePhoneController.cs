using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/employee-phones")]
    public class EmployeePhoneController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeePhoneController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET by employee
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var data = await _context.EmployeePhones
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.SequenceNo)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ UPSERT
        [HttpPost]
        public async Task<IActionResult> Upsert(EmployeePhone dto)
        {
            if (string.IsNullOrEmpty(dto.EmployeeId) || string.IsNullOrEmpty(dto.PhoneTypeCode))
                return BadRequest("EmployeeId & PhoneTypeCode required");

            var existing = await _context.EmployeePhones.FindAsync(dto.EmployeeId, dto.PhoneTypeCode);

            if (existing == null)
            {
                // Auto sequence
                var maxSeq = await _context.EmployeePhones
                    .Where(x => x.EmployeeId == dto.EmployeeId)
                    .MaxAsync(x => (int?)x.SequenceNo) ?? 0;

                dto.SequenceNo = maxSeq + 1;
                dto.TimeStamp = DateTime.UtcNow;

                _context.EmployeePhones.Add(dto);
            }
            else
            {
                existing.PhoneNumber = dto.PhoneNumber;
                existing.PhoneExtension = dto.PhoneExtension;
                existing.ModifiedBy = dto.ModifiedBy;
                existing.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        // ✅ DELETE
        [HttpDelete("{employeeId}/{phoneType}")]
        public async Task<IActionResult> Delete(string employeeId, string phoneType)
        {
            var entity = await _context.EmployeePhones.FindAsync(employeeId, phoneType);
            if (entity == null) return NotFound();

            _context.EmployeePhones.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
