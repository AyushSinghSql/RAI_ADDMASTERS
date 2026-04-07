using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.DTO;
using PlanningAPI.Models;
using PlanningAPI.Services;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/posting-validation")]
    public class PostingValidationController : ControllerBase
    {
        private readonly IFinancialControlService _service;

        public PostingValidationController(IFinancialControlService service)
        {
            _service = service;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate(TransactionValidationDto dto)
        {
            var error = await _service.ValidateAsync(
                dto.FyCd,
                dto.PeriodNo,
                dto.SubPeriodNo,
                dto.JournalCode,
                dto.CompanyId);

            if (error != null)
                return BadRequest(new { message = error });

            return Ok(new { message = "Valid for posting" });
        }

        [HttpPost("PostingValidateGlobal")]
        public async Task<IActionResult> Validate(PostingValidationDto dto)
        {
            var result = await _service.Validate(dto);

            if (!result.IsAllowed)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}
