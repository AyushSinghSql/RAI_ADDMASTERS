using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly MydatabaseContext _context;
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger, MydatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] Account Account)
        {
            try
            {
                if (Account == null)
                    return BadRequest();

                await _context.Accounts.AddAsync(Account);
                await _context.SaveChangesAsync();

                return Ok(Account);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("account_pkey") == true)
                {
                    return Conflict(new
                    {
                        message = $"Account with ID '{Account.AcctId}' already exists."
                    });
                }

                return StatusCode(500, "An unexpected database error occurred.");
            }
        }
        [HttpDelete("DeleteAccount/{AcctId}")]
        public async Task<IActionResult> DeleteAccount(string AcctId)
        {
            var Account = await _context.Accounts.FindAsync(AcctId);

            if (Account == null)
                return NotFound();

            _context.Accounts.Remove(Account);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully");
        }

        [HttpGet("GetAccount/{AcctId}")]
        public async Task<IActionResult> GetAccount(string AcctId)
        {
            var Account = await _context.Accounts
                .FirstOrDefaultAsync(p => p.AcctId == AcctId);

            if (Account == null)
                return NotFound();

            return Ok(Account);
        }

        [HttpPut("UpdateAccount/{acctId}")]
        public async Task<IActionResult> UpdateAccount(string acctId, [FromBody] Account updatedAccount)
        {
            if (acctId != updatedAccount.AcctId)
                return BadRequest("Account ID mismatch");

            var account = await _context.Accounts.FindAsync(acctId);

            if (account == null)
                return NotFound("Account not found");

            // Update fields
            account.AcctName = updatedAccount.AcctName;
            account.ActiveFlag = updatedAccount.ActiveFlag;
            account.L1AcctName = updatedAccount.L1AcctName;
            account.L2AcctName = updatedAccount.L2AcctName;
            account.L3AcctName = updatedAccount.L3AcctName;
            account.L4AcctName = updatedAccount.L4AcctName;
            account.L5AcctName = updatedAccount.L5AcctName;
            account.L6AcctName = updatedAccount.L6AcctName;
            account.L7AcctName = updatedAccount.L7AcctName;
            account.LvlNo = updatedAccount.LvlNo;
            account.SAcctTypeCd = updatedAccount.SAcctTypeCd;
            account.ModifiedBy = updatedAccount.ModifiedBy;

            //account.Updatedat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(account);
        }


        [HttpPost("AddOrgAccount")]
        public async Task<IActionResult> AddOrgAccount([FromBody] OrgAccount orgAccount)
        {
            if (orgAccount == null)
                return BadRequest();

            var exists = await _context.OrgAccounts
                .AnyAsync(x => x.OrgId == orgAccount.OrgId && x.AcctId == orgAccount.AcctId);

            if (exists)
                return Conflict("OrgAccount mapping already exists");

            orgAccount.TimeStamp = DateTime.UtcNow;

            await _context.OrgAccounts.AddAsync(orgAccount);
            await _context.SaveChangesAsync();

            return Ok(orgAccount);
        }

        [HttpDelete("DeleteOrgAccount")]
        public async Task<IActionResult> DeleteOrgAccount(string orgId, string acctId)
        {
            var orgAccount = await _context.OrgAccounts
                .FirstOrDefaultAsync(x => x.OrgId == orgId && x.AcctId == acctId);

            if (orgAccount == null)
                return NotFound("Mapping not found");

            _context.OrgAccounts.Remove(orgAccount);
            await _context.SaveChangesAsync();

            return Ok("OrgAccount deleted successfully");
        }

        [HttpGet("GetOrgAccounts/{OrgId}")]
        public async Task<IActionResult> GetOrgAccounts(string OrgId)
        {
            var Accounts = await _context.OrgAccounts
                .Where(p => p.OrgId == OrgId).Select(p => p.Account).ToListAsync();

            if (Accounts == null)
                return NotFound();

            return Ok(Accounts);
        }
        [HttpGet("GetAllAccounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var Accounts = await _context.Accounts.ToListAsync();

            if (Accounts == null)
                return NotFound();

            return Ok(Accounts);
        }

        [HttpPost("BulkSyncOrgAccounts")]
        public async Task<IActionResult> BulkSyncOrgAccounts(string orgId, List<OrgAccount> accounts)
        {
            var existing = _context.OrgAccounts.Where(x => x.OrgId == orgId);
            _context.OrgAccounts.RemoveRange(existing);

            foreach (var acc in accounts)
                acc.TimeStamp = DateTime.UtcNow;

            await _context.OrgAccounts.AddRangeAsync(accounts);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("SyncOrgAccounts")]
        public async Task<IActionResult> SyncOrgAccounts([FromBody] List<OrgAccount> accounts)
        {
            // ✅ Validate request
            if (accounts == null || !accounts.Any())
                return BadRequest("Accounts list cannot be empty.");

            var invalidRecords = accounts
                .Where(x => string.IsNullOrWhiteSpace(x.OrgId) || string.IsNullOrWhiteSpace(x.AcctId))
                .ToList();

            if (invalidRecords.Any())
                return BadRequest("Some records have missing OrgId or AcctId.");

            // ✅ Remove duplicates (composite key)
            accounts = accounts
                .GroupBy(x => new { x.OrgId, x.AcctId })
                .Select(g => g.First())
                .ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var utcNow = DateTime.UtcNow;

                // ✅ Build key set (FAST lookup)
                var keySet = accounts
                    .Select(x => $"{x.OrgId}|{x.AcctId}")
                    .ToHashSet();

                var orgIds = accounts
                    .Select(x => x.OrgId)
                    .Distinct()
                    .ToList();

                // ✅ Fetch only relevant existing data
                var existing = await _context.OrgAccounts
                    .Where(x => orgIds.Contains(x.OrgId))
                    .ToListAsync();

                // ✅ Dictionary for fast matching
                var existingDict = existing.ToDictionary(
                    x => $"{x.OrgId}|{x.AcctId}",
                    x => x
                );

                int insertCount = 0;
                int updateCount = 0;

                // ========================
                // ✅ INSERT + UPDATE
                // ========================
                foreach (var acc in accounts)
                {
                    var key = $"{acc.OrgId}|{acc.AcctId}";

                    if (existingDict.TryGetValue(key, out var dbEntity))
                    {
                        // 🔄 UPDATE
                        dbEntity.AccType = acc.AccType;
                        dbEntity.ActiveFl = acc.ActiveFl;
                        dbEntity.FyCdFr = acc.FyCdFr;
                        dbEntity.PdNoFr = acc.PdNoFr;
                        dbEntity.FyCdTo = acc.FyCdTo;
                        dbEntity.PdNoTo = acc.PdNoTo;
                        dbEntity.RqApprProcCd = acc.RqApprProcCd;
                        dbEntity.ModifiedBy = acc.ModifiedBy;
                        dbEntity.TimeStamp = utcNow;

                        updateCount++;
                    }
                    else
                    {
                        // ➕ INSERT
                        acc.TimeStamp = utcNow;
                        await _context.OrgAccounts.AddAsync(acc);

                        insertCount++;
                    }
                }

                // ========================
                // ❌ DELETE (only within scope)
                // ========================
                var toDelete = existing
                    .Where(x => !keySet.Contains($"{x.OrgId}|{x.AcctId}"))
                    .ToList();

                if (toDelete.Any())
                    _context.OrgAccounts.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Org-Account mappings synced successfully",
                    inserted = insertCount,
                    updated = updateCount,
                    deleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error syncing accounts: {ex.Message}");
            }
        }

        [HttpPost("UpsertOrgAccounts")]
        public async Task<IActionResult> UpsertOrgAccounts([FromBody] List<OrgAccount> accounts)
        {
            if (accounts == null || !accounts.Any())
                return BadRequest("Accounts list cannot be empty.");

            var invalidRecords = accounts
                .Where(x => string.IsNullOrWhiteSpace(x.OrgId) || string.IsNullOrWhiteSpace(x.AcctId))
                .ToList();

            if (invalidRecords.Any())
                return BadRequest("Some records have missing OrgId or AcctId.");

            // ✅ Remove duplicates
            accounts = accounts
                .GroupBy(x => new { x.OrgId, x.AcctId })
                .Select(g => g.First())
                .ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var utcNow = DateTime.UtcNow;

                var orgIds = accounts
                    .Select(x => x.OrgId)
                    .Distinct()
                    .ToList();

                // ✅ Fetch only relevant records
                var existing = await _context.OrgAccounts
                    .Where(x => orgIds.Contains(x.OrgId))
                    .ToListAsync();

                var existingDict = existing.ToDictionary(
                    x => $"{x.OrgId}|{x.AcctId}",
                    x => x
                );

                int insertCount = 0;
                int updateCount = 0;

                foreach (var acc in accounts)
                {
                    var key = $"{acc.OrgId}|{acc.AcctId}";

                    if (existingDict.TryGetValue(key, out var dbEntity))
                    {
                        // 🔄 UPDATE
                        dbEntity.AccType = acc.AccType;
                        dbEntity.ActiveFl = acc.ActiveFl;
                        dbEntity.FyCdFr = acc.FyCdFr;
                        dbEntity.PdNoFr = acc.PdNoFr;
                        dbEntity.FyCdTo = acc.FyCdTo;
                        dbEntity.PdNoTo = acc.PdNoTo;
                        dbEntity.RqApprProcCd = acc.RqApprProcCd;
                        dbEntity.ModifiedBy = acc.ModifiedBy;
                        dbEntity.TimeStamp = utcNow;

                        updateCount++;
                    }
                    else
                    {
                        // ➕ INSERT
                        acc.TimeStamp = utcNow;
                        await _context.OrgAccounts.AddAsync(acc);

                        insertCount++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Upsert completed successfully",
                    inserted = insertCount,
                    updated = updateCount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error in upsert: {ex.Message}");
            }
        }

        [HttpPost("DeleteOrgAccounts")]
        public async Task<IActionResult> DeleteOrgAccounts([FromBody] List<OrgAccount> accounts)
        {
            if (accounts == null || !accounts.Any())
                return BadRequest("Accounts list cannot be empty.");

            var invalidRecords = accounts
                .Where(x => string.IsNullOrWhiteSpace(x.OrgId) || string.IsNullOrWhiteSpace(x.AcctId))
                .ToList();

            if (invalidRecords.Any())
                return BadRequest("Some records have missing OrgId or AcctId.");

            // ✅ Remove duplicates
            var keys = accounts
                .GroupBy(x => new { x.OrgId, x.AcctId })
                .Select(g => g.Key)
                .ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orgIds = keys.Select(x => x.OrgId).Distinct().ToList();

                // ✅ Fetch only relevant records
                var existing = await _context.OrgAccounts
                    .Where(x => orgIds.Contains(x.OrgId))
                    .ToListAsync();

                var toDelete = existing
                    .Where(x => keys.Any(k => k.OrgId == x.OrgId && k.AcctId == x.AcctId))
                    .ToList();

                if (!toDelete.Any())
                    return Ok(new { message = "No matching records found to delete.", deleted = 0 });

                _context.OrgAccounts.RemoveRange(toDelete);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Records deleted successfully",
                    deleted = toDelete.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error deleting records: {ex.Message}");
            }
        }
    }
}
