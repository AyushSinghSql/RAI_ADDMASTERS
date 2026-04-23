using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcontractorLienController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcontractorLienController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(SubcontractorLien dto)
        {
            // Mandatory fields
            if (string.IsNullOrEmpty(dto.VendorId) || string.IsNullOrEmpty(dto.ProjectId))
                return BadRequest("Vendor and Project are required");

            // Lien flag validation
            if (dto.LienReleasedFl != "Y" && dto.LienReleasedFl != "N")
                return BadRequest("LienReleasedFl must be Y or N");

            // Released date rule
            if (dto.LienReleasedFl == "Y" && dto.LienReleasedDate == null)
                return BadRequest("Released date required when lien is released");

            // Date logic
            if (dto.LienReleasedDate != null && dto.LienReleasedDate < dto.EffectiveDate)
                return BadRequest("Released date cannot be before effective date");

            // Prevent duplicate
            var exists = await _context.SubcontractorLiens
                .AnyAsync(x =>
                    x.VendorId == dto.VendorId &&
                    x.ProjectId == dto.ProjectId &&
                    x.LienKey == dto.LienKey);

            if (exists)
                return BadRequest("Duplicate lien record");

            _context.SubcontractorLiens.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string vendorId, string projectId)
        {
            var data = await _context.SubcontractorLiens
                .Where(x => x.VendorId == vendorId && x.ProjectId == projectId)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Update(SubcontractorLien dto)
        {
            var entity = await _context.SubcontractorLiens
                .FindAsync(dto.VendorId, dto.ProjectId, dto.LienKey);

            if (entity == null)
                return NotFound();

            entity.LienReleasedFl = dto.LienReleasedFl;
            entity.LienReleasedDate = dto.LienReleasedDate;
            entity.IssuedByName = dto.IssuedByName;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.AddressLine1 = dto.AddressLine1;
            entity.City = dto.City;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string vendorId, string projectId, long lienKey)
        {
            var entity = await _context.SubcontractorLiens
                .FindAsync(vendorId, projectId, lienKey);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
