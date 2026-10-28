using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Domain.Entities;
using ClaimServiceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimServiceSystem.Infrastructure.Services;

public sealed class StaffClaimsService : IStaffClaimsService
{
    private readonly ClaimsDbContext _dbContext;

    public StaffClaimsService(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClaimSummary>> GetClaims(string? status)
    {
        var query = _dbContext.Claims.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ClaimStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        return await query
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

    public async Task<ClaimSummary?> AssignClaim(Guid id, AssignClaimRequest request)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        if (claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim is already assigned.");
        }

        var officer = await GetOrCreateStaff(request.OfficerId.Trim(), StaffRole.Officer);
        claim.AssignedOfficerEntityId = officer.Id;
        claim.Status = ClaimStatus.AssignedUnderReview;
        await _dbContext.SaveChangesAsync();

        return MapToSummary(claim, await GetClaimantExternalUserId(claim));
    }

    public async Task<ClaimSummary?> StartReview(Guid id)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        if (!claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim must be assigned before review starts.");
        }

        if (claim.Status is ClaimStatus.Rejected or ClaimStatus.Settled)
        {
            throw new InvalidOperationException("Claim cannot be reviewed in its current status.");
        }

        claim.Status = ClaimStatus.AssignedUnderReview;
        await _dbContext.SaveChangesAsync();

        return MapToSummary(claim, await GetClaimantExternalUserId(claim));
    }

    public async Task<ClaimSummary?> RecordAssessment(Guid id, RecordAssessmentRequest request)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        if (claim.Status != ClaimStatus.AssignedUnderReview)
        {
            throw new InvalidOperationException("Claim must be under review before assessment.");
        }

        var hasOpenInformationRequest = await _dbContext.InformationRequests
            .AnyAsync(r => r.ClaimId == claim.Id && r.Status == InformationRequestStatus.Open);

        if (hasOpenInformationRequest)
        {
            throw new InvalidOperationException("Cannot assess claim while additional information is pending.");
        }

        if (await _dbContext.ClaimAssessments.AnyAsync(a => a.ClaimId == claim.Id))
        {
            throw new InvalidOperationException("Assessment already exists for this claim.");
        }

        if (!claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim must be assigned before assessment is recorded.");
        }

        _dbContext.ClaimAssessments.Add(new ClaimAssessment
        {
            Id = Guid.NewGuid(),
            ClaimId = claim.Id,
            AssessedAmount = request.AssessedAmount,
            Notes = request.Notes.Trim(),
            RecordedByStaffId = claim.AssignedOfficerEntityId.Value,
            RecordedAtUtc = DateTime.UtcNow
        });

        claim.Status = ClaimStatus.AssessmentCompleted;
        await _dbContext.SaveChangesAsync();

        return MapToSummary(claim, await GetClaimantExternalUserId(claim));
    }

    public async Task<ClaimDecisionResponse?> MakeDecision(Guid id, MakeDecisionRequest request)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        if (!await _dbContext.ClaimAssessments.AnyAsync(a => a.ClaimId == claim.Id))
        {
            throw new InvalidOperationException("Assessment must be completed before making a decision.");
        }

        if (await _dbContext.ClaimDecisions.AnyAsync(d => d.ClaimId == claim.Id))
        {
            throw new InvalidOperationException("Decision already exists for this claim.");
        }

        if (!Enum.TryParse<ClaimDecisionType>(request.Decision, true, out var decision))
        {
            throw new InvalidOperationException("Decision must be Approved or Rejected.");
        }

        if (!claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim must be assigned before decision is recorded.");
        }

        var decidedAt = DateTime.UtcNow;
        _dbContext.ClaimDecisions.Add(new ClaimDecision
        {
            Id = Guid.NewGuid(),
            ClaimId = claim.Id,
            Decision = decision,
            Reason = request.Reason.Trim(),
            DecidedByStaffId = claim.AssignedOfficerEntityId.Value,
            DecidedAtUtc = decidedAt
        });

        claim.Status = decision == ClaimDecisionType.Approved ? ClaimStatus.Approved : ClaimStatus.Rejected;

        await _dbContext.SaveChangesAsync();

        return new ClaimDecisionResponse
        {
            ClaimId = claim.Id,
            Decision = decision.ToString(),
            Reason = request.Reason.Trim(),
            DecidedAtUtc = decidedAt
        };
    }

    public async Task<ClaimSummary?> RequestAdditionalInformation(Guid id, RequestAdditionalInformationRequest request)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        if (claim.Status != ClaimStatus.AssignedUnderReview)
        {
            throw new InvalidOperationException("Additional information can only be requested during review.");
        }

        if (await _dbContext.InformationRequests.AnyAsync(r => r.ClaimId == claim.Id && r.Status == InformationRequestStatus.Open))
        {
            throw new InvalidOperationException("An active information request already exists.");
        }

        if (!claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim must be assigned before requesting additional information.");
        }

        _dbContext.InformationRequests.Add(new InformationRequest
        {
            Id = Guid.NewGuid(),
            ClaimId = claim.Id,
            RequestedByStaffId = claim.AssignedOfficerEntityId.Value,
            Message = request.Message.Trim(),
            Status = InformationRequestStatus.Open,
            RequestedAtUtc = DateTime.UtcNow
        });

        claim.Status = ClaimStatus.InformationRequested;

        await _dbContext.SaveChangesAsync();

        return MapToSummary(claim, await GetClaimantExternalUserId(claim));
    }

    public async Task<SettlementResponse?> CreateSettlement(Guid id, CreateSettlementRequest request)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null)
        {
            return null;
        }

        var decision = await _dbContext.ClaimDecisions.AsNoTracking().FirstOrDefaultAsync(d => d.ClaimId == claim.Id);
        if (decision?.Decision != ClaimDecisionType.Approved)
        {
            throw new InvalidOperationException("Only approved claims can proceed to settlement.");
        }

        if (await _dbContext.Settlements.AnyAsync(s => s.ClaimId == claim.Id))
        {
            throw new InvalidOperationException("Settlement already exists for this claim.");
        }

        if (!claim.AssignedOfficerEntityId.HasValue)
        {
            throw new InvalidOperationException("Claim must be assigned before settlement is created.");
        }

        var settlement = new Settlement
        {
            Id = Guid.NewGuid(),
            ClaimId = claim.Id,
            Amount = request.Amount,
            State = SettlementState.Created,
            CreatedByStaffId = claim.AssignedOfficerEntityId.Value,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Settlements.Add(settlement);
        claim.Status = ClaimStatus.Settlement;

        await _dbContext.SaveChangesAsync();

        return MapToSettlement(settlement);
    }

    public async Task<SettlementResponse?> GetSettlement(Guid id)
    {
        var settlement = await _dbContext.Settlements
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ClaimId == id);

        return settlement is null ? null : MapToSettlement(settlement);
    }

    private async Task<string> GetClaimantExternalUserId(Claim claim)
    {
        return await _dbContext.Claimants
            .Where(c => c.Id == claim.ClaimantEntityId)
            .Select(c => c.ExternalUserId)
            .FirstAsync();
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

    private static SettlementResponse MapToSettlement(Settlement settlement)
    {
        return new SettlementResponse
        {
            ClaimId = settlement.ClaimId,
            Amount = settlement.Amount,
            State = settlement.State.ToString(),
            CreatedAtUtc = settlement.CreatedAtUtc,
            SettledAtUtc = settlement.CompletedAtUtc
        };
    }

    private async Task<StaffUser> GetOrCreateStaff(string externalUserId, StaffRole role)
    {
        var staff = await _dbContext.StaffUsers.FirstOrDefaultAsync(s => s.ExternalUserId == externalUserId);
        if (staff is not null)
        {
            return staff;
        }

        staff = new StaffUser
        {
            Id = Guid.NewGuid(),
            ExternalUserId = externalUserId,
            FullName = externalUserId,
            Email = $"{externalUserId}@local",
            Role = role
        };

        _dbContext.StaffUsers.Add(staff);
        return staff;
    }
}
