using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;
using WebApi.Helpers;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountMasterController : ControllerBase
    {

            private readonly MydatabaseContext _context;
            private Helper _helper => new Helper(_context);

            public AccountMasterController(MydatabaseContext context)
            {
                _context = context;
            }

            // GET: api/AcctMaster
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
            {
                return await _context.Accounts.ToListAsync();
            }

            [HttpGet("GetAllAccounts")]
            public async Task<ActionResult<List<AcctMasterDto>>> GetAllAccounts()
            {
                var accounts = await _context.Accounts
                    .Select(p => new AcctMasterDto
                    {
                        AcctId = p.AcctId,
                        AcctName = p.AcctName,
                        LvlNo = p.LvlNo.GetValueOrDefault()
                    })
                    .ToListAsync();

                return Ok(accounts);
            }

            [HttpGet("startswith/{id}")]
            public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByPrefix(string id)
            {
                var accounts = await _context.Accounts
                    .Where(a => a.AcctId.StartsWith(id))
                    .ToListAsync();

                if (!accounts.Any())
                {
                    return NotFound("No matching accounts found");
                }

                return Ok(accounts);
            }
            [HttpGet("SearchAccounts")]
            public async Task<ActionResult<IEnumerable<Account>>> SearchAccounts(
            [FromQuery] string? search,
            [FromQuery] string? startsWith,
            [FromQuery] string? sortBy = "AcctId",
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
            {
                var query = _context.Accounts.AsQueryable();

                // 🔍 Search (by name or ID)
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => a.AcctName.Contains(search) || a.AcctId.Contains(search));
                }

                // 🔎 StartsWith filter
                if (!string.IsNullOrEmpty(startsWith))
                {
                    query = query.Where(a => a.AcctId.StartsWith(startsWith));
                }

                // ↕️ Sorting
                query = (sortBy.ToLower(), sortOrder.ToLower()) switch
                {
                    ("acctname", "desc") => query.OrderByDescending(a => a.AcctName),
                    ("acctname", _) => query.OrderBy(a => a.AcctName),

                    ("lvlno", "desc") => query.OrderByDescending(a => a.LvlNo),
                    ("lvlno", _) => query.OrderBy(a => a.LvlNo),

                    ("acctid", "desc") => query.OrderByDescending(a => a.AcctId),
                    _ => query.OrderBy(a => a.AcctId),
                };

                // 📄 Pagination
                var totalRecords = await query.CountAsync();

                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Optional: include metadata
                return Ok(new
                {
                    totalRecords,
                    page,
                    pageSize,
                    data
                });
            }
            // GET: api/AcctMaster/{id}
            [HttpGet("{id}")]
            public async Task<ActionResult<Account>> GetAcctMaster(string id)
            {
                var acct = await _context.Accounts.FindAsync(id);

                if (acct == null)
                {
                    return NotFound();
                }

                return acct;
            }

            // POST: api/AcctMaster
            //[HttpPost]
            //public async Task<ActionResult<Account>> CreateAcctMaster(Account acctMaster)
            //{
            //    _context.Accounts.Add(acctMaster);
            //    await _context.SaveChangesAsync();

            //    return CreatedAtAction(nameof(GetAcctMaster), new { id = acctMaster.AcctId }, acctMaster);
            //}

            [HttpPost]
            public async Task<ActionResult<Account>> CreateAcctMaster(Account acctMaster)
            {
                var parts = acctMaster.AcctId.Split('.');
                int level = parts.Length;

                // Validate level matches
                if (acctMaster.LvlNo != level)
                {
                    return BadRequest($"Invalid Level. AccountId {acctMaster.AcctId} should have Level {level}");
                }

                // Check parent
                if (level > 1)
                {
                    string parentId = string.Join(".", parts.Take(parts.Length - 1));

                    bool parentExists = await _context.Accounts
                        .AnyAsync(a => a.AcctId == parentId);

                    if (!parentExists)
                    {
                        return BadRequest($"Parent account {parentId} must exist before creating {acctMaster.AcctId}");
                    }
                }

                _context.Accounts.Add(acctMaster);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAcctMaster), new { id = acctMaster.AcctId }, acctMaster);
            }

        [HttpPost("CreateAcctMasterV1")]
        public async Task<ActionResult<Account>> CreateAcctMasterV1(Account acctMaster)
        {
            var parts = acctMaster.AcctId.Split('-');
            int level = parts.Length;

            // Validate level matches
            if (acctMaster.LvlNo != level)
                return BadRequest($"Invalid Level. AccountId {acctMaster.AcctId} should have Level {level}");

            var levelConfigs = await _context.AcctLevels
                .ToDictionaryAsync(l => l.Level, l => l.Lenght);

            for (int i = 0; i < parts.Length; i++)
            {
                int currentLevel = i + 1;

                if (!levelConfigs.ContainsKey(currentLevel))
                    return BadRequest($"Level configuration missing for Level {currentLevel}");

                if (parts[i].Length != levelConfigs[currentLevel])
                    return BadRequest($"Invalid length at Level {currentLevel}");
            }

            //// 🔥 NEW: Validate segment length from acct_levels table
            //for (int i = 0; i < parts.Length; i++)
            //{
            //    int currentLevel = i + 1;

            //    var levelConfig = await _context.AcctLevels
            //        .FirstOrDefaultAsync(l => l.Level == currentLevel);

            //    if (levelConfig == null)
            //        return BadRequest($"Level configuration missing for Level {currentLevel}");

            //    if (parts[i].Length != levelConfig.Lenght)
            //    {
            //        return BadRequest(
            //            $"Invalid segment length at Level {currentLevel}. " +
            //            $"Expected {levelConfig.Lenght}, got {parts[i].Length} in '{parts[i]}'"
            //        );
            //    }
            //}

            // Check parent exists if level > 1
            if (level > 1)
            {
                string parentId = string.Join("-", parts.Take(level - 1));

                bool parentExists = await _context.Accounts
                    .AnyAsync(a => a.AcctId == parentId);

                if (!parentExists)
                    return BadRequest($"Parent account {parentId} must exist before creating {acctMaster.AcctId}");
            }

            // Update LxAcctName and LxAcctSegId
            for (int i = 1; i <= level; i++)
            {
                string currentId = string.Join("-", parts.Take(i));

                string? acctNameForLevel = i == level ? acctMaster.AcctName :
                    await _context.Accounts
                        .Where(a => a.AcctId == currentId)
                        .Select(a => a.AcctName)
                        .FirstOrDefaultAsync();

                string segId = parts[i - 1];

                switch (i)
                {
                    case 1:
                        acctMaster.L1AcctName = acctNameForLevel;
                        acctMaster.L1AcctSegId = segId;
                        break;
                    case 2:
                        acctMaster.L2AcctName = acctNameForLevel;
                        acctMaster.L2AcctSegId = segId;
                        break;
                    case 3:
                        acctMaster.L3AcctName = acctNameForLevel;
                        acctMaster.L3AcctSegId = segId;
                        break;
                    case 4:
                        acctMaster.L4AcctName = acctNameForLevel;
                        acctMaster.L4AcctSegId = segId;
                        break;
                    case 5:
                        acctMaster.L5AcctName = acctNameForLevel;
                        acctMaster.L5AcctSegId = segId;
                        break;
                    case 6:
                        acctMaster.L6AcctName = acctNameForLevel;
                        acctMaster.L6AcctSegId = segId;
                        break;
                    case 7:
                        acctMaster.L7AcctName = acctNameForLevel;
                        acctMaster.L7AcctSegId = segId;
                        break;
                    case 8:
                        acctMaster.L8AcctSegId = segId;
                        break;
                }
            }

            _context.Accounts.Add(acctMaster);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var message = GetFriendlyErrorMessage(ex);
                return BadRequest(new { message });
            }

            return CreatedAtAction(nameof(GetAcctMaster), new { id = acctMaster.AcctId }, acctMaster);
        }


        //[HttpPost("CreateAcctMasterV1")]
        //public async Task<ActionResult<Account>> CreateAcctMasterV1(Account acctMaster)
        //{
        //    var parts = acctMaster.AcctId.Split('-');
        //    int level = parts.Length;

        //    // Validate level matches
        //    if (acctMaster.LvlNo != level)
        //        return BadRequest($"Invalid Level. AccountId {acctMaster.AcctId} should have Level {level}");

        //    // Check parent exists if level > 1
        //    if (level > 1)
        //    {
        //        string parentId = string.Join("-", parts.Take(level - 1));

        //        bool parentExists = await _context.Accounts
        //            .AnyAsync(a => a.AcctId == parentId);

        //        if (!parentExists)
        //            return BadRequest($"Parent account {parentId} must exist before creating {acctMaster.AcctId}");
        //    }

        //    // Update LxAcctName and LxAcctSegId
        //    for (int i = 1; i <= level; i++)
        //    {
        //        string currentId = string.Join("-", parts.Take(i));

        //        // Get account name for this level if exists
        //        string? acctNameForLevel = i == level ? acctMaster.AcctName :
        //            await _context.Accounts
        //                .Where(a => a.AcctId == currentId)
        //                .Select(a => a.AcctName)
        //                .FirstOrDefaultAsync();

        //        string segId = parts[i - 1]; // Last segment

        //        switch (i)
        //        {
        //            case 1:
        //                acctMaster.L1AcctName = acctNameForLevel;
        //                acctMaster.L1AcctSegId = segId;
        //                break;
        //            case 2:
        //                acctMaster.L2AcctName = acctNameForLevel;
        //                acctMaster.L2AcctSegId = segId;
        //                break;
        //            case 3:
        //                acctMaster.L3AcctName = acctNameForLevel;
        //                acctMaster.L3AcctSegId = segId;
        //                break;
        //            case 4:
        //                acctMaster.L4AcctName = acctNameForLevel;
        //                acctMaster.L4AcctSegId = segId;
        //                break;
        //            case 5:
        //                acctMaster.L5AcctName = acctNameForLevel;
        //                acctMaster.L5AcctSegId = segId;
        //                break;
        //            case 6:
        //                acctMaster.L6AcctName = acctNameForLevel;
        //                acctMaster.L6AcctSegId = segId;
        //                break;
        //            case 7:
        //                acctMaster.L7AcctName = acctNameForLevel;
        //                acctMaster.L7AcctSegId = segId;
        //                break;
        //            case 8:
        //                acctMaster.L8AcctSegId = segId;
        //                break;
        //        }
        //    }

        //    _context.Accounts.Add(acctMaster);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        var message = GetFriendlyErrorMessage(ex);
        //        return BadRequest(new { message });
        //    }

        //    return CreatedAtAction(nameof(GetAcctMaster), new { id = acctMaster.AcctId }, acctMaster);
        //}

        // PUT: api/AcctMaster/{id}
        [HttpPut("{id}")]
            public async Task<IActionResult> UpdateAcctMaster(string id, Account acctMaster)
            {
                if (id != acctMaster.AcctId)
                {
                    return BadRequest();
                }

                _context.Entry(acctMaster).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcctMasterExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }

            // DELETE: api/AcctMaster/{id}
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteAcctMaster(string id)
            {
                var acct = await _context.Accounts.FindAsync(id);
                if (acct == null)
                {
                    return NotFound();
                }

                _context.Accounts.Remove(acct);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            private bool AcctMasterExists(string id)
            {
                return _context.Accounts.Any(e => e.AcctId == id);
            }

        [NonAction]
        public string GetFriendlyErrorMessage(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "23505":
                        return HandleDuplicateKeyError(pgEx);

                    case "23503":
                        return HandleForeignKeyError(pgEx);

                    default:
                        return "An unexpected database error occurred.";
                }
            }

            return "An error occurred while saving data.";
        }

        [NonAction]
        private string HandleForeignKeyError(PostgresException pgEx)
        {
            var constraint = pgEx.ConstraintName;

            if (constraint.Equals("proj"))
                return "Invalid Project reference. The specified Project does not exist.";

            if (constraint.Equals("empl"))
                return "Invalid Employee reference. The selected employee does not exist.";

            if (constraint.Equals("fk_proj_org"))
                return "Invalid Organization reference. Please select a valid organization.";

            return "One of the related records does not exist. Please check your input.";
        }
        [NonAction]
        private string HandleDuplicateKeyError(PostgresException pgEx)
        {
            var constraint = pgEx.ConstraintName;

            if (constraint.Contains("proj_pkey"))
                return "Project ID already exists. Please use a different Project ID.";

            if (constraint.Contains("pk_org"))
                return "Org ID already exists. Please use a different Org ID.";

            if (constraint.Contains("empl_pkey"))
                return "Employee ID already exists. Please use a different Employee ID.";

            if (constraint.Contains("plc_codes_pkey"))
                return "PlcCode already exists. Please use a different PlcCode.";

            if (constraint.Contains("proj_flags"))
                return "This flag already exists for the selected project.";

            if (constraint.Contains("proj_hierarchy"))
                return "Hierarchy entry already exists for this project and level.";

            if (constraint.Contains("proj_empl"))
                return "This employee is already assigned to the project.";

            if (constraint.Contains("acct_master_pkey"))
                return "This AccountId is already exist Please use a different AccountId.";

            return "Duplicate record detected. Please enter unique values.";
        }

    }
}
