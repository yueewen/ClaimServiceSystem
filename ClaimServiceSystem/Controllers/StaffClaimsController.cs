using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace ClaimServiceSystem.Controllers
{
    [Route("api/v1/staff/claims")]
    [ApiController]
    [Authorize(Roles = "ClaimStaff")]
    public class StaffClaimsController : ControllerBase
    {
        private readonly IStaffClaimsService _staffClaimsService;

        public StaffClaimsController(IStaffClaimsService staffClaimsService)
        {
            _staffClaimsService = staffClaimsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetClaims([FromQuery] string? status)
        {
            var claims = await _staffClaimsService.GetClaims(status);
            return Ok(claims);
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignClaim(string id, [FromBody] AssignClaimRequest request)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var claim = await _staffClaimsService.AssignClaim(claimId, request);
                return claim is null ? NotFound() : Ok(claim);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id}/review")]
        public async Task<IActionResult> StartReview(string id)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var claim = await _staffClaimsService.StartReview(claimId);
                return claim is null ? NotFound() : Ok(claim);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id}/assessments")]
        public async Task<IActionResult> RecordAssessment(string id, [FromBody] RecordAssessmentRequest request)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var claim = await _staffClaimsService.RecordAssessment(claimId, request);
                return claim is null ? NotFound() : Ok(claim);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id}/decisions")]
        public async Task<IActionResult> MakeDecision(string id, [FromBody] MakeDecisionRequest request)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var decision = await _staffClaimsService.MakeDecision(claimId, request);
                return decision is null ? NotFound() : Ok(decision);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id}/information-requests")]
        public async Task<IActionResult> RequestAdditionalInformation(string id, [FromBody] RequestAdditionalInformationRequest request)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var claim = await _staffClaimsService.RequestAdditionalInformation(claimId, request);
                return claim is null ? NotFound() : Ok(claim);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id}/settlements")]
        public async Task<IActionResult> CreateSettlement(string id, [FromBody] CreateSettlementRequest request)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            try
            {
                var settlement = await _staffClaimsService.CreateSettlement(claimId, request);
                return settlement is null ? NotFound() : Ok(settlement);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id}/settlements")]
        public async Task<IActionResult> GetSettlement(string id)
        {
            if (!ClaimIdValidationHelper.TryGetClaimId(id, out var claimId, out var errorResult))
            {
                return errorResult;
            }

            var settlement = await _staffClaimsService.GetSettlement(claimId);
            return settlement is null ? NotFound() : Ok(settlement);
        }

    }
}
