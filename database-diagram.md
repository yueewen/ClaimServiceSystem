# Claim Service System database diagram

The diagram below reflects the entity model and relationships configured in `ClaimsDbContext`.

```mermaid
erDiagram
	CLAIMANTS ||--o{ CLAIMS : submits
	STAFF_USERS o|--o{ CLAIMS : assigned_to
	CLAIMS ||--o{ CLAIM_ASSESSMENTS : has
	STAFF_USERS ||--o{ CLAIM_ASSESSMENTS : records
	CLAIMS ||--o| CLAIM_DECISIONS : has
	STAFF_USERS ||--o{ CLAIM_DECISIONS : makes
	CLAIMS ||--o{ INFORMATION_REQUESTS : contains
	STAFF_USERS ||--o{ INFORMATION_REQUESTS : requests
	INFORMATION_REQUESTS ||--o{ INFORMATION_REQUEST_RESPONSES : receives
	CLAIMANTS ||--o{ INFORMATION_REQUEST_RESPONSES : submits
	CLAIMS ||--o| SETTLEMENTS : has
	STAFF_USERS ||--o{ SETTLEMENTS : creates
	CLAIMS ||--o{ CLAIM_STATUS_HISTORY_ENTRIES : tracks

	CLAIMANTS {
		uuid Id PK
		string ExternalUserId UK
		string FullName
		string Email
	}

	STAFF_USERS {
		uuid Id PK
		string ExternalUserId UK
		string FullName
		string Email
		string Role
	}

	CLAIMS {
		uuid Id PK
		uuid ClaimantEntityId FK
		string PolicyNumber
		string ClaimType
		string Description
		decimal Amount
		datetime SubmittedAtUtc
		string Status
		uuid AssignedOfficerEntityId FK "nullable"
	}

	CLAIM_ASSESSMENTS {
		uuid Id PK
		uuid ClaimId FK "unique"
		decimal AssessedAmount
		string Notes
		uuid RecordedByStaffId FK
		datetime RecordedAtUtc
	}

	CLAIM_DECISIONS {
		uuid Id PK
		uuid ClaimId FK "unique"
		string Decision
		string Reason
		uuid DecidedByStaffId FK
		datetime DecidedAtUtc
	}

	INFORMATION_REQUESTS {
		uuid Id PK
		uuid ClaimId FK
		uuid RequestedByStaffId FK
		string Message
		string Status
		datetime RequestedAtUtc
		datetime ClosedAtUtc "nullable"
	}

	INFORMATION_REQUEST_RESPONSES {
		uuid Id PK
		uuid InformationRequestId FK
		uuid RespondedByClaimantId FK
		string ResponseText
		datetime RespondedAtUtc
	}

	SETTLEMENTS {
		uuid Id PK
		uuid ClaimId FK "unique"
		decimal Amount
		string State
		uuid CreatedByStaffId FK
		datetime CreatedAtUtc
		datetime CompletedAtUtc "nullable"
	}

	CLAIM_STATUS_HISTORY_ENTRIES {
		uuid Id PK
		uuid ClaimId FK
		string FromStatus
		string ToStatus
		string ChangedByUserId
		datetime ChangedAtUtc
		string Reason "nullable"
	}

	AUTH_USERS {
		uuid Id PK
		string Username UK
		string PasswordHash
		string Role
		string SubjectId
	}
```

## Relationship and delete behavior notes

- A `CLAIMANT` can submit many `CLAIMS`; each claim has exactly one claimant.
- A `STAFF_USER` can be assigned many claims; the assignment is optional on a claim.
- A claim can have many assessments and status-history records, but at most one decision and one settlement.
- An information request belongs to one claim and can have many responses. Responses identify the claimant who submitted them.
- Staff references use restricted deletes. Claim-owned records use cascade deletes.
- `AuthUser` is intentionally shown without relationships because the current EF Core model does not define foreign keys to the claimant or staff tables.
- Enum properties are stored as strings by the EF Core configuration: `ClaimStatus`, `StaffRole`, `ClaimDecisionType`, `InformationRequestStatus`, and `SettlementState`.
- `UK` marks unique indexes. `FK` marks foreign keys. `nullable` marks optional columns.
