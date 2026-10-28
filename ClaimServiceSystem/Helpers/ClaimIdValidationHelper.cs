using Microsoft.AspNetCore.Mvc;

namespace ClaimServiceSystem.Helpers;

public static class ClaimIdValidationHelper
{
    public static bool TryGetClaimId(string id, out Guid claimId, out IActionResult errorResult)
    {
        if (!Guid.TryParse(id, out claimId))
        {
            errorResult = new BadRequestObjectResult("Invalid claim id.");
            return false;
        }

        errorResult = null!;
        return true;
    }
}
