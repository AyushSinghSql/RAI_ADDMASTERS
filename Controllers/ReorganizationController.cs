using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/reorganizations")]
    public class ReorganizationController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ReorganizationController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllReOrgs")]
        public async Task<IActionResult> GetAllReOrgs()
        {
            var data = await _context.Reorganizations
                .Select(x => new ReorganizationDto
                {
                    ReorgId = x.ReorgId,
                    ReorgName = x.ReorgName,
                    ReorgTopFl = x.ReorgTopFl,
                    LvlNo = x.LvlNo,
                    ReorgLvlsNo = x.ReorgLvlsNo,
                    CompanyId = x.CompanyId
                }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("GetReorgById/{id}")]
        public async Task<IActionResult> GetReorgById(string id)
        {
            var data = await _context.Reorganizations
                .Where(x => x.ReorgId == id)
                .Select(x => new ReorganizationDto
                {
                    ReorgId = x.ReorgId,
                    ReorgName = x.ReorgName,
                    CompanyId = x.CompanyId
                })
                .FirstOrDefaultAsync();

            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPost("CreateReOrg")]
        public async Task<IActionResult> CreateReOrg(CreateReorganizationDto dto)
        {
            var entity = new Reorganization
            {
                ReorgId = dto.ReorgId,
                ReorgName = dto.ReorgName,
                CompanyId = dto.CompanyId,
                LvlNo = dto.LvlNo,
                ReorgTopFl = dto.ReorgTopFl
            };

            _context.Reorganizations.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("DeleteReOrg/{id}")]
        public async Task<IActionResult> DeleteReOrg(string id)
        {
            var entity = await _context.Reorganizations.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("GetByReorg/{reorgId}")]
        public async Task<IActionResult> GetByReorg(string reorgId)
        {
            var data = await _context.ReorganizationLevels
                .Where(x => x.ReorgIdTop == reorgId)
                .Select(x => new ReorganizationLevelDto
                {
                    ReorgLvlKey = x.ReorgLvlKey,
                    ReorgIdTop = x.ReorgIdTop,
                    LvlNo = x.LvlNo,
                    IdSegLenNo = x.IdSegLenNo,
                    ReorgLvlDesc = x.ReorgLvlDesc,
                    CompanyId = x.CompanyId
                }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("CreateReOrgLevel")]
        public async Task<IActionResult> CreateReOrgLevel(CreateReorganizationLevelDto dto)
        {
            var entity = new ReorganizationLevel
            {
                ReorgIdTop = dto.ReorgIdTop,
                LvlNo = dto.LvlNo,
                IdSegLenNo = dto.IdSegLenNo,
                ReorgLvlDesc = dto.ReorgLvlDesc,
                CompanyId = dto.CompanyId
            };

            _context.ReorganizationLevels.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet("GetReOrgLevels")]
        public async Task<IActionResult> GetReOrgLevels(string companyId, string? reorgIdTop = null)
        {
            if (string.IsNullOrEmpty(companyId))
                return BadRequest("CompanyId is required");

            var query = _context.ReorganizationLevels
                .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(reorgIdTop))
                query = query.Where(x => x.ReorgIdTop == reorgIdTop);

            var data = await query
                .OrderBy(x => x.ReorgIdTop)
                .ThenBy(x => x.LvlNo)
                .Select(x => new
                {
                    x.ReorgIdTop,
                    x.LvlNo,
                    x.IdSegLenNo,
                    x.ReorgLvlDesc,
                    x.CompanyId
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("CreateMapping")]
        public async Task<IActionResult> CreateMapping(ReorganizationOrgMapDto dto)
        {
            var exists = await _context.ReorganizationOrgMaps
                .AnyAsync(x => x.ReorgId == dto.ReorgId
                            && x.OrgId == dto.OrgId
                            && x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Mapping already exists");

            var entity = new ReorganizationOrgMap
            {
                ReorgId = dto.ReorgId,
                OrgId = dto.OrgId,
                CompanyId = dto.CompanyId
            };

            _context.ReorganizationOrgMaps.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpPut("UpdateReOrgLevel")]
        public async Task<IActionResult> UpdateReOrgLevel(CreateReorganizationLevelDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var entity = await _context.ReorganizationLevels
                .FirstOrDefaultAsync(x =>
                    x.ReorgIdTop == dto.ReorgIdTop &&
                    x.LvlNo == dto.LvlNo &&
                    x.CompanyId == dto.CompanyId);

            if (entity == null)
                return NotFound("Record not found");

            // Update only non-key fields
            entity.IdSegLenNo = dto.IdSegLenNo;
            entity.ReorgLvlDesc = dto.ReorgLvlDesc;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("DeleteReOrgLevel")]
        public async Task<IActionResult> DeleteReOrgLevel(string reorgIdTop, int lvlNo, string companyId)
        {
            if (string.IsNullOrEmpty(reorgIdTop) || string.IsNullOrEmpty(companyId))
                return BadRequest("Invalid key data");

            var entity = await _context.ReorganizationLevels
                .FirstOrDefaultAsync(x =>
                    x.ReorgIdTop == reorgIdTop &&
                    x.LvlNo == lvlNo &&
                    x.CompanyId == companyId);

            if (entity == null)
                return NotFound("Record not found");

            _context.ReorganizationLevels.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deleted successfully"
            });
        }

        [HttpPost("BulkCreateMapping")]
        public async Task<IActionResult> BulkCreateMapping(List<ReorganizationOrgMapDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided");

            // Get unique keys from input
            var keys = dtos
                .Select(x => new { x.ReorgId, x.OrgId, x.CompanyId })
                .Distinct()
                .ToList();

            // Fetch existing records in ONE query
            var existing = await _context.ReorganizationOrgMaps
                .Where(x => keys.Select(k => k.ReorgId).Contains(x.ReorgId)
                         && keys.Select(k => k.OrgId).Contains(x.OrgId)
                         && keys.Select(k => k.CompanyId).Contains(x.CompanyId))
                .Select(x => new { x.ReorgId, x.OrgId, x.CompanyId })
                .ToListAsync();

            var existingSet = new HashSet<string>(
                existing.Select(x => $"{x.ReorgId}|{x.OrgId}|{x.CompanyId}")
            );

            // Filter new records
            var newEntities = dtos
                .Where(x => !existingSet.Contains($"{x.ReorgId}|{x.OrgId}|{x.CompanyId}"))
                .Select(x => new ReorganizationOrgMap
                {
                    ReorgId = x.ReorgId,
                    OrgId = x.OrgId,
                    CompanyId = x.CompanyId
                })
                .ToList();

            if (!newEntities.Any())
                return Ok("No new records to insert");

            await _context.ReorganizationOrgMaps.AddRangeAsync(newEntities);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Inserted = newEntities.Count,
                Skipped = dtos.Count - newEntities.Count
            });
        }
        [HttpPost("BulkDeleteMapping")]
        public async Task<IActionResult> BulkDeleteMapping(List<ReorganizationOrgMapDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided");

            var keys = dtos
                .Select(x => new { x.ReorgId, x.OrgId, x.CompanyId })
                .Distinct()
                .ToList();

            // Fetch matching records
            var recordsToDelete = await _context.ReorganizationOrgMaps
                .Where(x => keys.Select(k => k.ReorgId).Contains(x.ReorgId)
                         && keys.Select(k => k.OrgId).Contains(x.OrgId)
                         && keys.Select(k => k.CompanyId).Contains(x.CompanyId))
                .ToListAsync();

            if (!recordsToDelete.Any())
                return Ok("No matching records found");

            _context.ReorganizationOrgMaps.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Deleted = recordsToDelete.Count
            });
        }


        [HttpGet("GetMappingByReorg/{reorgId}")]
        public async Task<IActionResult> GetMappingByReorg(string reorgId)
        {
            var data = await _context.ReorganizationOrgMaps
                .Where(x => x.ReorgId == reorgId)
                .Select(x => new ReorganizationOrgMapDto
                {
                    ReorgId = x.ReorgId,
                    OrgId = x.OrgId,
                    CompanyId = x.CompanyId
                }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("GetAllMappingByReorg")]
        public async Task<IActionResult> GetAllMappingByReorg()
        {
            var data = await _context.ReorganizationOrgMaps
                .Select(x => new ReorganizationOrgMapDto
                {
                    ReorgId = x.ReorgId,
                    OrgId = x.OrgId,
                    CompanyId = x.CompanyId
                }).ToListAsync();

            return Ok(data);
        }
    }
}
