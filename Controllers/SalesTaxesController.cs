using System;
using System.Linq;
using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/sales-taxes")]
    public sealed class SalesTaxesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SalesTaxesController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var taxes = await _context.SalesTaxes
                    .Include(t => t.Accounts)
                    .AsNoTracking()
                    .ToListAsync();

                var states = await _context.States.AsNoTracking().ToDictionaryAsync(s => (s.CountryCode, s.StateCode), s => s.StateName);
                var countries = await _context.Countries.AsNoTracking().ToDictionaryAsync(c => c.CountryCode, c => c.CountryName);

                foreach (var tax in taxes)
                {
                    if (countries.TryGetValue(tax.Country, out var countryName))
                        tax.CountryName = countryName;
                    if (states.TryGetValue((tax.Country, tax.StateProvince), out var stateName))
                        tax.StateName = stateName;
                }

                return Ok(taxes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpGet("{taxCode}")]
        public async Task<IActionResult> GetByCode(string taxCode)
        {
            return await GetByKey("1", taxCode);
        }

        [HttpGet("{companyId}/{taxCode}")]
        public async Task<IActionResult> GetByKey(string companyId, string taxCode)
        {
            try
            {
                var tax = await _context.SalesTaxes
                    .Include(t => t.Accounts)
                    .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.TaxCode == taxCode);

                if (tax is null)
                {
                    return NotFound();
                }

                var country = await _context.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.CountryCode == tax.Country);
                if (country != null)
                    tax.CountryName = country.CountryName;
                    
                var state = await _context.States.AsNoTracking().FirstOrDefaultAsync(s => s.CountryCode == tax.Country && s.StateCode == tax.StateProvince);
                if (state != null)
                    tax.StateName = state.StateName;

                return Ok(tax);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(SalesTax tax)
        {
            if (string.IsNullOrWhiteSpace(tax.TaxCode))
            {
                return BadRequest("TaxCode is required.");
            }

            try
            {
                var exists = await _context.SalesTaxes
                    .AnyAsync(t => t.CompanyId == tax.CompanyId && t.TaxCode == tax.TaxCode);
                if (exists)
                {
                    return Conflict($"Sales tax with code '{tax.TaxCode}' already exists for company '{tax.CompanyId}'.");
                }

                tax.CompanyId = string.IsNullOrWhiteSpace(tax.CompanyId) ? "1" : tax.CompanyId;
                tax.ModifiedBy = string.IsNullOrWhiteSpace(tax.ModifiedBy) ? "SYSTEM" : tax.ModifiedBy;
                tax.TimeStamp = DateTime.UtcNow;

                if (tax.Accounts != null && tax.Accounts.Count > 0)
                {
                    var maxKey = await _context.SalesTaxAccounts.MaxAsync(a => (int?)a.AccountKey) ?? 0;
                    foreach (var acc in tax.Accounts)
                    {
                        acc.AccountKey = ++maxKey;
                        acc.CompanyId = string.IsNullOrWhiteSpace(acc.CompanyId) ? tax.CompanyId : acc.CompanyId;
                        acc.TaxCode = tax.TaxCode;
                        acc.ChangedBy = string.IsNullOrWhiteSpace(acc.ChangedBy) ? "SYSTEM" : acc.ChangedBy;
                        acc.ChangedDate = DateTime.UtcNow;
                    }
                }

                await _context.SalesTaxes.AddAsync(tax);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByKey), new { companyId = tax.CompanyId, taxCode = tax.TaxCode }, tax);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{companyId}/{taxCode}")]
        public async Task<IActionResult> Update(string companyId, string taxCode, SalesTax tax)
        {
            try
            {
                var dbTax = await _context.SalesTaxes
                    .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.TaxCode == taxCode);
                if (dbTax == null)
                {
                    return NotFound();
                }

                dbTax.CertificateNo = tax.CertificateNo ?? string.Empty;
                dbTax.Exempt = tax.Exempt;
                dbTax.Description = tax.Description ?? string.Empty;
                dbTax.StateProvince = tax.StateProvince ?? string.Empty;
                dbTax.Country = tax.Country ?? string.Empty;
                dbTax.ModifiedBy = string.IsNullOrWhiteSpace(tax.ModifiedBy) ? "SYSTEM" : tax.ModifiedBy;
                dbTax.TimeStamp = DateTime.UtcNow;
                dbTax.CompositeTaxRate = tax.CompositeTaxRate;
                dbTax.RecoveryPercent = tax.RecoveryPercent;
                dbTax.RequiresVatInfo = tax.RequiresVatInfo;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{companyId}/{taxCode}")]
        public async Task<IActionResult> Delete(string companyId, string taxCode)
        {
            try
            {
                var dbTax = await _context.SalesTaxes
                    .Include(t => t.Accounts)
                    .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.TaxCode == taxCode);
                if (dbTax == null)
                {
                    return NotFound();
                }

                _context.SalesTaxes.Remove(dbTax);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return Conflict("Cannot delete this sales tax because it is referenced in another table.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
