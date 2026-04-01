using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
using PlanningAPI.Models;
using System;
using WebApi.Entities;
namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgGroupsController : ControllerBase
    {

        private readonly MydatabaseContext _context;

        public OrgGroupsController(MydatabaseContext context)
        {
            _context = context;
        }
        // ✅ GET ALL (with joins)
        [HttpGet("GetAllOrgGroups")]
        public async Task<IActionResult> GetAllOrgGroups()
        {
            var data = await _context.OrgGroup
                       .ToListAsync();

            return Ok(data);
        }

        [HttpPost("OrgGroups")]
        public async Task<IActionResult> Create(
[FromBody] OrgGroupCreateUpdateDto dto)
        {
            // Optional: Unique Code validation
            var exists = await _context.OrgGroup
                .AnyAsync(x => x.OrgGroupCode == dto.OrgGroupCode);

            if (exists)
                return BadRequest("OrgGroupCode already exists.");

            var entity = new OrgGroup
            {
                OrgGroupCode = dto.OrgGroupCode,
                OrgGroupName = dto.OrgGroupName,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CompanyId = dto.CompanyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name
            };

            _context.OrgGroup.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = entity.OrgGroupId },
                entity);
        }

        // =====================================================
        // READ (By Id)
        // =====================================================
        [HttpGet("OrgGroups/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await _context.OrgGroup
                .AsNoTracking()
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [HttpPut("OrgGroups/{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] OrgGroupCreateUpdateDto dto)
        {
            var entity = await _context.OrgGroup
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            entity.OrgGroupCode = dto.OrgGroupCode;
            entity.OrgGroupName = dto.OrgGroupName;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = User?.Identity?.Name;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpDelete("OrgGroups/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.OrgGroup
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            // Optional: prevent delete if mappings exist
            if (entity.OrgMappings.Any() || entity.UserMappings.Any())
                return BadRequest("Cannot delete OrgGroup with active mappings.");

            _context.OrgGroup.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("OrgGroups/BulkDelete")]
        public async Task<IActionResult> BulkDelete(
        [FromBody] List<int> request)
        {
            if (request == null || !request.Any())
                return BadRequest("OrgGroupIds cannot be empty.");

            var orgGroups = await _context.OrgGroup
                .Where(x => request.Contains(x.OrgGroupId))
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .ToListAsync();

            if (!orgGroups.Any())
                return NotFound("No OrgGroups found.");

            var blockedIds = orgGroups
                .Where(x => x.OrgMappings.Any() || x.UserMappings.Any())
                .Select(x => x.OrgGroupId)
                .ToList();

            if (blockedIds.Any())
            {
                return BadRequest(new
                {
                    Message = "Some OrgGroups have active mappings and cannot be deleted.",
                    BlockedOrgGroupIds = blockedIds
                });
            }

            _context.OrgGroup.RemoveRange(orgGroups);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                DeletedCount = orgGroups.Count
            });
        }
    }
}
