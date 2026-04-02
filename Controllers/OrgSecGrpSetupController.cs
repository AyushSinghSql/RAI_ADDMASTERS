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

        // Modules dropdown
        [HttpGet("GetAllModules")]
        public async Task<IActionResult> GetAllModules(string? profileCD, string? profileName)
        {
            return Ok(await _context.Modules
                .Select(x => new OrgSecGrpSetupDto
                {
                    OrgSecGrpCd = string.Empty,
                    ModuleCd = x.ModuleCd,
                    CompanyId = x.CompanyId,
                    OrgSecProfCd = profileCD??string.Empty,
                    ModuleName = x.Name,
                    ProfileName = profileName??string.Empty
                })
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

        [HttpGet("by-group/{grpCd}")]
        public async Task<IActionResult> GetByGroupStructured(string grpCd)
        {
            var data = await (
                from m in _context.Modules

                join s in _context.OrgSecGrpSetups
                    .Where(x => x.OrgSecGrpCd == grpCd)
                    on m.ModuleCd equals s.ModuleCd into ms
                from s in ms.DefaultIfEmpty()

                join p in _context.OrgSecProfiles
                    on s.OrgSecProfCd equals p.OrgSecProfCd into sp
                from p in sp.DefaultIfEmpty()

                select new
                {
                    ModuleCd = m.ModuleCd,
                    ModuleName = m.Name,

                    OrgSecProfCd = s != null ? s.OrgSecProfCd : string.Empty,
                    ProfileName = p != null ? p.Name : string.Empty
                }
            ).ToListAsync();

            return Ok(data);
        }

        //[HttpGet("by-group/{grpCd}")]
        //public async Task<IActionResult> GetByGroupStructured(string grpCd)
        //{
        //    var data = await _context.OrgSecGrpSetups
        //        .Include(x => x.Module)
        //        .Include(x => x.OrgSecProfile)
        //        .Where(x => x.OrgSecGrpCd == grpCd)
        //        .Select(x => new
        //        {
        //            x.OrgSecGrpCd,

        //            x.ModuleCd,
        //            ModuleName = x.Module.Name,

        //            x.OrgSecProfCd,
        //            ProfileName = x.OrgSecProfile.Name
        //        })
        //        .ToListAsync();

        //    if (!data.Any())
        //        return NotFound(new { message = "No mappings found for this group." });

        //    return Ok(data);
        //}

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

        [HttpPost("bulk-sync")]
        public async Task<IActionResult> BulkSync(List<OrgSecGrpSetupDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided.");

            var grpCodes = dtos.Select(x => x.OrgSecGrpCd).Distinct().ToList();
            var moduleCds = dtos.Select(x => x.ModuleCd).Distinct().ToList();
            var companyIds = dtos.Select(x => x.CompanyId).Distinct().ToList();

            // 🔍 Fetch existing
            var existing = await _context.OrgSecGrpSetups
                .Where(x => grpCodes.Contains(x.OrgSecGrpCd)
                         && companyIds.Contains(x.CompanyId))
                .ToListAsync();

            // 🔑 Create key sets
            string GetKey(string g, string m, string c) => $"{g}|{m}|{c}";

            var incomingKeys = dtos
                .Select(d => GetKey(d.OrgSecGrpCd, d.ModuleCd, d.CompanyId))
                .ToHashSet();

            var existingKeys = existing
                .Select(e => GetKey(e.OrgSecGrpCd, e.ModuleCd, e.CompanyId))
                .ToHashSet();

            var toInsert = new List<OrgSecGrpSetup>();
            var toUpdate = new List<OrgSecGrpSetup>();

            // 🔁 UPSERT
            foreach (var dto in dtos)
            {
                var match = existing.FirstOrDefault(x =>
                    x.OrgSecGrpCd == dto.OrgSecGrpCd &&
                    x.ModuleCd == dto.ModuleCd &&
                    x.CompanyId == dto.CompanyId);

                if (match != null)
                {
                    // ✅ UPDATE
                    match.OrgSecProfCd = dto.OrgSecProfCd;
                    match.ModifiedBy = "system";
                    match.TimeStamp = DateTime.UtcNow;

                    toUpdate.Add(match);
                }
                else
                {
                    // ✅ INSERT
                    toInsert.Add(new OrgSecGrpSetup
                    {
                        OrgSecGrpCd = dto.OrgSecGrpCd,
                        ModuleCd = dto.ModuleCd,
                        CompanyId = dto.CompanyId,
                        OrgSecProfCd = dto.OrgSecProfCd,
                        ModifiedBy = "system",
                        TimeStamp = DateTime.UtcNow
                    });
                }
            }

            // 🔴 DELETE (Missing in incoming list)
            var toDelete = existing
                .Where(e => !incomingKeys.Contains(
                    GetKey(e.OrgSecGrpCd, e.ModuleCd, e.CompanyId)))
                .ToList();

            // 🚀 Apply changes
            if (toInsert.Any())
                await _context.OrgSecGrpSetups.AddRangeAsync(toInsert);

            if (toUpdate.Any())
                _context.OrgSecGrpSetups.UpdateRange(toUpdate);

            if (toDelete.Any())
                _context.OrgSecGrpSetups.RemoveRange(toDelete);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Inserted = toInsert.Count,
                Updated = toUpdate.Count,
                Deleted = toDelete.Count,
                Total = dtos.Count
            });
        }

        //// ✅ Bulk Upsert
        //[HttpPost("bulk-upsert")]
        //public async Task<IActionResult> BulkUpsert(List<OrgSecGrpSetupDto> dtos)
        //{
        //    if (dtos == null || !dtos.Any())
        //        return BadRequest("No data provided.");

        //    var grpCodes = dtos.Select(x => x.OrgSecGrpCd).Distinct().ToList();
        //    var moduleCds = dtos.Select(x => x.ModuleCd).Distinct().ToList();
        //    var companyIds = dtos.Select(x => x.CompanyId).Distinct().ToList();
        //    //var profileCds = dtos.Select(x => x.OrgSecProfCd).Distinct().ToList();

        //    // Fetch existing records in one go
        //    var existing = await _context.OrgSecGrpSetups.AsNoTracking()
        //        .Where(x => grpCodes.Contains(x.OrgSecGrpCd)
        //                 && moduleCds.Contains(x.ModuleCd)
        //                 && companyIds.Contains(x.CompanyId))
        //        .ToListAsync();

        //    var toInsert = new List<OrgSecGrpSetup>();
        //    var toUpdate = new List<OrgSecGrpSetup>();

        //    foreach (var dto in dtos)
        //    {
        //        var match = existing.FirstOrDefault(x =>
        //            x.OrgSecGrpCd == dto.OrgSecGrpCd &&
        //            x.ModuleCd == dto.ModuleCd &&
        //            x.CompanyId == dto.CompanyId);

        //        if (match != null)
        //        {
        //            // UPDATE
        //            match.OrgSecProfCd = dto.OrgSecProfCd;
        //            match.ModifiedBy = "system";
        //            match.TimeStamp = DateTime.UtcNow;

        //            toUpdate.Add(match);
        //        }
        //        else
        //        {
        //            // INSERT
        //            toInsert.Add(new OrgSecGrpSetup
        //            {
        //                OrgSecGrpCd = dto.OrgSecGrpCd,
        //                ModuleCd = dto.ModuleCd,
        //                CompanyId = dto.CompanyId,
        //                OrgSecProfCd = dto.OrgSecProfCd,
        //                ModifiedBy = "system",
        //                TimeStamp = DateTime.UtcNow
        //            });
        //        }
        //    }

        //    // Perform DB operations
        //    if (toInsert.Any())
        //        await _context.OrgSecGrpSetups.AddRangeAsync(toInsert);

        //    if (toUpdate.Any())
        //        _context.OrgSecGrpSetups.UpdateRange(toUpdate);

        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        Inserted = toInsert.Count,
        //        Updated = toUpdate.Count,
        //        Total = dtos.Count
        //    });
        //}
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
