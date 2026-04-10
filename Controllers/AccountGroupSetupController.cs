using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountGroupSetupController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AccountGroupSetupController(MydatabaseContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE / ADD
        // =========================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AccountGroupSetup model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var exists = await _context.AccountGroupSetup
                    .AnyAsync(x => x.AcctGroupCode == model.AcctGroupCode && x.AccountId == model.AccountId);

                if (exists)
                    return BadRequest(new { message = "Mapping already exists." });

                _context.AccountGroupSetup.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Mapping saved successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // UPDATE
        // =========================
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] AccountGroupSetup model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid data." });

            try
            {
                var existing = await _context.AccountGroupSetup
                    .FirstOrDefaultAsync(x => x.AcctGroupCode == model.AcctGroupCode && x.AccountId == model.AccountId);

                if (existing == null)
                    return NotFound(new { message = "Mapping not found." });

                // Update fields
                existing.AccountFunctionDescription = model.AccountFunctionDescription;
                existing.ModifiedBy = model.ModifiedBy;
                existing.CompanyId = model.CompanyId;
                existing.ProjectAccountAbbreviation = model.ProjectAccountAbbreviation;
                existing.ActiveFlag = model.ActiveFlag;
                existing.RevenueMappedAccount = model.RevenueMappedAccount;
                existing.SalaryCapMappedAccount = model.SalaryCapMappedAccount;
                existing.TimeStamp = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Mapping updated successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string acctGroupCode, string accountId,string CompanyId)
        {
            if (string.IsNullOrWhiteSpace(acctGroupCode) || string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(CompanyId))
                return BadRequest(new { message = "Mapping identifiers are required." });

            try
            {
                var existing = await _context.AccountGroupSetup
                    .FirstOrDefaultAsync(x => x.AcctGroupCode == acctGroupCode && x.AccountId == accountId && x.CompanyId == CompanyId);

                if (existing == null)
                    return NotFound(new { message = "Mapping not found." });

                _context.AccountGroupSetup.Remove(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Mapping deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = GetFriendlyErrorMessage(ex) });
            }
        }

        //// =========================
        //// GET ALL WITH PAGINATION
        //// =========================
        //[HttpGet("getall")]
        //public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        //{
        //    var query = _context.AccountGroupSetups.AsQueryable();
        //    var totalCount = await query.CountAsync();

        //    var data = await query.Include(x => x.Account)
        //        .OrderBy(x => x.AcctGroupCode)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        pageNumber,
        //        pageSize,
        //        totalCount,
        //        data
        //    });
        //}

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.AccountGroupSetup
                //.Include(x => x.Account) // join with account table
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.AcctGroupCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AccountGroupSetupDTo
                {
                    AcctGroupCode = x.AcctGroupCode,
                    AccountId = x.AccountId,
                    AccountFunctionDescription = x.AccountFunctionDescription,
                    ProjectAccountAbbreviation = x.ProjectAccountAbbreviation,
                    ActiveFlag = x.ActiveFlag,
                    RevenueMappedAccount = x.RevenueMappedAccount,
                    SalaryCapMappedAccount = x.SalaryCapMappedAccount,
                    AccountName = x.Account != null ? x.Account.AcctName : null
                })
                .ToListAsync();

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount,
                data
            });
        }

        // =========================
        // GET BY GroupCode
        // =========================
        [HttpGet("get")]
        public async Task<IActionResult> Get(string acctGroupCode, string CompanyId)
        {
            if (string.IsNullOrWhiteSpace(acctGroupCode) || string.IsNullOrWhiteSpace(CompanyId))
                return BadRequest(new { message = "Mapping identifiers are required." });
            var data = await _context.AccountGroupSetup
                .Where(x => x.AcctGroupCode == acctGroupCode && x.CompanyId == CompanyId).Select(p => p.AccountId).ToListAsync();

            if (data == null)
                return NotFound(new { message = "Mapping not found." });

            return Ok(data);
        }

        // =========================
        // SEARCH WITH PAGINATION
        // =========================
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string? acctGroupCode,
            string? accountId,
            string? companyId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.AccountGroupSetup.AsQueryable();

            if (!string.IsNullOrWhiteSpace(acctGroupCode))
                query = query.Where(x => x.AcctGroupCode.Contains(acctGroupCode));

            if (!string.IsNullOrWhiteSpace(accountId))
                query = query.Where(x => x.AccountId.Contains(accountId));

            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.AcctGroupCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount,
                data
            });
        }


        // =========================
        // Replace Account Groups
        // =========================
        [HttpPost("Replace")]
        public async Task<IActionResult> BulkReplace([FromBody] List<AccountGroupSetup> models)
        {
            if (models == null || !models.Any())
                return BadRequest(new { message = "No data provided." });

            var result = new List<object>();

            try
            {
                foreach (var model in models)
                {
                    try
                    {
                        // Remove existing record if exists
                        var existing = await _context.AccountGroupSetup
                            .AsNoTracking() // 🔥 important for bulk ops
                            .FirstOrDefaultAsync(x =>
                                x.AcctGroupCode == model.AcctGroupCode &&
                                x.AccountId == model.AccountId &&
                                x.CompanyId == model.CompanyId);

                        //var existing = await _context.AccountGroupSetups
                        //    .FirstOrDefaultAsync(x => x.AcctGroupCode == model.AcctGroupCode
                        //                           && x.AccountId == model.AccountId);

                        if (existing != null)
                        {
                            _context.AccountGroupSetup.Remove(existing);
                        }

                        // Add the new record
                        _context.AccountGroupSetup.Add(model);

                        result.Add(new
                        {
                            model.AcctGroupCode,
                            model.AccountId,
                            status = existing != null ? "Replaced" : "Created"
                        });
                    }
                    catch (DbUpdateException ex)
                    {
                        result.Add(new
                        {
                            model.AcctGroupCode,
                            model.AccountId,
                            status = "Failed",
                            error = GetFriendlyErrorMessage(ex)
                        });
                    }
                    catch (Exception ex)
                    {
                        result.Add(new
                        {
                            model.AcctGroupCode,
                            model.AccountId,
                            status = "Failed",
                            error = ex.Message
                        });
                    }
                }

                // Save all changes in one batch
                await _context.SaveChangesAsync();

                return Ok(new { message = "Bulk replace completed.", result });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Batch save failed.", error = GetFriendlyErrorMessage(ex) });
            }
        }


        // =========================
        // FRIENDLY ERROR HANDLER
        // =========================
        private string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "23505":
                        return "Duplicate mapping exists.";

                    case "23503":
                        return "Account ID does not exist or is referenced elsewhere.";

                    case "23502":
                        return "Required field is missing.";

                    case "22001":
                        return "Input value is too long for a field.";

                    default:
                        return $"Database error: {pgEx.MessageText}";
                }
            }

            return "An unexpected error occurred while saving data.";
        }
    }
}
