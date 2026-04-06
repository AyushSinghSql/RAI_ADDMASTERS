using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrgSecProfileController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public OrgSecProfileController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET: api/orgsecprofile
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OrgSecProfiles
                .Select(x => new OrgSecProfileDto
                {
                    OrgSecProfCd = x.OrgSecProfCd,
                    CompanyId = x.CompanyId,
                    Name = x.Name,
                    RightsAppCOde_Flag = x.RightsAppCOde_Flag,
                    Profile_Org_Flag = x.Profile_Org_Flag
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET: api/orgsecprofile/{code}/{companyId}
        [HttpGet("{code}/{companyId}")]
        public async Task<IActionResult> Get(string code, string companyId)
        {
            var data = await _context.OrgSecProfiles
                .Where(x => x.OrgSecProfCd == code && x.CompanyId == companyId)
                .Select(x => new OrgSecProfileDto
                {
                    OrgSecProfCd = x.OrgSecProfCd,
                    CompanyId = x.CompanyId,
                    Name = x.Name,
                    RightsAppCOde_Flag = x.RightsAppCOde_Flag,
                    Profile_Org_Flag = x.Profile_Org_Flag
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Profile not found." });

            return Ok(data);
        }

        // ✅ POST
        [HttpPost]
        public async Task<IActionResult> Create(OrgSecProfileDto dto)
        {
            var entity = new OrgSecProfile
            {
                OrgSecProfCd = dto.OrgSecProfCd,
                CompanyId = dto.CompanyId,
                Name = dto.Name,
                ModifiedBy = "system",
                Profile_Org_Flag = dto.Profile_Org_Flag,
                RightsAppCOde_Flag = dto.RightsAppCOde_Flag,
                TimeStamp = DateTime.UtcNow
            };

            _context.OrgSecProfiles.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return CreatedAtAction(nameof(Get),
                new { code = entity.OrgSecProfCd, companyId = entity.CompanyId },
                dto);
        }

        // ✅ PUT
        [HttpPut("{code}/{companyId}")]
        public async Task<IActionResult> Update(string code, string companyId, OrgSecProfileDto dto)
        {
            var entity = await _context.OrgSecProfiles
                .FirstOrDefaultAsync(x => x.OrgSecProfCd == code && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Profile not found." });

            entity.RightsAppCOde_Flag = dto.RightsAppCOde_Flag;
            entity.Profile_Org_Flag = dto.Profile_Org_Flag;
            entity.Name = dto.Name;
            entity.ModifiedBy = "system";
            entity.TimeStamp = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        // ✅ DELETE
        [HttpDelete("{code}/{companyId}")]
        public async Task<IActionResult> Delete(string code, string companyId)
        {
            var entity = await _context.OrgSecProfiles
                .FirstOrDefaultAsync(x => x.OrgSecProfCd == code && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Profile not found." });

            _context.OrgSecProfiles.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Profile deleted successfully." });
        }

        // ✅ Error Handler
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                    return "Profile already exists.";

                if (pgEx.SqlState == "23503")
                    return "This profile is used in other records.";

                if (pgEx.SqlState == "23502")
                    return "A required field is missing.";
            }

            return "An error occurred while saving data.";
        }
    }
}
