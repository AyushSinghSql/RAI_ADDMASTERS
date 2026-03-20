using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Helpers;
using PlanningAPI.Models;

namespace WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PlcCodesController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public PlcCodesController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlcCode>>> GetAll()
        {
            return await _context.PlcCodes.ToListAsync();
        }
        [HttpGet("GetAllPlcs")]
        public async Task<ActionResult<IEnumerable<PlcCodesDto>>> GetAllPlcs()
        {
            return await _context.PlcCodes.Select(p=> new PlcCodesDto { PlcCode = p.LaborCategoryCode, Description = p.Description}).ToListAsync();
        }
        // GET BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PlcCode>> Get(string id)
        {
            var data = await _context.PlcCodes.FindAsync(id);

            if (data == null)
                return NotFound();

            return data;
        }

        [HttpGet("SearchPlcCodes")]
        public async Task<ActionResult> SearchPlcCodes(
        [FromQuery] string? search,
        [FromQuery] string? startsWith,
        [FromQuery] string? sortBy = "CodeId",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.PlcCodes.AsQueryable();

            // 🔍 Search by ID or Description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.LaborCategoryCode.Contains(search) ||
                    p.Description.Contains(search)); // adjust field name as needed
            }

            // 🔎 Filter by CodeId prefix
            if (!string.IsNullOrEmpty(startsWith))
            {
                query = query.Where(p => p.LaborCategoryCode.StartsWith(startsWith));
            }

            // ↕️ Sorting
            query = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                ("description", "desc") => query.OrderByDescending(p => p.Description),
                ("description", _) => query.OrderBy(p => p.Description),

                ("codeid", "desc") => query.OrderByDescending(p => p.LaborCategoryCode),
                _ => query.OrderBy(p => p.LaborCategoryCode),
            };

            // 📄 Total count BEFORE pagination
            var totalRecords = await query.CountAsync();

            // 📄 Pagination
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalRecords,
                page,
                pageSize,
                data
            });
        }

        // CREATE
        [HttpPost]
        public async Task<ActionResult<PlcCode>> Create(PlcCode model)
        {
            _context.PlcCodes.Add(model);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var message = GetFriendlyErrorMessage(ex);
                return BadRequest(new { message });
            }

            return CreatedAtAction(nameof(Get), new { id = model.LaborCategoryCode }, model);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, PlcCode model)
        {
            if (id != model.LaborCategoryCode)
                return BadRequest();

            _context.Entry(model).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var data = await _context.PlcCodes.FindAsync(id);

            if (data == null)
                return NotFound();

            _context.PlcCodes.Remove(data);
            await _context.SaveChangesAsync();

            return NoContent();
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