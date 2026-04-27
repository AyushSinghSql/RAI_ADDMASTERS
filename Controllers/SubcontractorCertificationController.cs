using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcontractorCertificationController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcontractorCertificationController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubcontractorCertification dto)
        {
            // Required
            if (string.IsNullOrWhiteSpace(dto.VendorEmployeeId) ||
                string.IsNullOrWhiteSpace(dto.VendorId) ||
                string.IsNullOrWhiteSpace(dto.CompanyId))
                return BadRequest("Key fields are required");

            // Expiry validation
            if (dto.ExpirationDate.HasValue &&
                dto.LastRenewalDate.HasValue &&
                dto.ExpirationDate < dto.LastRenewalDate)
                return BadRequest("Expiration date cannot be before last renewal");

            // Years validation
            if (dto.CertificationYears < 0)
                return BadRequest("Invalid certification years");

            // Duplicate prevention
            var exists = await _context.SubcontractorCertifications.AnyAsync(x =>
                //x.CertificationKey == dto.CertificationKey &&
                x.VendorEmployeeId == dto.VendorEmployeeId &&
                x.VendorId == dto.VendorId &&
                x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Duplicate certification record");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string vendorId, string vendorEmployeeId, string companyId)
        {
            var data = await _context.SubcontractorCertifications
                .Where(x => x.VendorId == vendorId && x.VendorEmployeeId == vendorEmployeeId && x.CompanyId == companyId)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Update(SubcontractorCertification dto)
        {
            var entity = await _context.SubcontractorCertifications
                .FindAsync(dto.CertificationKey, dto.VendorEmployeeId, dto.VendorId, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.CertificationId = dto.CertificationId;
            entity.LicenseNumber = dto.LicenseNumber;
            entity.ExpirationDate = dto.ExpirationDate;
            entity.LastRenewalDate = dto.LastRenewalDate;
            entity.CertificationYears = dto.CertificationYears;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        [HttpDelete("{key}/{emplId}/{vendorId}/{companyId}")]
        public async Task<IActionResult> Delete(long key, string emplId, string vendorId, string companyId)
        {
            var entity = await _context.SubcontractorCertifications
                .FindAsync(key, emplId, vendorId, companyId);

            if (entity == null)
                return NotFound();

            // Prevent delete if used elsewhere (optional)
            var isUsed = await _context.SubcontractorInsuranceLines
                .AnyAsync(x => x.VendorId == vendorId);

            if (isUsed)
                return BadRequest("Certification is linked to transactions");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string companyId)
        {
            var data = await _context.SubcontractorCertifications
                .Where(x => x.CompanyId == companyId)
                .Select(x => new {
                    value = x.CertificationId,
                    label = x.CertificationId
                })
                .Distinct()
                .ToListAsync();

            return Ok(data);
        }
    }
}
