using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcontractorInsuranceLineController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcontractorInsuranceLineController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddLine(SubcontractorInsuranceLine dto)
        {
            // Expiry validation
            if (dto.EffectiveDate > dto.ExpiryDate)
                return BadRequest("Effective date must be before expiry date");

            // Amount validation
            if (dto.InsuranceAmount <= 0)
                return BadRequest("Insurance amount must be greater than zero");

            // Prevent duplicate policy number per header
            var exists = await _context.SubcontractorInsuranceLines
                .AnyAsync(x =>
                    x.VendorId == dto.VendorId &&
                    x.ProjectId == dto.ProjectId &&
                    x.PolicyType == dto.PolicyType &&
                    x.PolicyNumber == dto.PolicyNumber);

            if (exists)
                return BadRequest("Duplicate policy number not allowed");

            _context.SubcontractorInsuranceLines.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLine(string vendorId, string projectId, string policyType, long lineKey)
        {
            var entity = await _context.SubcontractorInsuranceLines
                .FindAsync(vendorId, projectId, policyType, lineKey);

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
