using System;
using System.Linq;
using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public sealed class CountriesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CountriesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var countries = await _context.Countries
                .AsNoTracking()
                .OrderBy(c => c.CountryName)
                .ToListAsync();
            return Ok(countries);
        }

        [HttpGet("{countryCode}")]
        public async Task<IActionResult> GetByCode(string countryCode)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryCode == countryCode);
            if (country == null)
            {
                return NotFound();
            }
            return Ok(country);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Country country)
        {
            if (string.IsNullOrWhiteSpace(country.CountryCode) || string.IsNullOrWhiteSpace(country.CountryName))
            {
                return BadRequest("CountryCode and CountryName are required.");
            }

            try
            {
                var exists = await _context.Countries
                    .AnyAsync(c => c.CountryCode == country.CountryCode);
                if (exists)
                {
                    return Conflict($"Country with code '{country.CountryCode}' already exists.");
                }

                country.ChangedBy = string.IsNullOrWhiteSpace(country.ChangedBy) ? "SYSTEM" : country.ChangedBy;
                country.ChangedDate = DateTime.UtcNow;

                await _context.Countries.AddAsync(country);
                await _context.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetByCode), new { countryCode = country.CountryCode }, country);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{countryCode}")]
        public async Task<IActionResult> Update(string countryCode, Country country)
        {
            if (string.IsNullOrWhiteSpace(country.CountryName))
            {
                return BadRequest("CountryName is required.");
            }

            try
            {
                var dbCountry = await _context.Countries
                    .FirstOrDefaultAsync(c => c.CountryCode == countryCode);
                if (dbCountry == null)
                {
                    return NotFound($"Country with code '{countryCode}' not found.");
                }

                dbCountry.CountryName = country.CountryName;
                dbCountry.ChangedBy = string.IsNullOrWhiteSpace(country.ChangedBy) ? "SYSTEM" : country.ChangedBy;
                dbCountry.ChangedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{countryCode}")]
        public async Task<IActionResult> Delete(string countryCode)
        {
            try
            {
                var dbCountry = await _context.Countries
                    .FirstOrDefaultAsync(c => c.CountryCode == countryCode);
                if (dbCountry == null)
                {
                    return NotFound($"Country with code '{countryCode}' not found.");
                }

                _context.Countries.Remove(dbCountry);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return Conflict("Cannot delete this country because it is referenced by one or more states.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
