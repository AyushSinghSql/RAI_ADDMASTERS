using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/vendor-credit-cards")]
    public class VendorCreditCardsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorCreditCardsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ================================
        // ➕ CREATE
        // ================================
        [HttpPost]
        public async Task<IActionResult> Create(VendorCreditCardCreateDto dto)
        {
            var validation = ValidateCard(dto);
            if (validation != null)
                return BadRequest(validation);

            var exists = await _context.VendorCreditCards
                .AnyAsync(x => x.VendId == dto.VendId &&
                               x.CompanyId == dto.CompanyId);

            if (exists)
                return BadRequest("Card already exists for this vendor");

            var entity = new VendorCreditCard
            {
                VendId = dto.VendId,
                CompanyId = dto.CompanyId,
                CreditCardNumber = dto.CreditCardNumber,
                CreditCardType = dto.CreditCardType,
                CreditCardExpiryDate = dto.CreditCardExpiryDate,
                CreditCardLimitAmount = dto.CreditCardLimitAmount,
                ModifiedBy = "SYSTEM",
                ModifiedTs = DateTime.UtcNow
            };

            _context.VendorCreditCards.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ================================
        // 📥 GET ALL BY VENDOR
        // ================================
        [HttpGet("{companyId}/{vendId}")]
        public async Task<IActionResult> Get(string companyId, string vendId)
        {
            var data = await _context.VendorCreditCards
                .Where(x => x.CompanyId == companyId && x.VendId == vendId)
                .ToListAsync();

            return Ok(data);
        }

        // ================================
        // ✏️ UPDATE
        // ================================
        [HttpPut("{companyId}/{vendId}")]
        public async Task<IActionResult> Update(string companyId, string vendId, VendorCreditCardUpdateDto dto)
        {
            var entity = await _context.VendorCreditCards
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.VendId == vendId);

            if (entity == null)
                return NotFound("Card not found");

            // Validation
            if (dto.CreditCardExpiryDate.HasValue &&
                dto.CreditCardExpiryDate.Value.Date < DateTime.UtcNow.Date)
                return BadRequest("Card is expired");

            if (dto.CreditCardLimitAmount.HasValue)
            {
                if (dto.CreditCardLimitAmount <= 0)
                    return BadRequest("Limit must be greater than 0");

                if (dto.CreditCardLimitAmount > 1_000_000)
                    return BadRequest("Limit exceeds allowed maximum");
            }

            // Update fields
            entity.CreditCardNumber = dto.CreditCardNumber ?? entity.CreditCardNumber;
            entity.CreditCardType = dto.CreditCardType ?? entity.CreditCardType;
            entity.CreditCardExpiryDate = dto.CreditCardExpiryDate ?? entity.CreditCardExpiryDate;
            entity.CreditCardLimitAmount = dto.CreditCardLimitAmount ?? entity.CreditCardLimitAmount;
            entity.ModifiedTs = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ================================
        // ❌ DELETE
        // ================================
        [HttpDelete("{companyId}/{vendId}")]
        public async Task<IActionResult> Delete(string companyId, string vendId)
        {
            var entity = await _context.VendorCreditCards
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.VendId == vendId);

            if (entity == null)
                return NotFound();

            _context.VendorCreditCards.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        [NonAction]
        private string ValidateCard(VendorCreditCardCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.VendId) || string.IsNullOrWhiteSpace(dto.CompanyId))
                return "Vendor and Company are required";

            if (string.IsNullOrWhiteSpace(dto.CreditCardNumber))
                return "Card number is required";

            if (dto.CreditCardExpiryDate.HasValue &&
                dto.CreditCardExpiryDate.Value.Date < DateTime.UtcNow.Date)
                return "Card is expired";

            if (dto.CreditCardLimitAmount.HasValue)
            {
                if (dto.CreditCardLimitAmount <= 0)
                    return "Limit must be greater than 0";

                if (dto.CreditCardLimitAmount > 1_000_000) // business cap
                    return "Limit exceeds allowed maximum";
            }

            return null;
        }
    }
}
