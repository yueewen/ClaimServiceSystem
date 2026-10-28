using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClaimServiceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ClaimServiceSystem.Controllers
{
    [Route("api/v1/staff/dashboard")]
    [ApiController]
    [Authorize(Roles = "ClaimStaff")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffWorkloadSummary()
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(staffId))
            {
                return Unauthorized();
            }

            var summary = await _dashboardService.GetStaffWorkloadSummary(staffId);
            return Ok(summary);
        }
    }
}
