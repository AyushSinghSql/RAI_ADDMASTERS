using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeMasterController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmployeeMasterController(MydatabaseContext context)
        {
            _context = context;
        }

        // =========================
        // EMPLOYEE (EMPL) APIs
        // =========================

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<object>> GetEmployees(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = "emplId",
            string? sortOrder = "asc",
            string? search = null)
        {
            // 🔒 Limit page size
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _context.Empls.AsNoTracking();

            // 🔍 SEARCH (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(e =>
                    (e.EmplId != null && e.EmplId.ToLower().Contains(search)) ||
                    (e.FirstName != null && e.FirstName.ToLower().Contains(search)) ||
                    (e.LastName != null && e.LastName.ToLower().Contains(search)) ||
                    (e.EmailId != null && e.EmailId.ToLower().Contains(search)) ||
                    (e.CompanyId != null && e.CompanyId.ToLower().Contains(search))
                );
            }

            // 🔽 SORTING
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("firstname", "asc") => query.OrderBy(e => e.FirstName),
                ("firstname", "desc") => query.OrderByDescending(e => e.FirstName),

                ("lastname", "asc") => query.OrderBy(e => e.LastName),
                ("lastname", "desc") => query.OrderByDescending(e => e.LastName),

                ("emailid", "asc") => query.OrderBy(e => e.EmailId),
                ("emailid", "desc") => query.OrderByDescending(e => e.EmailId),

                ("companyid", "asc") => query.OrderBy(e => e.CompanyId),
                ("companyid", "desc") => query.OrderByDescending(e => e.CompanyId),

                ("emplid", "desc") => query.OrderByDescending(e => e.EmplId),

                _ => query.OrderBy(e => e.EmplId) // default
            };

            // 📊 TOTAL COUNT (after search)
            var totalRecords = await query.CountAsync();

            // 🔢 PAGINATION
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 📦 RESPONSE
            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Data = data
            });
        }

        // GET: api/employees/{emplId}
        [HttpGet("{emplId}")]
        public async Task<ActionResult<EmployeeMaster>> GetEmployee(string emplId)
        {
            var empl = await _context.Empls.FindAsync(emplId);

            if (empl == null)
                return NotFound();

            return empl;
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<EmployeeMaster>> CreateEmployee(EmployeeMaster empl)
        {
            _context.Empls.Add(empl);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { emplId = empl.EmplId }, empl);
        }

        // PUT: api/employees/{emplId}
        [HttpPut("{emplId}")]
        public async Task<IActionResult> UpdateEmployee(string emplId, EmployeeMaster empl)
        {
            if (emplId != empl.EmplId)
                return BadRequest();

            _context.Entry(empl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Empls.Any(e => e.EmplId == emplId))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/employees/{emplId}
        [HttpDelete("{emplId}")]
        public async Task<IActionResult> DeleteEmployee(string emplId)
        {
            var empl = await _context.Empls.FindAsync(emplId);

            if (empl == null)
                return NotFound();

            _context.Empls.Remove(empl);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // EMPLOYEE LAB INFO APIs
        // =========================
        [HttpGet("labinfo")]
        public async Task<ActionResult<object>> GetLabInfo(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = "effectDt",
            string? sortOrder = "desc",
            string? search = null)
        {
            // 🔒 Limit page size
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _context.EmplLabInfos.AsNoTracking();

            // 🔍 SEARCH (includes EmplId now)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    EF.Functions.ILike(x.EmplId!, $"%{search}%") ||
                    EF.Functions.ILike(x.TitleDesc!, $"%{search}%") ||
                    EF.Functions.ILike(x.OrgId!, $"%{search}%") ||
                    EF.Functions.ILike(x.CompPlanCd!, $"%{search}%") ||
                    EF.Functions.ILike(x.SEmplTypeCd!, $"%{search}%") ||
                    EF.Functions.ILike(x.LabGrpType!, $"%{search}%")
                );
            }

            // 🔽 SORTING
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("effectdt", "asc") => query.OrderBy(x => x.EffectDt),
                ("effectdt", "desc") => query.OrderByDescending(x => x.EffectDt),

                ("annlamt", "asc") => query.OrderBy(x => x.AnnlAmt),
                ("annlamt", "desc") => query.OrderByDescending(x => x.AnnlAmt),

                ("hrlyamt", "asc") => query.OrderBy(x => x.HrlyAmt),
                ("hrlyamt", "desc") => query.OrderByDescending(x => x.HrlyAmt),

                ("emplid", "asc") => query.OrderBy(x => x.EmplId),
                ("emplid", "desc") => query.OrderByDescending(x => x.EmplId),

                _ => query.OrderByDescending(x => x.EffectDt)
            };

            // 📊 Total count
            var totalRecords = await query.CountAsync();

            // 🔢 Pagination
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 📦 Response
            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Data = data
            });
        }

        // GET: api/employees/{emplId}/labinfo/{effectDt}
        [HttpGet("{emplId}/labinfo/{effectDt}")]
        public async Task<ActionResult<EmplLabInfo>> GetLabInfoByDate(string emplId, DateTime effectDt)
        {
            var data = await _context.EmplLabInfos
                .FirstOrDefaultAsync(x => x.EmplId == emplId && x.EffectDt == effectDt);

            if (data == null)
                return NotFound();

            return data;
        }

        // GET: api/employees/{emplId}/labinfo/latest
        [HttpGet("{emplId}/labinfo/latest")]
        public async Task<ActionResult<EmplLabInfo>> GetLatestLabInfo(string emplId)
        {
            var data = await _context.EmplLabInfos
                .Where(x => x.EmplId == emplId)
                .OrderByDescending(x => x.EffectDt)
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return data;
        }

        // POST: api/employees/{emplId}/labinfo
        [HttpPost("{emplId}/labinfo")]
        public async Task<ActionResult<EmplLabInfo>> CreateLabInfo(string emplId, EmplLabInfo model)
        {
            if (emplId != model.EmplId)
                return BadRequest("Employee ID mismatch");

            _context.EmplLabInfos.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // PUT: api/employees/{emplId}/labinfo/{effectDt}
        [HttpPut("{emplId}/labinfo/{effectDt}")]
        public async Task<IActionResult> UpdateLabInfo(string emplId, DateTime effectDt, EmplLabInfo model)
        {
            if (emplId != model.EmplId || effectDt != model.EffectDt)
                return BadRequest();

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = _context.EmplLabInfos
                    .Any(x => x.EmplId == emplId && x.EffectDt == effectDt);

                if (!exists)
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/employees/{emplId}/labinfo/{effectDt}
        [HttpDelete("{emplId}/labinfo/{effectDt}")]
        public async Task<IActionResult> DeleteLabInfo(string emplId, DateTime effectDt)
        {
            var data = await _context.EmplLabInfos
                .FirstOrDefaultAsync(x => x.EmplId == emplId && x.EffectDt == effectDt);

            if (data == null)
                return NotFound();

            _context.EmplLabInfos.Remove(data);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
