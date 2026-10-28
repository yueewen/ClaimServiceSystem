using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ClaimServiceSystem.Middleware;

namespace ClaimServiceSystem.Controllers
{
    [Route("api/v1/claims")]
    [ApiController]
    [Authorize(Roles = "Claimant")]
    public class ClaimsController : ControllerBase
    {
        private string ClaimantId => (string)HttpContext.Items[ClaimantContextMiddleware.ClaimantIdItemKey]!;

        private readonly IClaimsService _claimsService;

        public ClaimsController(IClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim([FromBody] SubmitClaimRequest request)
        {
            var submitRequest = new SubmitClaimRequest
            {
                ClaimantId = ClaimantId,
                PolicyNumber = request.PolicyNumber,
                ClaimType = request.ClaimType,
                Description = request.Description,
                Amount = request.Amount
            };

            var submittedClaim = await _claimsService.SubmitClaim(submitRequest);

            return CreatedAtAction(nameof(GetClaimById), new { id = submittedClaim.Id }, submittedClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyClaims()
        {
            var claims = await _claimsService.GetMyClaims(ClaimantId);
            return Ok(claims);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaimById(string id)
        {
            if (!Guid.TryParse(id, out var claimId))
            {
                return BadRequest("Invalid claim id.");
            }

            var claim = await _claimsService.GetClaimById(claimId, ClaimantId);
            if (claim is null)
            {
                return NotFound();
            }

            return Ok(claim);
        }

        [HttpGet("{id}/decision")]
        public async Task<IActionResult> GetClaimDecision(string id)
        {
            if (!Guid.TryParse(id, out var claimId))
            {
                return BadRequest("Invalid claim id.");
            }

            var decision = await _claimsService.GetClaimDecision(claimId, ClaimantId);
            if (decision is null)
            {
                return NotFound();
            }

            return Ok(decision);
        }

        [HttpGet("{id}/information-requests")]
        public async Task<IActionResult> GetInformationRequests(string id)
        {
            if (!Guid.TryParse(id, out var claimId))
            {
                return BadRequest("Invalid claim id.");
            }

            var requests = await _claimsService.GetInformationRequests(claimId, ClaimantId);
            return Ok(requests);
        }

        [HttpPost("{id}/information-requests/{requestId}/responses")]
        public async Task<IActionResult> SubmitInformationResponse(string id, string requestId, [FromBody] InformationRequestResponse response)
        {
            if (!Guid.TryParse(id, out var claimId))
            {
                return BadRequest("Invalid claim id.");
            }

            if (!Guid.TryParse(requestId, out var infoRequestId))
            {
                return BadRequest("Invalid request id.");
            }

            var updated = await _claimsService.SubmitInformationResponse(claimId, infoRequestId, ClaimantId, response);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
