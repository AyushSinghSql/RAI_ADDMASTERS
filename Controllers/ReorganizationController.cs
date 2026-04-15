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
                CompanyId = dto.CompanyId
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
    }
}
