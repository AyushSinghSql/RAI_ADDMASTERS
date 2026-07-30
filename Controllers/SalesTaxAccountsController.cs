using System;
using System.Linq;
using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/sales-tax-accounts")]
    public sealed class SalesTaxAccountsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SalesTaxAccountsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _context.SalesTaxAccounts
                    .AsNoTracking()
                    .OrderBy(a => a.AccountKey)
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpGet("{companyId}/{taxCode}/{accountKey}")]
        public async Task<IActionResult> GetByKey(string companyId, string taxCode, int accountKey)
        {
            try
            {
                var result = await _context.SalesTaxAccounts
                    .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.TaxCode == taxCode && a.AccountKey == accountKey);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(SalesTaxAccount account)
        {
            if (string.IsNullOrWhiteSpace(account.TaxCode))
            {
                return BadRequest("TaxCode is required.");
            }

            try
            {
                if (account.AccountKey <= 0)
                {
                    var maxKey = await _context.SalesTaxAccounts.MaxAsync(a => (int?)a.AccountKey) ?? 0;
                    account.AccountKey = maxKey + 1;
                }
                
                account.CompanyId = string.IsNullOrWhiteSpace(account.CompanyId) ? "1" : account.CompanyId;
                account.ChangedBy = string.IsNullOrWhiteSpace(account.ChangedBy) ? "SYSTEM" : account.ChangedBy;
                account.ChangedDate = DateTime.UtcNow;

                await _context.SalesTaxAccounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByKey), new { companyId = account.CompanyId, taxCode = account.TaxCode, accountKey = account.AccountKey }, account);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return BadRequest("Parent Sales Tax or Account/Organization references are invalid.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("{companyId}/{taxCode}/{accountKey}")]
        public async Task<IActionResult> Update(string companyId, string taxCode, int accountKey, SalesTaxAccount account)
        {
            try
            {
                var dbAccount = await _context.SalesTaxAccounts
                    .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.TaxCode == taxCode && a.AccountKey == accountKey);
                if (dbAccount is null)
                {
                    return NotFound();
                }

                dbAccount.Account = account.Account ?? string.Empty;
                dbAccount.Organization = account.Organization ?? string.Empty;
                dbAccount.TaxRate = account.TaxRate;
                dbAccount.ChangedBy = string.IsNullOrWhiteSpace(account.ChangedBy) ? "SYSTEM" : account.ChangedBy;
                dbAccount.ChangedDate = DateTime.UtcNow;
                dbAccount.TaxType = account.TaxType ?? "SALES/USE";
                dbAccount.EffectiveTaxRate = account.EffectiveTaxRate;
                dbAccount.CompoundTax = account.CompoundTax;
                dbAccount.AcctRecovPct = account.AcctRecovPct;
                dbAccount.RecAccount = account.RecAccount ?? string.Empty;
                dbAccount.RecOrg = account.RecOrg ?? string.Empty;
                dbAccount.SuspenseAccount = account.SuspenseAccount ?? string.Empty;
                dbAccount.SuspenseOrg = account.SuspenseOrg ?? string.Empty;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.Message.Contains("violates foreign key") == true)
                {
                    return BadRequest("Account/Organization references are invalid.");
                }
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("{companyId}/{taxCode}/{accountKey}")]
        public async Task<IActionResult> Delete(string companyId, string taxCode, int accountKey)
        {
            try
            {
                var dbAccount = await _context.SalesTaxAccounts
                    .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.TaxCode == taxCode && a.AccountKey == accountKey);
                if (dbAccount is null)
                {
                    return NotFound();
                }

                _context.SalesTaxAccounts.Remove(dbAccount);
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
