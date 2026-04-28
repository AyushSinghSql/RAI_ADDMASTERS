using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustVatInfoController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CustVatInfoController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 100)
        {
            pageSize = Math.Min(pageSize, 500);

            var data = await _context.CustVatInfos
                .AsNoTracking()
                .OrderBy(x => x.CustId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET ONE
        [HttpGet("{custId}/{taxId}/{companyId}")]
        public async Task<IActionResult> Get(string custId, string taxId, string companyId)
        {
            custId = Normalize(custId);
            taxId = Normalize(taxId);
            companyId = Normalize(companyId);

            var entity = await _context.CustVatInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustId == custId && x.TaxId == taxId && x.CompanyId == companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CustVatInfo model)
        {
            try
            {
                Validate(model);

                var exists = await _context.CustVatInfos
                    .AnyAsync(x => x.CustId == model.CustId &&
                                   x.TaxId == model.TaxId &&
                                   x.CompanyId == model.CompanyId);

                if (exists)
                    return Conflict("Duplicate record.");

                // Ensure single default tax
                if (model.DfltTaxIdFl == "Y")
                {
                    var hasDefault = await _context.CustVatInfos
                        .AnyAsync(x => x.CustId == model.CustId &&
                                       x.CompanyId == model.CompanyId &&
                                       x.DfltTaxIdFl == "Y");

                    if (hasDefault)
                        return Conflict("Default tax already exists.");
                }

                model.TimeStamp = DateTime.UtcNow;
                model.RowVersion = 1;

                _context.CustVatInfos.Add(model);
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE
        [HttpPut("{custId}/{taxId}/{companyId}")]
        public async Task<IActionResult> Update(string custId, string taxId, string companyId, CustVatInfo model)
        {
            try
            {
                Validate(model);

                var entity = await _context.CustVatInfos
                    .FirstOrDefaultAsync(x => x.CustId == custId &&
                                              x.TaxId == taxId &&
                                              x.CompanyId == companyId);

                if (entity == null)
                    return NotFound();

                // Concurrency check
                if (entity.RowVersion != model.RowVersion)
                    return Conflict("Record modified by another user.");

                // Default rule
                if (model.DfltTaxIdFl == "Y")
                {
                    var otherDefault = await _context.CustVatInfos
                        .AnyAsync(x => x.CustId == model.CustId &&
                                       x.CompanyId == model.CompanyId &&
                                       x.TaxId != model.TaxId &&
                                       x.DfltTaxIdFl == "Y");

                    if (otherDefault)
                        return Conflict("Another default tax exists.");
                }

                entity.TaxLocCd = model.TaxLocCd;
                entity.DfltTaxIdFl = model.DfltTaxIdFl;
                entity.ModifiedBy = model.ModifiedBy;
                entity.TimeStamp = DateTime.UtcNow;
                entity.RowVersion++;

                await _context.SaveChangesAsync();

                return Ok(entity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE
        [HttpDelete("{custId}/{taxId}/{companyId}")]
        public async Task<IActionResult> Delete(string custId, string taxId, string companyId)
        {
            var entity = await _context.CustVatInfos
                .FirstOrDefaultAsync(x => x.CustId == custId &&
                                          x.TaxId == taxId &&
                                          x.CompanyId == companyId);

            if (entity == null)
                return NotFound();

            _context.CustVatInfos.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
        [NonAction]
        public string Normalize(string val)
    => val?.Trim().ToUpperInvariant();
        [NonAction]
        public void Validate(CustVatInfo m)
        {
            if (m == null)
                throw new ArgumentException("Request required.");

            m.CustId = Normalize(m.CustId);
            m.TaxId = Normalize(m.TaxId);
            m.CompanyId = Normalize(m.CompanyId);
            m.TaxLocCd = m.TaxLocCd?.Trim();
            m.ModifiedBy = m.ModifiedBy?.Trim();
            m.DfltTaxIdFl = Normalize(m.DfltTaxIdFl);

            if (string.IsNullOrWhiteSpace(m.CustId) || m.CustId.Length > 12)
                throw new ArgumentException("Invalid cust_id");

            if (string.IsNullOrWhiteSpace(m.TaxId) || m.TaxId.Length > 20)
                throw new ArgumentException("Invalid tax_id");

            if (string.IsNullOrWhiteSpace(m.CompanyId) || m.CompanyId.Length > 10)
                throw new ArgumentException("Invalid company_id");

            if (string.IsNullOrWhiteSpace(m.TaxLocCd) || m.TaxLocCd.Length > 30)
                throw new ArgumentException("Invalid tax_loc_cd");

            if (m.DfltTaxIdFl != "Y" && m.DfltTaxIdFl != "N")
                throw new ArgumentException("dflt_tax_id_fl must be Y/N");

            if (string.IsNullOrWhiteSpace(m.ModifiedBy))
                throw new ArgumentException("modified_by required");
        }
    }
}
