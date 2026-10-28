using ClaimServiceSystem.Application.DTOs;

namespace ClaimServiceSystem.Application.Interfaces;

public interface IDashboardService
{
    Task<StaffDashboardSummaryResponse> GetStaffWorkloadSummary(string staffExternalUserId);
}
