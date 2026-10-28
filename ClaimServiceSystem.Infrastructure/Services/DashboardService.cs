using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Domain.Entities;
using ClaimServiceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimServiceSystem.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ClaimsDbContext _dbContext;

    public DashboardService(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StaffDashboardSummaryResponse> GetStaffWorkloadSummary(string staffExternalUserId)
    {
        var normalizedStaffId = staffExternalUserId.Trim();
        var staff = await _dbContext.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalUserId == normalizedStaffId);

        if (staff is null)
        {
            return new StaffDashboardSummaryResponse
            {
                StaffId = normalizedStaffId
            };
        }

        var totalClaims = await _dbContext.Claims.CountAsync();
        var assignedStatusCounts = await _dbContext.Claims
            .AsNoTracking()
            .Where(c => c.AssignedOfficerEntityId == staff.Id)
            .GroupBy(c => c.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var assignedToMe = assignedStatusCounts.Values.Sum();
        var underReview = GetStatusCount(ClaimStatus.AssignedUnderReview);
        var informationRequested = GetStatusCount(ClaimStatus.InformationRequested);
        var assessmentCompleted = GetStatusCount(ClaimStatus.AssessmentCompleted);
        var approved = GetStatusCount(ClaimStatus.Approved);
        var rejected = GetStatusCount(ClaimStatus.Rejected);
        var settlementInProgress = GetStatusCount(ClaimStatus.Settlement);
        var decisionsMadeByMe = await _dbContext.ClaimDecisions.CountAsync(d => d.DecidedByStaffId == staff.Id);

        int GetStatusCount(ClaimStatus status)
        {
            return assignedStatusCounts.GetValueOrDefault(status, 0);
        }

        return new StaffDashboardSummaryResponse
        {
            StaffId = normalizedStaffId,
            TotalClaims = totalClaims,
            AssignedToMe = assignedToMe,
            UnderReview = underReview,
            InformationRequested = informationRequested,
            AssessmentCompleted = assessmentCompleted,
            Approved = approved,
            Rejected = rejected,
            SettlementInProgress = settlementInProgress,
            DecisionsMadeByMe = decisionsMadeByMe
        };
    }
}
