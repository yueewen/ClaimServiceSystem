using ClaimServiceSystem.Application.DTOs;

namespace ClaimServiceSystem.Application.Interfaces;

public interface IClaimsService
{
    Task<ClaimSummary> SubmitClaim(SubmitClaimRequest request);
    Task<IReadOnlyList<ClaimSummary>> GetMyClaims(string claimantId);
    Task<ClaimSummary?> GetClaimById(Guid id, string claimantId);
    Task<ClaimDecisionResponse?> GetClaimDecision(Guid id, string claimantId);
    Task<IReadOnlyList<InformationRequestView>> GetInformationRequests(Guid id, string claimantId);
    Task<bool> SubmitInformationResponse(Guid id, Guid requestId, string claimantId, InformationRequestResponse response);
}
