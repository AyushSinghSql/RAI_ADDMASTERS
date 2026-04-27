using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using PlanningAPI.Services;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LienWaiverHdrsController : ControllerBase
    {

        private readonly MydatabaseContext _context;

        public LienWaiverHdrsController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(LienWaiverHdrDto dto)
        {
            var error = await Validate(dto);
            if (error != null)
                return BadRequest(error);

            var entity = new LienWaiverHdr
            {
                LienNo = dto.LienNo,
                WaiverTypeCd = dto.WaiverTypeCd,
                VendCustId = dto.VendCustId,
                ProjId = dto.ProjId,
                LienAmt = dto.LienAmt,
                LienDate = dto.LienDate,
                SentDt = dto.SentDt,
                ReturnedDt = dto.ReturnedDt,
                FinalWaiverFl = dto.FinalWaiverFl,
                AddrDc = dto.AddrDc,
                ChkNo = dto.ChkNo,
                CompanyId = dto.CompanyId,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow
            };

            _context.LienWaiverHdrs.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.LienWaiverHdrs
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.LienDate)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{lienNo}")]
        public async Task<IActionResult> GetById(long lienNo)
        {
            var entity = await _context.LienWaiverHdrs.FindAsync(lienNo);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPut]
        public async Task<IActionResult> Update(LienWaiverHdrDto dto)
        {
            var error = await Validate(dto, true);
            if (error != null)
                return BadRequest(error);

            var entity = await _context.LienWaiverHdrs.FindAsync(dto.LienNo);
            if (entity == null)
                return NotFound();

            entity.WaiverTypeCd = dto.WaiverTypeCd;
            entity.VendCustId = dto.VendCustId;
            entity.ProjId = dto.ProjId;
            entity.LienAmt = dto.LienAmt;
            entity.LienDate = dto.LienDate;
            entity.SentDt = dto.SentDt;
            entity.ReturnedDt = dto.ReturnedDt;
            entity.FinalWaiverFl = dto.FinalWaiverFl;
            entity.AddrDc = dto.AddrDc;
            entity.ChkNo = dto.ChkNo;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{lienNo}")]
        public async Task<IActionResult> Delete(long lienNo)
        {
            var entity = await _context.LienWaiverHdrs.FindAsync(lienNo);
            if (entity == null)
                return NotFound();

            //// Example protection (if linked to detail lines)
            //var isUsed = await _context.LienWaiverLines
            //    .AnyAsync(x => x.LienNo == lienNo);

            //if (isUsed)
            //    return BadRequest("Cannot delete, linked records exist");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string companyId)
        {
            var data = await _context.LienWaiverHdrs
                .Where(x => x.CompanyId == companyId)
                .Select(x => new
                {
                    value = x.LienNo,
                    label = x.LienNo + " - " + x.ProjId
                })
                .ToListAsync();

            return Ok(data);
        }
        [NonAction]
        private async Task<string> Validate(LienWaiverHdrDto dto, bool isUpdate = false)
        {
            if (dto.LienNo <= 0)
                return "Lien No required";

            if (string.IsNullOrWhiteSpace(dto.WaiverTypeCd))
                return "Waiver Type required";

            if (string.IsNullOrWhiteSpace(dto.VendCustId))
                return "Vendor/Customer required";

            if (dto.LienAmt <= 0)
                return "Lien Amount must be > 0";

            if (dto.SentDt < dto.LienDate)
                return "Sent Date cannot be before Lien Date";

            if (dto.ReturnedDt.HasValue && dto.ReturnedDt < dto.SentDt)
                return "Returned Date cannot be before Sent Date";

            if (dto.FinalWaiverFl != "Y" && dto.FinalWaiverFl != "N")
                return "Final Waiver must be Y/N";

            if (!isUpdate)
            {
                var exists = await _context.LienWaiverHdrs
                    .AnyAsync(x => x.LienNo == dto.LienNo);

                if (exists)
                    return "Duplicate Lien No";
            }

            return null;
        }
    }
}
