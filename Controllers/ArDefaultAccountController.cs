using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/ar-default-accounts")]
    [ApiController]
    public class ArDefaultAccountController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ArDefaultAccountController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArDfltAcct model)
        {
            model.SArTrnType = model.SArTrnType?.Trim().ToUpper();
            model.CompanyId = model.CompanyId?.Trim().ToUpper();

            // Validate Transaction Type
            var trnExists = await _context.SArTrnTypes
                .AnyAsync(x => x.SArTrnTypeId == model.SArTrnType);

            if (!trnExists)
                return BadRequest("Invalid transaction type");

            // Validate Bank Account
            if (!string.IsNullOrEmpty(model.BankAcctAbbrv))
            {
                var acctExists = await _context.BankAccts
                    .AnyAsync(x => x.BankAcctAbbrv == model.BankAcctAbbrv &&
                                   x.CompanyId == model.CompanyId);

                if (!acctExists)
                    return BadRequest("Invalid bank account");
            }

            var exists = await _context.ArDfltAccts
                .AnyAsync(x => x.SArTrnType == model.SArTrnType &&
                               x.CompanyId == model.CompanyId);

            if (exists)
                return Conflict("Default already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.ArDfltAccts.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}
