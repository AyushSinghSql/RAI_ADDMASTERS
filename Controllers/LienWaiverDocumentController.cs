using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LienWaiverDocumentController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public LienWaiverDocumentController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(LienWaiverDocument dto)
        {
            // Required
            if (string.IsNullOrWhiteSpace(dto.DocumentCode) ||
                string.IsNullOrWhiteSpace(dto.DocumentName))
                return BadRequest("Document Value & Name required");

            // Flag validation
            var validFlags = new[] { "Y", "N" };

            if (!validFlags.Contains(dto.ApSuppDetailFlag) ||
                !validFlags.Contains(dto.ApAllDetailFlag) ||
                !validFlags.Contains(dto.ArSuppDetailFlag) ||
                !validFlags.Contains(dto.ArAllDetailFlag))
                return BadRequest("Flags must be Y/N");

            // Duplicate
            var exists = await _context.LienWaiverDocuments
                .AnyAsync(x => x.DocumentCode == dto.DocumentCode);

            if (exists)
                return BadRequest("Duplicate Document Value");

            _context.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.LienWaiverDocuments.ToListAsync());
        }
        [HttpGet("{code}")]
        public async Task<IActionResult> GetById(string code)
        {
            var data = await _context.LienWaiverDocuments.FindAsync(code);
            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Update(LienWaiverDocument dto)
        {
            var entity = await _context.LienWaiverDocuments
                .FindAsync(dto.DocumentCode);

            if (entity == null)
                return NotFound();

            entity.DocumentName = dto.DocumentName;
            entity.DocumentDescription = dto.DocumentDescription;
            entity.ApSuppDetailFlag = dto.ApSuppDetailFlag;
            entity.ApAllDetailFlag = dto.ApAllDetailFlag;
            entity.ArSuppDetailFlag = dto.ArSuppDetailFlag;
            entity.ArAllDetailFlag = dto.ArAllDetailFlag;
            entity.DocumentDetailName = dto.DocumentDetailName;
            entity.ModifiedBy = dto.ModifiedBy;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var entity = await _context.LienWaiverDocuments.FindAsync(code);
            if (entity == null) return NotFound();

///*            // Example usage check
//            var isUsed = await _context.SubcontractorLiens
//                .AnyAsync(x => x.LienKey == code);

//            if (isUsed)
//                return BadRequest("Document is in use");*/

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.LienWaiverDocuments
                .Select(x => new {
                    value = x.DocumentCode,
                    label = x.DocumentName
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
