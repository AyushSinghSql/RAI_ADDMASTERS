using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/bank-accounts")]
    [ApiController]
    public class BankAccountController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public BankAccountController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(BankAcct model)
        {
            model.BankAcctAbbrv = model.BankAcctAbbrv?.Trim().ToUpper();
            model.CompanyId = model.CompanyId?.Trim().ToUpper();

            // 🔥 ERP RULE: Only one bank type allowed
            if (model.BankAbaNo == null && string.IsNullOrEmpty(model.NonUsBankId))
                return BadRequest("Either US or Non-US bank required");

            if (model.BankAbaNo != null && model.NonUsBankId != null)
                return BadRequest("Cannot assign both US and Non-US bank");

            var exists = await _context.BankAccts
                .AnyAsync(x => x.BankAcctAbbrv == model.BankAcctAbbrv &&
                               x.CompanyId == model.CompanyId);

            if (exists)
                return Conflict("Duplicate account");

            model.TimeStamp = DateTime.UtcNow;

            _context.BankAccts.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}
