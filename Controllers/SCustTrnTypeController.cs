using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SCustTrnTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SCustTrnTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SCustTrnTypes
                .Select(x => new SCustTrnTypeDto
                {
                    SCustTrnTypeCode = x.SCustTrnTypeCode,
                    CustTrnTypeDesc = x.Description
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY ID
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var entity = await _context.SCustTrnTypes.FindAsync(code);

            if (entity == null)
                return NotFound($"Transaction Type '{code}' not found");

            return Ok(new SCustTrnTypeDto
            {
                SCustTrnTypeCode = entity.SCustTrnTypeCode,
                CustTrnTypeDesc = entity.Description
            });
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateSCustTrnTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Duplicate check
            var exists = await _context.SCustTrnTypes
                .AnyAsync(x => x.SCustTrnTypeCode == dto.SCustTrnTypeCode);

            if (exists)
                return Conflict($"Code '{dto.SCustTrnTypeCode}' already exists");

            var entity = new SCustTrnType
            {
                SCustTrnTypeCode = dto.SCustTrnTypeCode,
                Description = dto.CustTrnTypeDesc,
                ModifiedBy = "SYSTEM",
                TimeStamp = DateTime.UtcNow
            };

            _context.SCustTrnTypes.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { code = entity.SCustTrnTypeCode }, dto);
        }

        // ✅ UPDATE
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, UpdateSCustTrnTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _context.SCustTrnTypes.FindAsync(code);

            if (entity == null)
                return NotFound($"Transaction Type '{code}' not found");

            entity.Description = dto.CustTrnTypeDesc;
            entity.ModifiedBy = "SYSTEM";
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Updated successfully");
        }

        // ✅ DELETE
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var entity = await _context.SCustTrnTypes.FindAsync(code);

            if (entity == null)
                return NotFound($"Transaction Type '{code}' not found");

            _context.SCustTrnTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN API
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.SCustTrnTypes
                .Select(x => new
                {
                    value = x.SCustTrnTypeCode,
                    label = x.Description
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
