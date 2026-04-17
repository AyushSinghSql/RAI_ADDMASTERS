using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-expense-accounts")]
    [ApiController]
    public class VendorExpenseAccountsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorExpenseAccountsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (with filtering + pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? projId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorExpenseAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(projId))
                query = query.Where(x => x.ProjId == projId);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.LnNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendId}/{key}")]
        public async Task<IActionResult> Get(string vendId, decimal key)
        {
            var entity = await _context.VendorExpenseAccounts
                .FindAsync(vendId, key);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorExpenseAccount dto)
        {
            dto.ModifiedTs = DateTime.UtcNow;

            await _context.VendorExpenseAccounts.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendId}/{key}")]
        public async Task<IActionResult> Update(
            string vendId,
            decimal key,
            VendorExpenseAccount dto)
        {
            var entity = await _context.VendorExpenseAccounts
                .FindAsync(vendId, key);

            if (entity == null)
                return NotFound();

            entity.LnNo = dto.LnNo;
            entity.AcctId = dto.AcctId;
            entity.OrgId = dto.OrgId;
            entity.ProjId = dto.ProjId;
            entity.Ref1Id = dto.Ref1Id;
            entity.Ref2Id = dto.Ref2Id;
            entity.PctOfTotalRt = dto.PctOfTotalRt;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendId}/{key}")]
        public async Task<IActionResult> Delete(string vendId, decimal key)
        {
            var entity = await _context.VendorExpenseAccounts
                .FindAsync(vendId, key);

            if (entity == null)
                return NotFound();

            _context.VendorExpenseAccounts.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorExpenseAccount> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendId, x.VendExpLnKey })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var vendIds = input.Select(x => x.VendId).Distinct().ToList();

                var existing = await _context.VendorExpenseAccounts
                    .Where(x => vendIds.Contains(x.VendId))
                    .ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendId}|{x.VendExpLnKey}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendId}|{item.VendExpLnKey}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.LnNo = item.LnNo;
                        db.AcctId = item.AcctId;
                        db.OrgId = item.OrgId;
                        db.ProjId = item.ProjId;
                        db.PctOfTotalRt = item.PctOfTotalRt;
                        db.ModifiedBy = item.ModifiedBy;
                        db.ModifiedTs = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        item.ModifiedTs = DateTime.UtcNow;
                        await _context.VendorExpenseAccounts.AddAsync(item);
                        inserted++;
                    }
                }

                // DELETE (sync behavior)
                var keySet = input
                    .Select(x => $"{x.VendId}|{x.VendExpLnKey}")
                    .ToHashSet();

                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.VendId}|{x.VendExpLnKey}"))
                    .ToList();

                if (toDelete.Any())
                    _context.VendorExpenseAccounts.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
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
    }
}
