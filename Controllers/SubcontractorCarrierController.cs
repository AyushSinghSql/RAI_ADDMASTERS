using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcontractorCarrierController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SubcontractorCarrierController(MydatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(SubcontractorCarrier dto)
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(dto.CarrierId) ||
                string.IsNullOrWhiteSpace(dto.CarrierName))
                return BadRequest("Carrier Id and Name required");

            // Phone validation
            if (dto.PhoneNumber.Length < 7)
                return BadRequest("Invalid phone number");

            // Postal code validation
            if (string.IsNullOrWhiteSpace(dto.PostalCode))
                return BadRequest("Postal code required");

            // Duplicate prevention
            var exists = await _context.SubcontractorCarriers
                .AnyAsync(x => x.CarrierId == dto.CarrierId);

            if (exists)
                return BadRequest("Carrier already exists");

            _context.SubcontractorCarriers.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.SubcontractorCarriers.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.SubcontractorCarriers.FindAsync(id);

            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> Update(SubcontractorCarrier dto)
        {
            var entity = await _context.SubcontractorCarriers
                .FindAsync(dto.CarrierId);

            if (entity == null)
                return NotFound();

            entity.CarrierName = dto.CarrierName;
            entity.AgentName = dto.AgentName;
            entity.AgentTitle = dto.AgentTitle;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.FaxNumber = dto.FaxNumber;
            entity.AddressLine1 = dto.AddressLine1;
            entity.City = dto.City;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var isUsed = await _context.SubcontractorInsuranceLines
                .AnyAsync(x => x.CarrierId == id);

            if (isUsed)
                return BadRequest("Carrier is used in transactions");

            var entity = await _context.SubcontractorCarriers.FindAsync(id);

            if (entity == null)
                return NotFound();

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
