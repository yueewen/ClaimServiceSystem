namespace ClaimServiceSystem.Domain.Entities;

public enum ClaimStatus
{
    Submitted = 1,
    AssignedUnderReview = 2,
    InformationRequested = 3,
    AssessmentCompleted = 4,
    Approved = 5,
    Rejected = 6,
    Settlement = 7,
    Settled = 8
}

public enum ClaimDecisionType
{
    Approved = 1,
    Rejected = 2
}

public enum SettlementState
{
    Created = 1,
    ProcessingPayment = 2,
    Completed = 3
}
