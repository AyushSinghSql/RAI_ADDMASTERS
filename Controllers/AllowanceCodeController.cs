using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/allowances")]
    public class AllowanceController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AllowanceController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(AllowanceDto dto)
        {
            // 🔹 Duplicate check
            if (await _context.AllowanceCodes
                .AnyAsync(x => x.AllowanceCd == dto.AllowanceCd))
            {
                return BadRequest("Allowance already exists.");
            }

            // 🔹 Validation
            if (dto.AllowanceRateAmount <= 0)
                return BadRequest("Rate must be greater than 0.");

            if (dto.WeeklyCeilHours < 0 || dto.MonthlyCeilHours < 0)
                return BadRequest("Ceiling hours cannot be negative.");

            // 🔹 PayType validation
            var payTypeExists = await _context.PayTypes
                .AnyAsync(x => x.PayTypeCode == dto.PayType);

            if (!payTypeExists)
                return BadRequest("Invalid Pay Type.");

            var entity = new AllowanceCode
            {
                AllowanceCd = dto.AllowanceCd,
                AllowanceDesc = dto.AllowanceDesc,
                PayType = dto.PayType,
                AllowBasisCd = dto.AllowBasisCd,
                AllowRateCd = dto.AllowRateCd,
                AllowanceRateAmount = dto.AllowanceRateAmount,
                WeeklyCeilHours = dto.WeeklyCeilHours,
                BiWeeklyCeilHours = dto.BiWeeklyCeilHours,
                SemiMonthlyCeilHours = dto.SemiMonthlyCeilHours,
                MonthlyCeilHours = dto.MonthlyCeilHours,
                AddLineMethod = dto.AddLineMethod,
                ProjectId = dto.ProjectId,
                AccountId = dto.AccountId,
                OrgId = dto.OrgId,
                CompanyId = dto.CompanyId,
                ModifiedBy = "system",
                TimeStamp = DateTime.UtcNow
            };

            _context.AllowanceCodes.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AllowanceCodes.ToListAsync();
            return Ok(data);
        }

        // ✅ GET BY ID
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var item = await _context.AllowanceCodes.FindAsync(code);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ✅ UPDATE
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, AllowanceDto dto)
        {
            var entity = await _context.AllowanceCodes.FindAsync(code);

            if (entity == null)
                return NotFound();

            if (dto.AllowanceRateAmount <= 0)
                return BadRequest("Rate must be greater than 0.");

            entity.AllowanceDesc = dto.AllowanceDesc;
            entity.AllowanceRateAmount = dto.AllowanceRateAmount;
            entity.WeeklyCeilHours = dto.WeeklyCeilHours;
            entity.MonthlyCeilHours = dto.MonthlyCeilHours;
            entity.ModifiedBy = "system";
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var entity = await _context.AllowanceCodes.FindAsync(code);

            if (entity == null)
                return NotFound();

            //// 🔒 Prevent delete if used
            //var isUsed = await _context.PayrollTransactions
            //    .AnyAsync(x => x.AllowanceCd == code);

            //if (isUsed)
            //    return BadRequest("Cannot delete. Allowance is used in transactions.");

            _context.AllowanceCodes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
