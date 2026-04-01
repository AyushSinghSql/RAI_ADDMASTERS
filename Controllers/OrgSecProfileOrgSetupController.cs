using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgSecProfileOrgSetupController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public OrgSecProfileOrgSetupController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (JOINS 🔥)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OrgSecProfileOrgs
                .Include(x => x.OrgSecProfile)
                .Include(x => x.Organization)   // ✅ Updated
                .Include(x => x.Company)
                .Select(x => new OrgSecProfileOrgDto
                {
                    OrgSecProfCd = x.OrgSecProfCd,
                    OrgId = x.OrgId,
                    CompanyId = x.CompanyId,
                    OrgWildcardFl = x.OrgWildcardFl,
                    SOrgRightsCd = x.SOrgRightsCd,

                    // 🔥 Joined Fields
                    ProfileName = x.OrgSecProfile.Name,
                    OrgName = x.Organization.OrgName, // from org_groups
                    CompanyName = x.Company.CompanyName
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY KEY
        [HttpGet("{profCd}/{orgId}/{companyId}")]
        public async Task<IActionResult> Get(string profCd, string orgId, string companyId)
        {
            var data = await _context.OrgSecProfileOrgs
                .Include(x => x.OrgSecProfile)
                .Include(x => x.Organization)
                .Include(x => x.Company)
                .Where(x => x.OrgSecProfCd == profCd
                         && x.OrgId == orgId
                         && x.CompanyId == companyId)
                .Select(x => new OrgSecProfileOrgDto
                {
                    OrgSecProfCd = x.OrgSecProfCd,
                    OrgId = x.OrgId,
                    CompanyId = x.CompanyId,
                    OrgWildcardFl = x.OrgWildcardFl,
                    SOrgRightsCd = x.SOrgRightsCd,

                    ProfileName = x.OrgSecProfile.Name,
                    OrgName = x.Organization.OrgName,
                    CompanyName = x.Company.CompanyName
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Record not found." });

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(OrgSecProfileOrgDto dto)
        {
            var entity = new OrgSecProfileOrg
            {
                OrgSecProfCd = dto.OrgSecProfCd,
                OrgId = dto.OrgId,
                CompanyId = dto.CompanyId,
                OrgWildcardFl = dto.OrgWildcardFl,
                SOrgRightsCd = dto.SOrgRightsCd,
                ModifiedBy = "system",
                TimeStamp = DateTime.UtcNow
            };

            _context.OrgSecProfileOrgs.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Record created successfully." });
        }

        // ✅ UPDATE
        [HttpPut("{profCd}/{orgId}/{companyId}")]
        public async Task<IActionResult> Update(string profCd, string orgId, string companyId, OrgSecProfileOrgDto dto)
        {
            var entity = await _context.OrgSecProfileOrgs
                .FirstOrDefaultAsync(x => x.OrgSecProfCd == profCd
                                      && x.OrgId == orgId
                                      && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            entity.OrgWildcardFl = dto.OrgWildcardFl;
            entity.SOrgRightsCd = dto.SOrgRightsCd;
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

            return Ok(new { message = "Record updated successfully." });
        }

        // ✅ DELETE
        [HttpDelete("{profCd}/{orgId}/{companyId}")]
        public async Task<IActionResult> Delete(string profCd, string orgId, string companyId)
        {
            var entity = await _context.OrgSecProfileOrgs
                .FirstOrDefaultAsync(x => x.OrgSecProfCd == profCd
                                      && x.OrgId == orgId
                                      && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            _context.OrgSecProfileOrgs.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }

            return Ok(new { message = "Record deleted successfully." });
        }

        // ✅ Error Handling
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                    return "This mapping already exists.";

                if (pgEx.SqlState == "23503")
                {
                    return pgEx.ConstraintName switch
                    {
                        "fk_profile_org" => "Invalid Profile selected.",
                        "fk_company" => "Invalid Company selected.",
                        "fk_org_groups" => "Invalid Organization selected.",
                        _ => "Reference data not found."
                    };
                }

                if (pgEx.SqlState == "23502")
                    return "A required field is missing.";
            }

            return "An error occurred while saving data.";
        }
    }
}
