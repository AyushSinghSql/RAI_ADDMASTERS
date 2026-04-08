using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/org-level")]
    public class OrgLevelController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public OrgLevelController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.OrgLevels
                .OrderBy(x => x.OrgIdTop)
                .ThenBy(x => x.LevelNo)
                .ToListAsync();



            //    return result;

            return Ok(data);
        }

        // ✅ GET BY ORG
        [HttpGet("{orgIdTop}")]
        public async Task<IActionResult> GetByOrg(string orgIdTop)
        {
            var data = await _context.OrgLevels
                .Where(x => x.OrgIdTop == orgIdTop)
                .OrderBy(x => x.LevelNo)
                .ToListAsync();

            var result = await (
                from lvl in _context.OrgLevels
                join org in _context.Organizations
                    on lvl.LevelNo equals org.LvlNo into OrgGroup
                where lvl.OrgIdTop.StartsWith(orgIdTop)
                select new LevelDto
                {
                    Level = lvl.LevelNo,
                    Lenght = lvl.IdSegmentLength,
                    Count = OrgGroup.Count(),
                    Description = lvl.OrgLevelDesc
                }
            ).OrderBy(x => x.Level).ToListAsync();

            return Ok(result);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(OrgLevelDto dto)
        {
            // 🔹 Get existing records for this Org
            var existing = await _context.OrgLevels
                .Where(x => x.OrgIdTop == dto.OrgIdTop)
                .ToListAsync();

            // 🔴 Duplicate check
            if (existing.Any(x => x.OrgLevelKey == dto.OrgLevelKey))
                return BadRequest($"OrgLevelKey {dto.OrgLevelKey} already exists");

            if (existing.Any(x => x.LevelNo == dto.LevelNo))
                return BadRequest($"LevelNo {dto.LevelNo} already exists");

            // 🔥 Get expected next sequence
            var maxLevelKey = existing.Any() ? existing.Max(x => x.OrgLevelKey) : 0;
            var maxLevelNo = existing.Any() ? existing.Max(x => x.LevelNo) : 0;

            var expectedLevelKey = maxLevelKey + 1;
            var expectedLevelNo = maxLevelNo + 1;

            // 🔴 Validate sequence
            if (dto.OrgLevelKey != expectedLevelKey)
                return BadRequest($"Invalid OrgLevelKey. Expected: {expectedLevelKey}");

            if (dto.LevelNo != expectedLevelNo)
                return BadRequest($"Invalid LevelNo. Expected: {expectedLevelNo}");

            // ✅ Create
            var entity = new OrgLevel
            {
                OrgIdTop = dto.OrgIdTop,
                OrgLevelKey = dto.OrgLevelKey,
                LevelNo = dto.LevelNo,
                IdSegmentLength = dto.IdSegmentLength,
                OrgLevelDesc = dto.OrgLevelDesc,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow,
                Rowversion = dto.Rowversion
            };

            _context.OrgLevels.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }
        // ✅ UPDATE
        [HttpPut("{orgIdTop}/{orgLevelKey}")]
        public async Task<IActionResult> Update(string orgIdTop, decimal orgLevelKey, OrgLevelDto dto)
        {
            var entity = await _context.OrgLevels
                .FindAsync(orgIdTop, orgLevelKey);

            if (entity == null)
                return NotFound();

            //entity.LevelNo = dto.LevelNo;
            entity.IdSegmentLength = dto.IdSegmentLength;
            entity.OrgLevelDesc = dto.OrgLevelDesc;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.Rowversion = dto.Rowversion;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{orgIdTop}/{orgLevelKey}")]
        public async Task<IActionResult> Delete(string orgId, decimal orgLevelKey)
        {
            var entity = await _context.OrgLevels
                .FindAsync(orgId, orgLevelKey);

            if (entity == null)
                return NotFound();

            // 🔥 Get max sequence
            var maxLevel = await _context.OrgLevels
                .Where(x => x.OrgIdTop == orgId)
                .MaxAsync(x => (decimal?)x.OrgLevelKey) ?? 0;

            // ❌ Only allow deleting last level
            if (orgLevelKey != maxLevel)
                return BadRequest($"Only last level ({maxLevel}) can be deleted to maintain sequence.");

            // 🔥 Check usage (example tables)
            var isUsed = await _context.OrgAccounts.AnyAsync(x =>
                x.OrgId == orgId);

            if (isUsed)
                return BadRequest("Cannot delete. Level is used in transactions.");


            // ✅ Delete
            _context.OrgLevels.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

    }
}
