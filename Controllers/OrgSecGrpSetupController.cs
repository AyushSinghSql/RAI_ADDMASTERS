using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgSecGrpSetupController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public OrgSecGrpSetupController(MydatabaseContext context)
        {
            _context = context;
        }

        // Modules dropdown
        [HttpGet("modules-dropdown")]
        public async Task<IActionResult> GetModules()
        {
            return Ok(await _context.Modules
                .Select(x => new { x.ModuleCd, x.Name })
                .ToListAsync());
        }

        // Profiles dropdown
        [HttpGet("profiles-dropdown")]
        public async Task<IActionResult> GetProfiles()
        {
            return Ok(await _context.OrgSecProfiles
                .Select(x => new { x.OrgSecProfCd, x.Name })
                .ToListAsync());
        }

        // ✅ GET ALL (with joins)
        [HttpGet("GetAllOrgGroups")]
        public async Task<IActionResult> GetAllOrgGroups()
        {
            var data = await _context.OrgGroup
                       .ToListAsync();

            return Ok(data);
        }

        // ✅ GET ALL (with joins)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OrgSecGrpSetups
                .Include(x => x.Module)
                .Include(x => x.OrgSecProfile)
                .Select(x => new OrgSecGrpSetupDto
                {
                    OrgSecGrpCd = x.OrgSecGrpCd,
                    ModuleCd = x.ModuleCd,
                    CompanyId = x.CompanyId,
                    OrgSecProfCd = x.OrgSecProfCd,
                    ModuleName = x.Module.Name,
                    ProfileName = x.OrgSecProfile.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        //[HttpGet("by-group/{grpCd}")]
        //public async Task<IActionResult> GetByGroupStructured(string grpCd)
        //{
        //    var data = await _context.OrgSecGrpSetups
        //        .Where(x => x.OrgSecGrpCd == grpCd)
        //        .ToListAsync();

        //    if (!data.Any())
        //        return NotFound(new { message = "No mappings found for this group." });

        //    var result = new
        //    {
        //        GroupCode = grpCd,

        //        Modules = data
        //            .Select(x => new { x.ModuleCd, ModuleName = x.Module.Name })
        //            .Distinct()
        //            .ToList(),

        //        Profiles = data
        //            .Select(x => new { x.OrgSecProfCd, ProfileName = x.OrgSecProfile.Name })
        //            .Distinct()
        //            .ToList()
        //    };

        //    return Ok(result);
        //}

        [HttpGet("all-groups-structured")]
        public async Task<IActionResult> GetAllGroupsStructured()
        {
            var result = await _context.OrgGroup
                .Select(g => new
                {
                    GroupCode = g.OrgGroupCode,
                    GroupName = g.OrgGroupName,

                    Modules = _context.OrgSecGrpSetups
                        .Where(s => s.OrgSecGrpCd == g.OrgGroupCode)
                        .Select(s => new
                        {
                            s.ModuleCd,
                            ModuleName = s.Module.Name
                        })
                        .Distinct()
                        .ToList(),

                    Profiles = _context.OrgSecGrpSetups
                        .Where(s => s.OrgSecGrpCd == g.OrgGroupCode)
                        .Select(s => new
                        {
                            s.OrgSecProfCd,
                            ProfileName = s.OrgSecProfile.Name
                        })
                        .Distinct()
                        .ToList()
                })
                .ToListAsync();

            return Ok(result);
        }

        // ✅ GET BY KEY
        [HttpGet("{grpCd}/{moduleCd}/{companyId}")]
        public async Task<IActionResult> Get(string grpCd, string moduleCd, string companyId)
        {
            var data = await _context.OrgSecGrpSetups
                .Include(x => x.Module)
                .Include(x => x.OrgSecProfile)
                .Where(x => x.OrgSecGrpCd == grpCd
                         && x.ModuleCd == moduleCd
                         && x.CompanyId == companyId)
                .Select(x => new OrgSecGrpSetupDto
                {
                    OrgSecGrpCd = x.OrgSecGrpCd,
                    ModuleCd = x.ModuleCd,
                    CompanyId = x.CompanyId,
                    OrgSecProfCd = x.OrgSecProfCd,
                    ModuleName = x.Module.Name,
                    ProfileName = x.OrgSecProfile.Name
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Record not found." });

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(OrgSecGrpSetupDto dto)
        {
            var entity = new OrgSecGrpSetup
            {
                OrgSecGrpCd = dto.OrgSecGrpCd,
                ModuleCd = dto.ModuleCd,
                CompanyId = dto.CompanyId,
                OrgSecProfCd = dto.OrgSecProfCd,
                ModifiedBy = "system",
                TimeStamp = DateTime.UtcNow
            };

            _context.OrgSecGrpSetups.Add(entity);

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
        [HttpPut("{grpCd}/{moduleCd}/{companyId}")]
        public async Task<IActionResult> Update(string grpCd, string moduleCd, string companyId, OrgSecGrpSetupDto dto)
        {
            var entity = await _context.OrgSecGrpSetups
                .FirstOrDefaultAsync(x => x.OrgSecGrpCd == grpCd
                                      && x.ModuleCd == moduleCd
                                      && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            entity.OrgSecProfCd = dto.OrgSecProfCd;
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
        [HttpDelete("{grpCd}/{moduleCd}/{companyId}")]
        public async Task<IActionResult> Delete(string grpCd, string moduleCd, string companyId)
        {
            var entity = await _context.OrgSecGrpSetups
                .FirstOrDefaultAsync(x => x.OrgSecGrpCd == grpCd
                                      && x.ModuleCd == moduleCd
                                      && x.CompanyId == companyId);

            if (entity == null)
                return NotFound(new { message = "Record not found." });

            _context.OrgSecGrpSetups.Remove(entity);

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

        // ✅ Error Handler (Constraint-based 🔥)
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                {
                    return pgEx.ConstraintName switch
                    {
                        "pk_org_sec_grp_setup" => "This mapping already exists.",
                        _ => "Duplicate value already exists."
                    };
                }

                if (pgEx.SqlState == "23503")
                {
                    return pgEx.ConstraintName switch
                    {
                        "fk_org_groups" => "Invalid Org Group selected.",
                        "fk_org_sec_profile" => "Invalid Profile selected.",
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
