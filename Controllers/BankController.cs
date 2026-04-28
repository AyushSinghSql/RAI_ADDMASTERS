using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/banks")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public BankController(MydatabaseContext context)
        {
            _context = context;
        }

        // CREATE US BANK
        [HttpPost("us")]
        public async Task<IActionResult> CreateUsBank(DirDepBank model)
        {
            if (model.BankAbaNo <= 0)
                return BadRequest("Invalid ABA number");

            model.BankName = model.BankName?.Trim().ToUpper();

            var exists = await _context.DirDepBanks
                .AnyAsync(x => x.BankAbaNo == model.BankAbaNo);

            if (exists)
                return Conflict("Bank already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.DirDepBanks.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // CREATE NON-US BANK
        [HttpPost("non-us")]
        public async Task<IActionResult> CreateNonUsBank(NonUsBank model)
        {
            model.NonUsBankId = model.NonUsBankId?.Trim().ToUpper();

            var exists = await _context.NonUsBanks
                .AnyAsync(x => x.NonUsBankId == model.NonUsBankId);

            if (exists)
                return Conflict("Bank already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.NonUsBanks.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}
