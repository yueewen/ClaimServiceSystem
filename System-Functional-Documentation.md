# Claim Service System - Functional Documentation

## 1. Purpose
This system supports end-to-end claim handling for two roles:
- **Claimant**: submits incidents/claims, tracks progress, responds to information requests, and checks decisions.
- **Claims Staff**: processes incoming claims through review, assessment, decision, and settlement stages, while monitoring workload.

The implemented lifecycle follows:
**Submitted -> Assigned/UnderReview -> (InformationRequested loop if needed) -> AssessmentCompleted -> Decision (Approved/Rejected) -> Settlement -> Settled**

---

## 2. User Roles and Access Model
Authentication and authorization are JWT-based.

### Roles
- **Claimant**
- **ClaimStaff**

### Access Control
- Claimant APIs are protected with role-based authorization and claimant identity is taken from JWT claims.
- Staff APIs are protected with `ClaimStaff` role.
- Claimant-level data access is scoped by authenticated claimant identity.

---

## 3. Claimant Functions
Base route: `/api/v1/claims`

1. **Submit a new claim**
   - `POST /api/v1/claims`
   - Function: report an incident with policy/type/description/amount.
   - Result: claim is created in **Submitted** state.

2. **List own claims**
   - `GET /api/v1/claims`
   - Function: shows authenticated claimant’s claims.

3. **View claim details and status**
   - `GET /api/v1/claims/{id}`
   - Function: tracks claim details and current lifecycle status.

4. **View claim decision**
   - `GET /api/v1/claims/{id}/decision`
   - Function: retrieves approval/rejection outcome with decision details.

5. **View information requests**
   - `GET /api/v1/claims/{id}/information-requests`
   - Function: shows additional information requests from staff.

6. **Submit information response**
   - `POST /api/v1/claims/{id}/information-requests/{requestId}/responses`
   - Function: claimant replies to a request for more evidence/data.
   - Effect: claim returns to review flow.

---

## 4. Claims Staff Functions
Base route: `/api/v1/staff/claims`

1. **List/filter claims**
   - `GET /api/v1/staff/claims`
   - Function: view queue and filter by status.

2. **Assign claim**
   - `POST /api/v1/staff/claims/{id}/assign`
   - Function: assigns claim to an officer and moves to under-review flow.

3. **Start/restart review**
   - `POST /api/v1/staff/claims/{id}/review`

4. **Record assessment**
   - `POST /api/v1/staff/claims/{id}/assessments`
   - Function: records findings and assessed amount.

5. **Make decision**
   - `POST /api/v1/staff/claims/{id}/decisions`
   - Function: approve/reject claim with reason.

6. **Request additional information**
   - `POST /api/v1/staff/claims/{id}/information-requests`
   - Function: requests more details from claimant.

7. **Create settlement**
   - `POST /api/v1/staff/claims/{id}/settlements`
   - Function: starts settlement for approved claims.

8. **View settlement details**
   - `GET /api/v1/staff/claims/{id}/settlements`

---

## 5. Dashboard Functions
Base route: `/api/v1/staff/dashboard`

- `GET /api/v1/staff/dashboard`
- Role: `ClaimStaff`
- Provides staff workload summary metrics, including:
  - total claims
  - assigned claims
  - status-level breakdown (under review, information requested, assessment completed, approved, rejected, settlement in progress)
  - decisions made by the current staff user

---

## 6. Authentication Functions
Base route: `/api/v1/auth`

- `POST /api/v1/auth/login`
- Returns JWT access token with role and subject claims.
- Pre-seeded local users (for demo/dev setup) are available in database seeding.

---

## 7. Requirement Traceability Matrix

### Requirement A (Claimant capabilities)
**“Report incident, track claim, provide additional information, receive decisions.”**

Coverage:
- Report incident: `POST /api/v1/claims` ✅
- Track claim: `GET /api/v1/claims`, `GET /api/v1/claims/{id}` ✅
- Provide additional information: `POST /api/v1/claims/{id}/information-requests/{requestId}/responses` ✅
- Receive decisions: `GET /api/v1/claims/{id}/decision` ✅

### Requirement B (Claims staff capabilities)
**“Pick up incoming claims, review/assess, progress to settlement or rejection, view workload/performance.”**

Coverage:
- Pick up incoming claims: list + assign endpoints ✅
- Review and assess: review + assessments endpoints ✅
- Progress to rejection/settlement: decisions + settlements endpoints ✅
- Workload/performance view: staff dashboard summary ✅

---

## 8. Business Rule Alignment
Implemented logic aligns with key rules:
- New claim starts as **Submitted**.
- Decision requires completed assessment.
- Only approved claims can proceed to settlement.
- Duplicate decisions and settlements are prevented.
- Claimant access is restricted to own data via authenticated identity scoping.

---

## 9. Data and Architecture Summary
- Layered structure: API -> Application contracts -> Infrastructure implementations -> EF Core persistence.
- Normalized claim workflow data model includes dedicated tables/entities for:
  - claims
  - assessments
  - decisions
  - information requests/responses
  - settlements
  - status history
  - claimant/staff identities

---

## 10. Presentation Notes (Suggested Demo Flow)
1. Login as claimant and submit a claim.
2. Login as staff and list/assign the claim.
3. Request additional information.
4. Login as claimant and submit response.
5. Login as staff and record assessment.
6. Make decision (approve/reject).
7. If approved, create settlement.
8. Show staff dashboard metrics update.
