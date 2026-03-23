using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Models;
using WebApi.Helpers;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectMasterController : ControllerBase
    {

        private readonly MydatabaseContext _context;

        public ProjectMasterController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlProject>>> Get()
        {
            return await _context.PlProjects
                .Include(p => p.Organization)
                .Include(p => p.Financial)
                .Include(p => p.Contract)
                .Include(p => p.Address)
                .Include(p => p.Flags)
                .Include(p => p.Hierarchy).ToListAsync();
            //return await _context.Projects.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<PlProject>> Post(PlProject project)
        {
            var parts = project.ProjId.Split('.');
            int calculatedLevel = parts.Length;

            // Validate Level
            if (project.LevelNo != calculatedLevel)
            {
                return BadRequest($"Invalid Level. Project id {project.ProjId} should have Level {calculatedLevel}");
            }

            // Validate Parent
            if (calculatedLevel > 1)
            {
                string parentCode = string.Join(".", parts.Take(parts.Length - 1));

                bool parentExists = await _context.PlProjects
                    .AnyAsync(p => p.ProjId == parentCode);

                if (!parentExists)
                {
                    return BadRequest($"Parent project id {parentCode} must exist before creating {project.ProjId}");
                }
            }

            _context.PlProjects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = project.ProjId }, project);
        }

        [HttpPost("CreateProject")]
        public async Task<ActionResult<PlProject>> CreateProject(PlProject project)
        {
            // Check if project already exists
            //bool projectExists = await _context.Projects
            //    .AnyAsync(p => p.ProjId == project.ProjId);

            //if (projectExists)
            //{
            //    return Conflict($"Project with id {project.ProjId} already exists.");
            //}

            var parts = project.ProjId.Split('.');
            int calculatedLevel = parts.Length;

            // Validate Level
            if (project.LevelNo != calculatedLevel)
            {
                return BadRequest($"Invalid Level. Project id {project.ProjId} should have Level {calculatedLevel}");
            }

            // Validate Parent
            if (calculatedLevel > 1)
            {
                string parentCode = string.Join(".", parts.Take(parts.Length - 1));

                bool parentExists = await _context.PlProjects
                    .AnyAsync(p => p.ProjId == parentCode);

                if (!parentExists)
                {
                    return BadRequest($"Parent project id {parentCode} must exist before creating {project.ProjId}");
                }
            }

            // Validate sequence: all parents must exist
            for (int i = 1; i < calculatedLevel; i++)
            {
                string parentId = string.Join(".", parts.Take(i));

                bool parentExists = await _context.PlProjects
                    .AnyAsync(p => p.ProjId == parentId);

                if (!parentExists)
                {
                    return BadRequest($"Missing parent project at level {i}: {parentId}");
                }
            }

            // Build hierarchy dynamically
            var hierarchies = new List<ProjectHierarchy>();

            for (int i = 0; i < calculatedLevel; i++)
            {
                string segId = parts[i];
                string segName;

                if (i == calculatedLevel - 1)
                {
                    segName = project.ProjName;
                }
                else
                {
                    string parentId = string.Join(".", parts.Take(i + 1));

                    segName = await _context.PlProjects
                        .Where(p => p.ProjId == parentId)
                        .Select(p => p.ProjName)
                        .FirstOrDefaultAsync() ?? "UNKNOWN";
                }

                hierarchies.Add(new ProjectHierarchy
                {
                    ProjectId = project.ProjId,
                    LevelNo = i + 1,
                    ProjSegId = segId,
                    ProjSegName = segName
                });
            }

            project.Hierarchy = hierarchies;



            _context.PlProjects.Add(project);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var message = GetFriendlyErrorMessage(ex);
                return BadRequest(new { message });
            }

            return CreatedAtAction(nameof(Get), new { id = project.ProjId }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, PlProject project)
        {
            if (id != project.ProjId)
                return BadRequest();

            var existingProject = await _context.PlProjects
                .Include(p => p.Address)
                .Include(p => p.Contract)
                .Include(p => p.Flags)
                .Include(p => p.Financial)
                .Include(p => p.Hierarchy)
                .FirstOrDefaultAsync(p => p.ProjId == id);

            if (existingProject == null)
                return NotFound();

            project.ProjId = existingProject.ProjId;        // Skip primary key
            project.LevelNo = existingProject.LevelNo;  // Skip timestamp
            project.OrgId = existingProject.OrgId; // Skip or set manually
            project.CompanyId = existingProject.CompanyId; // Skip or set manually

            // Update parent fields
            _context.Entry(existingProject).CurrentValues.SetValues(project);


            // Replace children (simple approach)

            if (existingProject.Address != null)
                _context.ProjectAddress.RemoveRange(existingProject.Address);
            if (project.Address != null)
                await _context.ProjectAddress.AddRangeAsync(project.Address);


            if (existingProject.Contract != null)
                _context.ProjectContract.RemoveRange(existingProject.Contract);
            if (project.Contract != null)
                await _context.ProjectContract.AddRangeAsync(project.Contract);

            if (existingProject.Flags != null)
                _context.ProjectFlags.RemoveRange(existingProject.Flags);
            if (project.Flags != null)
                await _context.ProjectFlags.AddRangeAsync(project.Flags);


            if (existingProject.Financial != null)
                _context.ProjectFinancial.RemoveRange(existingProject.Financial);
            if (project.Financial != null)
                await _context.ProjectFinancial.AddRangeAsync(project.Financial);

            if (existingProject.Hierarchy != null)
                _context.ProjectHierarchy.RemoveRange(existingProject.Hierarchy);
            if (project.Hierarchy != null)
                await _context.ProjectHierarchy.AddRangeAsync(project.Hierarchy);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var project = await _context.PlProjects.FindAsync(id);
            if (project == null) return NotFound();

            _context.ProjectAddress.RemoveRange(_context.ProjectAddress.Where(e => e.ProjectId == id));
            _context.ProjectContract.RemoveRange(_context.ProjectContract.Where(a => a.ProjectId == id));
            _context.ProjectFlags.RemoveRange(_context.ProjectFlags.Where(c => c.ProjectId == id));
            _context.ProjectHierarchy.RemoveRange(_context.ProjectHierarchy.Where(l => l.ProjectId == id));
            _context.PlProjects.Remove(project);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.InnerException?.Message);
            }
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
