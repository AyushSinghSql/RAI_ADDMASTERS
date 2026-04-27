using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/dropdowns")]
    public class DropdownController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public DropdownController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("ProfOrgs")]
        public async Task<IActionResult> ProfOrgs()
        {
            var data = await _context.ProfOrgs
                .Select(x => new
                {
                    value = x.ProfOrgId,
                    label = x.ProfOrgDesc
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 1. Certifications Dropdown
        [HttpGet("certifications")]
        public async Task<IActionResult> GetCertifications()
        {
            var data = await _context.VendorCertificationSetups
                .Where(x => x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertName)
                .Select(x => new DropdownDto
                {
                    Value = x.CertCode,
                    Label = x.CertName
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 2. Levels by Certification
        [HttpGet("certifications/{certCd}/levels")]
        public async Task<IActionResult> GetLevels(string certCd)
        {
            var data = await _context.CertificationLevels
                .Where(x => x.CertCd == certCd && x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertLevelCd)
                .Select(x => new DropdownDto
                {
                    Value = x.CertLevelCd,
                    Label = x.CertLevelDesc
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 3. Status by Certification
        [HttpGet("certifications/{certCd}/status")]
        public async Task<IActionResult> GetStatus(string certCd)
        {
            var data = await _context.CertificationStatuses
                .Where(x => x.CertCd == certCd && x.ShowLookupFl == "Y")
                .OrderBy(x => x.CertStatusCd)
                .Select(x => new DropdownDto
                {
                    Value = x.CertStatusCd,
                    Label = x.CertStatusDesc
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // CUST TYPE
        // =========================
        [HttpGet("cust-types")]
        public async Task<IActionResult> GetCustTypes()
        {
            var data = await _context.CustTypes
                .Select(x => new DropdownDto
                {
                    Value = x.CustTypeDc,
                    Label = x.CustTypeDc
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // CREDIT LIMIT
        // =========================
        [HttpGet("credit-limits")]
        public async Task<IActionResult> GetCreditLimits()
        {
            var data = await _context.ArCrLimits
                .Select(x => new DropdownDto
                {
                    Value = x.ArCrLimitKey.ToString(),
                    Label = x.CrLimitDc + " (" + x.LimitAmt + ")"
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // CREDIT RATING
        // =========================
        [HttpGet("credit-ratings")]
        public async Task<IActionResult> GetCreditRatings()
        {
            var data = await _context.ArCrRatings
                .Select(x => new DropdownDto
                {
                    Value = x.ArCrRatingKey.ToString(),
                    Label = x.CrRatingDesc
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // SALES TERRITORY
        // =========================
        [HttpGet("sales-territories")]
        public async Task<IActionResult> GetSalesTerritories()
        {
            var data = await _context.ArSalesTerrs
                .Select(x => new DropdownDto
                {
                    Value = x.SalesTerrKey.ToString(),
                    Label = x.SalesTerrDc
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // ISSUE BY ADDRESS
        // =========================
        [HttpGet("issue-addresses")]
        public async Task<IActionResult> GetIssueAddresses()
        {
            var data = await _context.IssueByAddrs
                .Select(x => new DropdownDto
                {
                    Value = x.IssueByAddrCd,
                    Label = x.IssueByAddrName + " - " + x.CityName
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================
        // SALES ABBREVIATION
        // =========================
        [HttpGet("sales-abbrv")]
        public async Task<IActionResult> GetSalesAbbrv()
        {
            var data = await _context.SalesAbbrvCds
                .Select(x => new DropdownDto
                {
                    Value = x.SalesAbbrvCdId,
                    Label = x.SalesAbbrvDesc
                })
                .ToListAsync();

            return Ok(data);
        }


    }
}
