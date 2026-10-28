using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Domain.Entities;
using ClaimServiceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ApplicationInformationRequestResponse = ClaimServiceSystem.Application.DTOs.InformationRequestResponse;

namespace ClaimServiceSystem.Infrastructure.Services;

public sealed class ClaimsService : IClaimsService
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimsService(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClaimSummary> SubmitClaim(SubmitClaimRequest request)
    {
        var normalizedClaimantId = request.ClaimantId.Trim();
        var claimant = await _dbContext.Claimants.FirstOrDefaultAsync(c => c.ExternalUserId == normalizedClaimantId);
        if (claimant is null)
        {
            claimant = new Claimant
            {
                Id = Guid.NewGuid(),
                ExternalUserId = normalizedClaimantId,
                FullName = normalizedClaimantId,
                Email = $"{normalizedClaimantId}@local"
            };

            _dbContext.Claimants.Add(claimant);
        }

        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            ClaimantEntityId = claimant.Id,
            PolicyNumber = request.PolicyNumber.Trim(),
            ClaimType = request.ClaimType.Trim(),
            Description = request.Description.Trim(),
            Amount = request.Amount,
            SubmittedAtUtc = DateTime.UtcNow,
            Status = ClaimStatus.Submitted
        };

        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync();

        return MapToSummary(claim, normalizedClaimantId);
    }

    public async Task<IReadOnlyList<ClaimSummary>> GetMyClaims(string claimantId)
    {
        var normalizedClaimantId = claimantId.Trim();

        return await _dbContext.Claims
            .AsNoTracking()
            .Where(c => c.Claimant.ExternalUserId == normalizedClaimantId)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .Select(c => new ClaimSummary
            {
                Id = c.Id,
                ClaimantId = c.Claimant.ExternalUserId,
                PolicyNumber = c.PolicyNumber,
                ClaimType = c.ClaimType,
                Description = c.Description,
                Amount = c.Amount,
                SubmittedAtUtc = c.SubmittedAtUtc,
                Status = c.Status.ToString()
            })
            .ToListAsync();
    }

    public async Task<ClaimSummary?> GetClaimById(Guid id, string claimantId)
    {
        var normalizedClaimantId = claimantId.Trim();

        var claim = await _dbContext.Claims
            .AsNoTracking()
            .Include(c => c.Claimant)
            .FirstOrDefaultAsync(c => c.Id == id && c.Claimant.ExternalUserId == normalizedClaimantId);

        return claim is null ? null : MapToSummary(claim, claim.Claimant.ExternalUserId);
    }

    public async Task<ClaimDecisionResponse?> GetClaimDecision(Guid id, string claimantId)
    {
        var normalizedClaimantId = claimantId.Trim();

        return await _dbContext.ClaimDecisions
            .AsNoTracking()
            .Where(d => d.ClaimId == id && d.Claim.Claimant.ExternalUserId == normalizedClaimantId)
            .Select(d => new ClaimDecisionResponse
            {
                ClaimId = d.ClaimId,
                Decision = d.Decision.ToString(),
                Reason = d.Reason,
                DecidedAtUtc = d.DecidedAtUtc
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<InformationRequestView>> GetInformationRequests(Guid id, string claimantId)
    {
        var normalizedClaimantId = claimantId.Trim();

        return await _dbContext.InformationRequests
            .AsNoTracking()
            .Where(r => r.ClaimId == id && r.Claim.Claimant.ExternalUserId == normalizedClaimantId)
            .OrderByDescending(r => r.RequestedAtUtc)
            .Select(r => new InformationRequestView
            {
                RequestId = r.Id,
                Message = r.Message,
                RequestedAtUtc = r.RequestedAtUtc,
                RespondedAtUtc = r.Responses
                    .OrderByDescending(x => x.RespondedAtUtc)
                    .Select(x => (DateTime?)x.RespondedAtUtc)
                    .FirstOrDefault(),
                Response = r.Responses
                    .OrderByDescending(x => x.RespondedAtUtc)
                    .Select(x => x.ResponseText)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<bool> SubmitInformationResponse(Guid id, Guid requestId, string claimantId, ApplicationInformationRequestResponse response)
    {
        var normalizedClaimantId = claimantId.Trim();

        var claim = await _dbContext.Claims
            .FirstOrDefaultAsync(c => c.Id == id && c.Claimant.ExternalUserId == normalizedClaimantId);

        if (claim is null)
        {
            return false;
        }

        var requestEntity = await _dbContext.InformationRequests
            .Include(r => r.Responses)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ClaimId == id);

        if (requestEntity is null || requestEntity.Status != InformationRequestStatus.Open)
        {
            return false;
        }

        requestEntity.Responses.Add(new Domain.Entities.InformationRequestResponse
        {
            Id = Guid.NewGuid(),
            InformationRequestId = requestEntity.Id,
            RespondedByClaimantId = claim.ClaimantEntityId,
            ResponseText = response.Response.Trim(),
            RespondedAtUtc = DateTime.UtcNow
        });

        requestEntity.Status = InformationRequestStatus.Responded;
        requestEntity.ClosedAtUtc = DateTime.UtcNow;
        claim.Status = ClaimStatus.AssignedUnderReview;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static ClaimSummary MapToSummary(Claim claim, string claimantExternalUserId)
    {
        return new ClaimSummary
        {
            Id = claim.Id,
            ClaimantId = claimantExternalUserId,
            PolicyNumber = claim.PolicyNumber,
            ClaimType = claim.ClaimType,
            Description = claim.Description,
            Amount = claim.Amount,
            SubmittedAtUtc = claim.SubmittedAtUtc,
            Status = claim.Status.ToString()
        };
    }
}
