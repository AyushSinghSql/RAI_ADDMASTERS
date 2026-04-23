using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingDetlJobTitlesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public TrainingDetlJobTitlesController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? trainId,
            string? jobCd,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.TrainingDetlJobTitles.AsQueryable();

            if (!string.IsNullOrEmpty(trainId))
                query = query.Where(x => x.TrainId == trainId);

            if (!string.IsNullOrEmpty(jobCd))
                query = query.Where(x => x.DetlJobCd.Contains(jobCd));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.TrainId)
                .ThenBy(x => x.DetlJobCd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{trainId}/{jobCd}")]
        public async Task<IActionResult> Get(string trainId, string jobCd)
        {
            var data = await _context.TrainingDetlJobTitles
                .FindAsync(trainId, jobCd);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(TrainingDetlJobTitle model)
        {
            var exists = await _context.TrainingDetlJobTitles
                .AnyAsync(x => x.TrainId == model.TrainId && x.DetlJobCd == model.DetlJobCd);

            if (exists)
                return BadRequest("Mapping already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.TrainingDetlJobTitles.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{trainId}/{jobCd}")]
        public async Task<IActionResult> Update(string trainId, string jobCd, TrainingDetlJobTitle model)
        {
            var db = await _context.TrainingDetlJobTitles
                .FindAsync(trainId, jobCd);

            if (db == null)
                return NotFound();

            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;
            db.RowVersion = model.RowVersion;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE
        [HttpDelete("{trainId}/{jobCd}")]
        public async Task<IActionResult> Delete(string trainId, string jobCd)
        {
            var db = await _context.TrainingDetlJobTitles
                .FindAsync(trainId, jobCd);

            if (db == null)
                return NotFound();

            _context.TrainingDetlJobTitles.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ BULK UPSERT
        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<TrainingDetlJobTitle> input)
        {
            if (input == null || !input.Any())
                return BadRequest("No data provided");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var keys = input
                    .Select(x => $"{x.TrainId}|{x.DetlJobCd}")
                    .ToHashSet();

                var existing = await _context.TrainingDetlJobTitles.ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.TrainId}|{x.DetlJobCd}");

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.TrainId}|{item.DetlJobCd}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        db.ModifiedBy = item.ModifiedBy;
                        db.TimeStamp = DateTime.UtcNow;
                        db.RowVersion = item.RowVersion;
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateTime.UtcNow;
                        await _context.TrainingDetlJobTitles.AddAsync(item);
                        inserted++;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new
                {
                    message = "Bulk upsert successful",
                    inserted,
                    updated
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
