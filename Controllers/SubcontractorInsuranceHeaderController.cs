using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcontractorInsuranceHeaderController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcontractorInsuranceHeaderController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubcontractorInsuranceHeader dto)
        {
            if (dto.RequiredStartDate > dto.RequiredEndDate)
                return BadRequest("Start date cannot be after end date");

            _context.SubcontractorInsuranceHeaders.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
        [HttpGet]
        public async Task<IActionResult> Get(string vendorId, string projectId)
        {
            var data = await _context.SubcontractorInsuranceHeaders
                .Include(x => x.Lines)
                .Where(x => x.VendorId == vendorId && x.ProjectId == projectId)
                .ToListAsync();

            return Ok(data);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(string vendorId, string projectId, string policyType)
        {
            var hasLines = await _context.SubcontractorInsuranceLines
                .AnyAsync(x => x.VendorId == vendorId &&
                               x.ProjectId == projectId &&
                               x.PolicyType == policyType);

            if (hasLines)
                return BadRequest("Cannot delete. Lines exist.");

            var entity = await _context.SubcontractorInsuranceHeaders.FindAsync(vendorId, projectId, policyType);

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
