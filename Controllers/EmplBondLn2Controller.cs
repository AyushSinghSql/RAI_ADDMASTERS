using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/empl-bond-ln2")]
    [ApiController]
    public class EmplBondLn2Controller : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public EmplBondLn2Controller(MydatabaseContext context)
        {
            _context = context;
        }

        // 🔍 GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.EmplBondLn2s.ToListAsync());

        // 🔍 GET BY KEY
        [HttpGet("{emplId}/{dedCd}/{bondLnKey}")]
        public async Task<IActionResult> Get(string emplId, string dedCd, int bondLnKey)
        {
            var entity = await _context.EmplBondLn2s
                .FindAsync(emplId, dedCd, bondLnKey);

            return entity == null ? NotFound() : Ok(entity);
        }

        // ➕ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(EmplBondLn2 model)
        {
            // 🔥 Normalize
            model.EmplId = model.EmplId?.Trim().ToUpper();
            model.DedCd = model.DedCd?.Trim().ToUpper();

            // 🔥 FK validation
            var headerExists = await _context.EmplBondHdr2s
                .AnyAsync(x => x.EmplId == model.EmplId &&
                               x.DedCd == model.DedCd);

            if (!headerExists)
                return BadRequest("Invalid Bond Header");

            // 🔥 Flag validation
            string[] valid = { "Y", "N" };
            if (!valid.Contains(model.NextPurchFl) ||
                !valid.Contains(model.EmplIsOwnerFl) ||
                !valid.Contains(model.EmplIsBenFl) ||
                !valid.Contains(model.UseEmplAddrFl))
                return BadRequest("Invalid flag values");

            // 🔥 Duplicate check
            var exists = await _context.EmplBondLn2s
                .AnyAsync(x => x.EmplId == model.EmplId &&
                               x.DedCd == model.DedCd &&
                               x.BondLnKey == model.BondLnKey);

            if (exists)
                return Conflict("Duplicate record");

            model.TimeStamp = DateTime.UtcNow;
            model.RowVersion = 1;

            _context.EmplBondLn2s.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✏️ UPDATE
        [HttpPut("{emplId}/{dedCd}/{bondLnKey}")]
        public async Task<IActionResult> Update(string emplId, string dedCd, int bondLnKey, EmplBondLn2 model)
        {
            var entity = await _context.EmplBondLn2s
                .FindAsync(emplId, dedCd, bondLnKey);

            if (entity == null)
                return NotFound();

            if (entity.RowVersion != model.RowVersion)
                return Conflict("Concurrency conflict");

            entity.SeqNo = model.SeqNo;
            entity.BondFaceAmt = model.BondFaceAmt;
            entity.BondCostAmt = model.BondCostAmt;
            entity.ModifiedBy = model.ModifiedBy;

            entity.TimeStamp = DateTime.UtcNow;
            entity.RowVersion++;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ❌ DELETE
        [HttpDelete("{emplId}/{dedCd}/{bondLnKey}")]
        public async Task<IActionResult> Delete(string emplId, string dedCd, int bondLnKey)
        {
            var entity = await _context.EmplBondLn2s
                .FindAsync(emplId, dedCd, bondLnKey);

            if (entity == null)
                return NotFound();

            _context.EmplBondLn2s.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
