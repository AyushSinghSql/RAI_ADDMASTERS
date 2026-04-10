using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using PlanningAPI.Services;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/vendor-transactions")]
    public class VendorTransactionController : ControllerBase
    {
        private readonly VendorTransactionService _service;

        public VendorTransactionController(VendorTransactionService service)
        {
            _service = service;
        }

        // ✅ Create full vendor (MASTER + CHILD TABLES)
        [HttpPost("create-full")]
        public async Task<IActionResult> CreateFullVendor(
            [FromBody] VendorTransactionRequest request)
        {
            var result = await _service.CreateOrUpdateFullVendorAsync(
                request.Vendor,
                request.Addresses,
                request.TaxDetails,
                request.Employees);

            return Ok(new { success = result });
        }

        // ✅ GET WITH PAGINATION + SORTING
        [HttpGet]
        public async Task<IActionResult> GetVendors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "vend_id",
            [FromQuery] string? sortOrder = "asc")
        {

            var result = await _service.GetVendorsAsync(
              page,
              pageSize,
              sortBy, sortOrder);


            return Ok(result);
        }
    }
}
