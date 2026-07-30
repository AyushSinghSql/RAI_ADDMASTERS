using System;
using System.Linq;
using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/postal-codes")]
    public sealed class PostalCodesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public PostalCodesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? countryCode, [FromQuery] string? stateCode)
        {
            IQueryable<PostalCode> query = _context.PostalCodes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(countryCode) && !string.IsNullOrWhiteSpace(stateCode))
            {
                query = query.Where(p => p.CountryCode == countryCode && p.StateCode == stateCode);
            }

            var records = await query.OrderBy(p => p.PostalCd).ToListAsync();
            return Ok(records);
        }

        [HttpGet("{postalKey:int}")]
        public async Task<IActionResult> GetByKey(int postalKey)
        {
            var record = await _context.PostalCodes
                .FirstOrDefaultAsync(p => p.PostalKey == postalKey);
            if (record == null)
            {
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostalCode record)
        {
            if (string.IsNullOrWhiteSpace(record.CityName) || string.IsNullOrWhiteSpace(record.PostalCd))
            {
                return BadRequest("CityName and PostalCd are required.");
            }

            try
            {
                // Generate next postal_key
                var maxKey = await _context.PostalCodes.MaxAsync(p => (int?)p.PostalKey) ?? 0;
                record.PostalKey = maxKey + 1;
                record.ChangedBy = string.IsNullOrWhiteSpace(record.ChangedBy) ? "SYSTEM" : record.ChangedBy;
                record.ChangedDate = DateTime.UtcNow;

                await _context.PostalCodes.AddAsync(record);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByKey), new { postalKey = record.PostalKey }, record);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return BadRequest($"Parent State (CountryCode: '{record.CountryCode}', StateCode: '{record.StateCode}') does not exist in state table.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{postalKey:int}")]
        public async Task<IActionResult> Update(int postalKey, PostalCode record)
        {
            if (string.IsNullOrWhiteSpace(record.CityName) || string.IsNullOrWhiteSpace(record.PostalCd))
            {
                return BadRequest("CityName and PostalCd are required.");
            }

            try
            {
                var dbRecord = await _context.PostalCodes
                    .FirstOrDefaultAsync(p => p.PostalKey == postalKey);
                if (dbRecord == null)
                {
                    return NotFound($"Postal code record with key '{postalKey}' not found.");
                }

                dbRecord.CityName = record.CityName;
                dbRecord.CountryCode = record.CountryCode;
                dbRecord.StateCode = record.StateCode;
                dbRecord.PostalCd = record.PostalCd;
                dbRecord.ChangedBy = string.IsNullOrWhiteSpace(record.ChangedBy) ? "SYSTEM" : record.ChangedBy;
                dbRecord.ChangedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return BadRequest($"Parent State (CountryCode: '{record.CountryCode}', StateCode: '{record.StateCode}') does not exist in state table.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{postalKey:int}")]
        public async Task<IActionResult> Delete(int postalKey)
        {
            try
            {
                var dbRecord = await _context.PostalCodes
                    .FirstOrDefaultAsync(p => p.PostalKey == postalKey);
                if (dbRecord == null)
                {
                    return NotFound($"Postal code record with key '{postalKey}' not found.");
                }

                _context.PostalCodes.Remove(dbRecord);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
