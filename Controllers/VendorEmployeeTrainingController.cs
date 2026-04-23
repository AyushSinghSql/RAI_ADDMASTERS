using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-employee-trainings")]
    [ApiController]
    public class VendorEmployeeTrainingController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeTrainingController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? vendEmplId,
            string? trainId,
            string? companyId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.VendorEmployeeTrainings.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(vendEmplId))
                query = query.Where(x => x.VendEmplId == vendEmplId);

            if (!string.IsNullOrEmpty(trainId))
                query = query.Where(x => x.TrainId == trainId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.StartDt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY KEY
        [HttpGet("{vendEmplId}/{vendId}/{trainId}/{companyId}")]
        public async Task<IActionResult> Get(
            string vendEmplId,
            string vendId,
            string trainId,
            string companyId)
        {
            var entity = await _context.VendorEmployeeTrainings.FindAsync(
                vendEmplId, vendId, trainId, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(VendorEmployeeTraining dto)
        {
            // 🔥 Validation
            if (dto.SIntExtCd != "I" && dto.SIntExtCd != "E")
                return BadRequest("SIntExtCd must be 'I' or 'E'");

            if (dto.EndDt != null && dto.StartDt != null && dto.EndDt < dto.StartDt)
                return BadRequest("End date cannot be before start date");

            dto.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.VendorEmployeeTrainings.AddAsync(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE
        [HttpPut("{vendEmplId}/{vendId}/{trainId}/{companyId}")]
        public async Task<IActionResult> Update(
            string vendEmplId,
            string vendId,
            string trainId,
            string companyId,
            VendorEmployeeTraining dto)
        {
            var entity = await _context.VendorEmployeeTrainings.FindAsync(
                vendEmplId, vendId, trainId, companyId);

            if (entity == null)
                return NotFound();

            if (dto.SIntExtCd != "I" && dto.SIntExtCd != "E")
                return BadRequest("SIntExtCd must be 'I' or 'E'");

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{trainId}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            string trainId,
            string companyId)
        {
            var entity = await _context.VendorEmployeeTrainings.FindAsync(
                vendEmplId, vendId, trainId, companyId);

            if (entity == null)
                return NotFound();

            _context.VendorEmployeeTrainings.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // ✅ BULK UPSERT (SYNC)
        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendorEmployeeTraining> input)
        {
            if (input == null || !input.Any())
                return BadRequest("Empty input");

            input = input
                .GroupBy(x => new { x.VendEmplId, x.VendId, x.TrainId, x.CompanyId })
                .Select(g => g.First())
                .ToList();

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.VendorEmployeeTrainings.ToListAsync();

                var dict = existing.ToDictionary(
                    x => $"{x.VendEmplId}|{x.VendId}|{x.TrainId}|{x.CompanyId}"
                );

                int inserted = 0, updated = 0;

                foreach (var item in input)
                {
                    var key = $"{item.VendEmplId}|{item.VendId}|{item.TrainId}|{item.CompanyId}";

                    if (dict.TryGetValue(key, out var db))
                    {
                        _context.Entry(db).CurrentValues.SetValues(item);
                        db.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        updated++;
                    }
                    else
                    {
                        item.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);
                        await _context.VendorEmployeeTrainings.AddAsync(item);
                        inserted++;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { inserted, updated });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive(string vendId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var data = await _context.VendorEmployeeTrainings
                .Where(x => x.VendId == vendId &&
                       (x.ExpiryDt == null || x.ExpiryDt >= today))
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiring(int days = 30)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var future = today.AddDays(days);

            var data = await _context.VendorEmployeeTrainings
                .Where(x => x.ExpiryDt >= today && x.ExpiryDt <= future)
                .ToListAsync();

            return Ok(data);
        }
    }
}
