using ClaimServiceSystem.Application.DTOs;

namespace ClaimServiceSystem.Application.Interfaces;

public interface IStaffClaimsService
{
    Task<IReadOnlyList<ClaimSummary>> GetClaims(string? status);
    Task<ClaimSummary?> AssignClaim(Guid id, AssignClaimRequest request);
    Task<ClaimSummary?> StartReview(Guid id);
    Task<ClaimSummary?> RecordAssessment(Guid id, RecordAssessmentRequest request);
    Task<ClaimDecisionResponse?> MakeDecision(Guid id, MakeDecisionRequest request);
    Task<ClaimSummary?> RequestAdditionalInformation(Guid id, RequestAdditionalInformationRequest request);
    Task<SettlementResponse?> CreateSettlement(Guid id, CreateSettlementRequest request);
    Task<SettlementResponse?> GetSettlement(Guid id);
}
