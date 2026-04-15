using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefStrucController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public RefStrucController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.RefStrucs
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{refStrucId}/{companyId}")]
        public async Task<IActionResult> Get(string refStrucId, string companyId)
        {
            var entity = await _context.RefStrucs
                .FindAsync(refStrucId, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RefStrucDto dto)
        {
            if (dto == null)
                return BadRequest();

            // 🔥 Validate flags
            if (dto.RefStrucTopFl is not ("Y" or "N"))
                return BadRequest("RefStrucTopFl must be Y/N");

            if (dto.RefDataEntryFl is not ("Y" or "N"))
                return BadRequest("RefDataEntryFl must be Y/N");

            if (string.IsNullOrWhiteSpace(dto.SRefEntryCd) || dto.SRefEntryCd.Length != 1)
                return BadRequest("SRefEntryCd must be 1 character");

            var exists = await _context.RefStrucs
                .AnyAsync(x => x.RefStrucId == dto.RefStrucId
                            && x.CompanyId == dto.CompanyId);

            if (exists)
                return Conflict("Record already exists");

            var entity = new RefStruc
            {
                RefStrucId = dto.RefStrucId,
                CompanyId = dto.CompanyId,
                RefStrucTopFl = dto.RefStrucTopFl,
                LvlNo = dto.LvlNo,
                RefStrucName = dto.RefStrucName,
                RefStrucLvlsNo = dto.RefStrucLvlsNo,
                RefDataEntryFl = dto.RefDataEntryFl,
                SRefEntryCd = dto.SRefEntryCd,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.RefStrucs.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpPut("{refStrucId}/{companyId}")]
        public async Task<IActionResult> Update(string refStrucId, string companyId, RefStrucDto dto)
        {
            var entity = await _context.RefStrucs
                .FindAsync(refStrucId, companyId);

            if (entity == null)
                return NotFound();

            // 🔥 Validate
            if (dto.RefStrucTopFl is not ("Y" or "N"))
                return BadRequest("RefStrucTopFl must be Y/N");

            if (dto.RefDataEntryFl is not ("Y" or "N"))
                return BadRequest("RefDataEntryFl must be Y/N");

            if (dto.SRefEntryCd.Length != 1)
                return BadRequest("SRefEntryCd must be 1 char");

            // Update
            entity.RefStrucTopFl = dto.RefStrucTopFl;
            entity.LvlNo = dto.LvlNo;
            entity.RefStrucName = dto.RefStrucName;
            entity.RefStrucLvlsNo = dto.RefStrucLvlsNo;
            entity.RefDataEntryFl = dto.RefDataEntryFl;
            entity.SRefEntryCd = dto.SRefEntryCd;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{refStrucId}/{companyId}")]
        public async Task<IActionResult> Delete(string refStrucId, string companyId)
        {
            var entity = await _context.RefStrucs
                .FindAsync(refStrucId, companyId);

            if (entity == null)
                return NotFound();

            _context.RefStrucs.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpPost("sync-ref-struc")]
        public async Task<IActionResult> SyncRefStruc([FromBody] List<RefStrucDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Input list cannot be empty.");

            // ✅ Validate records
            var invalid = input.Where(x =>
                string.IsNullOrWhiteSpace(x.RefStrucId) ||
                string.IsNullOrWhiteSpace(x.CompanyId) ||
                x.RefStrucTopFl is not ("Y" or "N") ||
                x.RefDataEntryFl is not ("Y" or "N") ||
                string.IsNullOrWhiteSpace(x.SRefEntryCd) || x.SRefEntryCd.Length != 1
            ).ToList();

            if (invalid.Any())
                return BadRequest("Invalid records found (check keys / flags / codes).");

            // ✅ Remove duplicates
            input = input
                .GroupBy(x => new { x.RefStrucId, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // ✅ Fetch only relevant existing records
                var companyIds = input.Select(x => x.CompanyId).Distinct().ToList();

                var existing = await _context.RefStrucs
                    .Where(x => companyIds.Contains(x.CompanyId))
                    .ToListAsync();

                // ✅ Dictionary for fast lookup
                var existingDict = existing.ToDictionary(
                    x => $"{x.RefStrucId}|{x.CompanyId}",
                    x => x
                );

                int inserted = 0;
                int updated = 0;

                // ========================
                // ✅ UPSERT (Insert + Update)
                // ========================
                foreach (var dto in input)
                {
                    var key = $"{dto.RefStrucId}|{dto.CompanyId}";

                    if (existingDict.TryGetValue(key, out var db))
                    {
                        // 🔄 UPDATE
                        db.RefStrucTopFl = dto.RefStrucTopFl;
                        db.LvlNo = dto.LvlNo;
                        db.RefStrucName = dto.RefStrucName;
                        db.RefStrucLvlsNo = dto.RefStrucLvlsNo;
                        db.RefDataEntryFl = dto.RefDataEntryFl;
                        db.SRefEntryCd = dto.SRefEntryCd;
                        db.ModifiedBy = dto.ModifiedBy;
                        db.TimeStamp = today;

                        updated++;
                    }
                    else
                    {
                        // ➕ INSERT
                        var entity = new RefStruc
                        {
                            RefStrucId = dto.RefStrucId,
                            CompanyId = dto.CompanyId,
                            RefStrucTopFl = dto.RefStrucTopFl,
                            LvlNo = dto.LvlNo,
                            RefStrucName = dto.RefStrucName,
                            RefStrucLvlsNo = dto.RefStrucLvlsNo,
                            RefDataEntryFl = dto.RefDataEntryFl,
                            SRefEntryCd = dto.SRefEntryCd,
                            ModifiedBy = dto.ModifiedBy,
                            TimeStamp = today
                        };

                        await _context.RefStrucs.AddAsync(entity);
                        inserted++;
                    }
                }

                // ========================
                // ❌ DELETE (Sync behavior)
                // ========================
                var keySet = input
                    .Select(x => $"{x.RefStrucId}|{x.CompanyId}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.RefStrucId}|{x.CompanyId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.RefStrucs.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "RefStruc synced successfully",
                    inserted,
                    updated,
                    deleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("sync-org-acct-ref-struc")]
        public async Task<IActionResult> SyncOrgAcctRefStruc(List<OrgAcctRefStruc> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Input cannot be empty");

            // Remove duplicates
            input = input
                .GroupBy(x => new { x.OrgId, x.AcctId, x.RefStrucId, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var companyIds = input.Select(x => x.CompanyId).Distinct().ToList();
                var refStrucIds = input.Select(x => x.RefStrucId).Distinct().ToList();

                var existing = await _context.OrgAcctRefStrucs
                    .Where(x => companyIds.Contains(x.CompanyId)
                             && refStrucIds.Contains(x.RefStrucId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.OrgId}|{x.AcctId}|{x.RefStrucId}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.OrgId}|{item.AcctId}|{item.RefStrucId}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        // UPDATE
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = today;
                        updated++;
                    }
                    else
                    {
                        // INSERT
                        item.TimeStamp = today;
                        await _context.OrgAcctRefStrucs.AddAsync(item);
                        inserted++;
                    }
                }

                // DELETE (sync behavior)
                var keySet = input
                    .Select(x => $"{x.OrgId}|{x.AcctId}|{x.RefStrucId}|{x.CompanyId}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.OrgId}|{x.AcctId}|{x.RefStrucId}|{x.CompanyId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.OrgAcctRefStrucs.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "Sync successful",
                    inserted,
                    updated,
                    deleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("org-acct-ref-struc/paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int size = 50)
        {
            var query = _context.OrgAcctRefStrucs.AsNoTracking();

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                size,
                data
            });
        }
    }
}
