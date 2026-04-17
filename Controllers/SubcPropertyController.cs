using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubcPropertyController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcPropertyController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? vendId,
            string? vendEmplId,
            string? companyId,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.SubcProperties.AsQueryable();

            if (!string.IsNullOrEmpty(vendId))
                query = query.Where(x => x.VendId == vendId);

            if (!string.IsNullOrEmpty(vendEmplId))
                query = query.Where(x => x.VendEmplId == vendEmplId);

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.VendId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        // ✅ GET BY ID
        [HttpGet("{vendEmplId}/{vendId}/{propId}/{companyId}")]
        public async Task<IActionResult> GetById(
            string vendEmplId,
            string vendId,
            string propId,
            string companyId)
        {
            var entity = await _context.SubcProperties.FindAsync(
                vendEmplId, vendId, propId, companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(SubcProperty model)
        {
            model.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SubcProperties.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(SubcProperty model)
        {
            var existing = await _context.SubcProperties.FindAsync(
                model.VendEmplId,
                model.VendId,
                model.PropId,
                model.CompanyId);

            if (existing == null)
                return NotFound();

            existing.PropQty = model.PropQty;
            existing.PropOwnCode = model.PropOwnCode;
            existing.AssetId = model.AssetId;
            existing.ItemNo = model.ItemNo;
            existing.IssueDate = model.IssueDate;
            existing.ReturnDate = model.ReturnDate;
            existing.WhseName = model.WhseName;
            existing.ControlId = model.ControlId;
            existing.OtherS = model.OtherS;

            existing.ModifiedBy = model.ModifiedBy;
            existing.TimeStamp = DateOnly.FromDateTime(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [HttpDelete("{vendEmplId}/{vendId}/{propId}/{companyId}")]
        public async Task<IActionResult> Delete(
            string vendEmplId,
            string vendId,
            string propId,
            string companyId)
        {
            var entity = await _context.SubcProperties.FindAsync(
                vendEmplId, vendId, propId, companyId);

            if (entity == null)
                return NotFound();

            _context.SubcProperties.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
