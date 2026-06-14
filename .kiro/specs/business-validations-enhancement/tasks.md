# Implementation Plan: Business Validations Enhancement

## Overview

This implementation enhances the GymManagement API with critical business validations for class reservations and proper controller organization. The implementation modifies the ReserveClassCommand handler to enforce validation order (duplicate check → membership validation → capacity validation) and moves the UpdateMembershipType endpoint to the correct controller. All changes follow the existing vertical slice architecture with MediatR, Entity Framework Core 10, and the Result pattern.

## Tasks

- [x] 1. Update validation logic in ReserveClassCommand handler
  - [x] 1.1 Reorder validation checks to match requirements
    - Move duplicate reservation check to execute first
    - Move membership validation to execute second
    - Move capacity validation to execute third
    - Ensure fail-fast behavior (return immediately on first validation failure)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [x] 1.2 Update duplicate reservation validation
    - Verify duplicate check executes before other validations
    - Update error code from "ALREADY_RESERVED" to "DuplicateReservation"
    - Ensure tenant filtering is applied via ITenantEntity query filters
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 6.5, 7.3_

  - [x] 1.3 Update active membership validation
    - Verify membership validation executes after duplicate check
    - Update error code from "MEMBERSHIP_INACTIVE" to "NoActiveMembership"
    - Ensure validation checks: StartDate <= CurrentDate AND EndDate >= CurrentDate
    - Ensure tenant filtering is applied via ITenantEntity query filters
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 6.4, 7.2_

  - [x] 1.4 Update class capacity validation
    - Verify capacity validation executes after membership validation
    - Update error code from "CAPACITY_EXCEEDED" to "ClassAtCapacity"
    - Ensure validation counts existing reservations for the class
    - Ensure tenant filtering is applied via ITenantEntity query filters
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 6.3, 7.1_

  - [ ]* 1.5 Write unit tests for validation order
    - Test that duplicate check executes first and prevents subsequent validations
    - Test that membership validation executes second when duplicate check passes
    - Test that capacity validation executes third when membership validation passes
    - Test that reservation creation only occurs when all validations pass
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [ ]* 1.6 Write unit tests for error response format
    - Test that validation failures return Result with IsSuccess = false
    - Test that each validation returns the correct error code
    - Test that successful reservation returns Result with IsSuccess = true
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6_

- [x] 2. Checkpoint - Verify validation logic
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Move UpdateMembershipType endpoint to correct controller
  - [x] 3.1 Add UpdateMembershipType endpoint to MembershipTypesController
    - Copy the UpdateMembershipType action method from MembershipsController
    - Copy the UpdateMembershipTypeRequest record from MembershipsController
    - Update route to use "{id:guid}" to match existing pattern
    - Verify authorization attribute is inherited from controller
    - Verify response type attributes are preserved
    - _Requirements: 5.1, 5.3, 5.4, 5.5_

  - [x] 3.2 Remove UpdateMembershipType endpoint from MembershipsController
    - Delete the UpdateMembershipType action method
    - Delete the UpdateMembershipTypeRequest record
    - _Requirements: 5.2_

  - [ ]* 3.3 Write integration tests for moved endpoint
    - Test that PUT /api/membership-types/{id} updates membership type successfully
    - Test that endpoint requires Admin or Staff role
    - Test that endpoint returns 404 for non-existent membership type
    - Test that endpoint validates request data
    - _Requirements: 5.1, 5.3, 5.4, 5.5_

- [x] 4. Final checkpoint - Verify all changes
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Validation order is critical: duplicate → membership → capacity
- Error codes must match exactly: "DuplicateReservation", "NoActiveMembership", "ClassAtCapacity"
- Tenant filtering relies on existing DbContext query filter configuration (no custom logic needed)
- The existing handler already uses SaveChangesAsync for transaction management (Requirement 8.1, 8.2, 8.3, 8.4)

## Implementation Status

All required implementation tasks have been completed successfully:
- Validation logic in ReserveClassCommand handler has been updated with correct ordering and error codes
- UpdateMembershipType endpoint has been moved to MembershipTypesController
- All checkpoints have been verified

Optional test tasks (1.5, 1.6, 3.3) remain available for future implementation if needed.
