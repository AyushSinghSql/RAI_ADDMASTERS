using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendActionController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendActionController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendActionDto dto)
        {
            if (dto == null)
                return BadRequest();

            // ✅ Validate flags
            if (dto.VendorAddressContactFlag?.Length > 1 ||
                dto.VendorEmployeeFlag?.Length > 1 ||
                dto.VendorLaborInfoFlag?.Length > 1)
                return BadRequest("Flags must be single character");

            var exists = await _context.VendActions
                .AnyAsync(x => x.VendId == dto.VendId && x.ActionKey == dto.ActionKey);

            if (exists)
                return Conflict("Record already exists");

            var entity = new VendAction
            {
                VendId = dto.VendId,
                ActionKey = dto.ActionKey,
                PortalActionCode = dto.PortalActionCode,
                VendorAddressContactFlag = dto.VendorAddressContactFlag,
                VendorEmployeeFlag = dto.VendorEmployeeFlag,
                VendorLaborInfoFlag = dto.VendorLaborInfoFlag,
                ModifiedBy = dto.ModifiedBy,
                ActionNotes = dto.ActionNotes,
                TimeStamp = DateTime.UtcNow
            };

            _context.VendActions.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VendActions
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("{vendId}/{actionKey}")]
        public async Task<IActionResult> Get(string vendId, decimal actionKey)
        {
            var entity = await _context.VendActions
                .FindAsync(vendId, actionKey);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }
        [HttpPut("{vendId}/{actionKey}")]
        public async Task<IActionResult> Update(string vendId, decimal actionKey, VendActionDto dto)
        {
            var entity = await _context.VendActions
                .FindAsync(vendId, actionKey);

            if (entity == null)
                return NotFound();

            entity.PortalActionCode = dto.PortalActionCode;
            entity.VendorAddressContactFlag = dto.VendorAddressContactFlag;
            entity.VendorEmployeeFlag = dto.VendorEmployeeFlag;
            entity.VendorLaborInfoFlag = dto.VendorLaborInfoFlag;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.ActionNotes = dto.ActionNotes;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{vendId}/{actionKey}")]
        public async Task<IActionResult> Delete(string vendId, decimal actionKey)
        {
            var entity = await _context.VendActions
                .FindAsync(vendId, actionKey);

            if (entity == null)
                return NotFound();

            _context.VendActions.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync(List<VendActionDto> input)
        {
            if (input == null || !input.Any())
                return BadRequest();

            var existing = await _context.VendActions.ToListAsync();

            var dict = existing.ToDictionary(x => $"{x.VendId}|{x.ActionKey}");

            int insert = 0, update = 0;

            foreach (var dto in input)
            {
                var key = $"{dto.VendId}|{dto.ActionKey}";

                if (dict.TryGetValue(key, out var db))
                {
                    db.ModifiedBy = dto.ModifiedBy;
                    db.ActionNotes = dto.ActionNotes;
                    db.TimeStamp = DateTime.UtcNow;
                    update++;
                }
                else
                {
                    _context.VendActions.Add(new VendAction
                    {
                        VendId = dto.VendId,
                        ActionKey = dto.ActionKey,
                        ModifiedBy = dto.ModifiedBy,
                        TimeStamp = DateTime.UtcNow
                    });
                    insert++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { insert, update });
        }
    }
}
