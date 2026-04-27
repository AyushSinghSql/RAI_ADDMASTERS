using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly MydatabaseContext _context;

        public CustomerController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL WITH RELATIONS
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Custs
                .Include(x => x.CustType)
                .Include(x => x.ArCrLimit)
                .Include(x => x.ArCrRating)
                .Include(x => x.SalesTerr)
                //.Include(x => x.IssueByAddr)
                .Include(x => x.SalesAbbrv)
                .ToListAsync();

            return Ok(data);
        }

        // GET BY ID
        [HttpGet("{custId}/{companyId}")]
        public async Task<IActionResult> Get(string custId, string companyId)
        {
            var entity = await _context.Custs
                .Include(x => x.CustType)
                .FirstOrDefaultAsync(x => x.CustId == custId && x.CompanyId == companyId);

            if (entity == null) return NotFound();

            return Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(Cust dto, string ModifiedBy)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //var entity = new Cust
            //{
            //    CustId = dto.CustId,
            //    CompanyId = dto.CompanyId,
            //    CustName = dto.CustName,
            //    CustTypeDc = dto.CustTypeDc,
            //    TimeStamp = DateTime.UtcNow,
            //    ModifiedBy = ModifiedBy
            //};

            dto.ModifiedBy = ModifiedBy;
            _context.Custs.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // UPDATE
        [HttpPut("{custId}/{companyId}")]
        public async Task<IActionResult> Update(string custId, string companyId, CustDto dto, string ModifiedBy)
        {
            var entity = await _context.Custs.FindAsync(custId, companyId);

            if (entity == null) return NotFound();

            entity.CustName = dto.CustName;
            entity.CustTypeDc = dto.CustTypeDc;
            entity.TimeStamp = DateTime.UtcNow;
            entity.ModifiedBy = ModifiedBy;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{custId}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, string companyId)
        {
            var entity = await _context.Custs.FindAsync(custId, companyId);

            if (entity == null) return NotFound();

            _context.Custs.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
