using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeApvlGrpController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VeApvlGrpController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(VeApvlGrpDto dto)
        {
            var error = await Validate(dto);
            if (error != null)
                return BadRequest(error);

            var entity = new VeApvlGrp
            {
                VeApprvlGrpCd = dto.VeApprvlGrpCd,
                CompanyId = dto.CompanyId,
                VeApprvlGrpDesc = dto.VeApprvlGrpDesc,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.VeApvlGrps.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.VeApvlGrps
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.VeApprvlGrpCd)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{code}/{companyId}")]
        public async Task<IActionResult> GetById(string code, string companyId)
        {
            var entity = await _context.VeApvlGrpUsers.Include(x => x.VeApvlGrp).Include(x => x.ApproverUser).Where(x => x.CompanyId == companyId && x.VeApprvlGrpCd == code)
                .FirstOrDefaultAsync();

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut]
        public async Task<IActionResult> Update(VeApvlGrpDto dto)
        {
            var error = await Validate(dto, true);
            if (error != null)
                return BadRequest(error);

            var entity = await _context.VeApvlGrps
                .FindAsync(dto.VeApprvlGrpCd, dto.CompanyId);

            if (entity == null)
                return NotFound();

            entity.VeApprvlGrpDesc = dto.VeApprvlGrpDesc;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{code}/{companyId}")]
        public async Task<IActionResult> Delete(string code, string companyId)
        {
            var entity = await _context.VeApvlGrps
                .FindAsync(code, companyId);

            if (entity == null)
                return NotFound();

            //// 🔥 IMPORTANT: Prevent delete if used
            //var isUsed = await _context.VeApvlAuditHistories
            //    .AnyAsync(x => x.VeApprvlGrpCd == code && x.CompanyId == companyId);

            //if (isUsed)
            //    return BadRequest("Approval Group is in use");

            _context.VeApvlGrps.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [NonAction]
        private async Task<string> Validate(VeApvlGrpDto dto, bool isUpdate = false)
        {
            if (string.IsNullOrWhiteSpace(dto.VeApprvlGrpCd))
                return "Group Value is required";

            if (string.IsNullOrWhiteSpace(dto.CompanyId))
                return "Company Id is required";

            if (string.IsNullOrWhiteSpace(dto.VeApprvlGrpDesc))
                return "Description is required";

            if (dto.VeApprvlGrpCd.Length > 6)
                return "Group Value max length is 6";

            if (dto.CompanyId.Length > 10)
                return "Company Id max length is 10";

            // Duplicate check (only for create)
            if (!isUpdate)
            {
                var exists = await _context.VeApvlGrps.AnyAsync(x =>
                    x.VeApprvlGrpCd == dto.VeApprvlGrpCd &&
                    x.CompanyId == dto.CompanyId);

                if (exists)
                    return "Duplicate approval group";
            }

            return null;
        }
    }
}
