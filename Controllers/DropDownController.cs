using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/dropdowns")]
    public class DropdownController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public DropdownController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("ProfOrgs")]
        public async Task<IActionResult> ProfOrgs()
        {
            var data = await _context.ProfOrgs
                .Select(x => new
                {
                    value = x.ProfOrgId,
                    label = x.ProfOrgDesc
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 1. Certifications Dropdown
        [HttpGet("certifications")]
        public async Task<IActionResult> GetCertifications()
        {
            var data = await _context.VendorCertificationSetups
                .Where(x => x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertName)
                .Select(x => new DropdownDto
                {
                    Code = x.CertCode,
                    Description = x.CertName
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 2. Levels by Certification
        [HttpGet("certifications/{certCd}/levels")]
        public async Task<IActionResult> GetLevels(string certCd)
        {
            var data = await _context.CertificationLevels
                .Where(x => x.CertCd == certCd && x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertLevelCd)
                .Select(x => new DropdownDto
                {
                    Code = x.CertLevelCd,
                    Description = x.CertLevelDesc
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 3. Status by Certification
        [HttpGet("certifications/{certCd}/status")]
        public async Task<IActionResult> GetStatus(string certCd)
        {
            var data = await _context.CertificationStatuses
                .Where(x => x.CertCd == certCd && x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertStatusCd)
                .Select(x => new DropdownDto
                {
                    Code = x.CertStatusCd,
                    Description = x.CertStatusDesc
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }


    }
}
