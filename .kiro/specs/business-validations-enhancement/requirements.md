# Requirements Document: Business Validations Enhancement

## Introduction

This document specifies the requirements for enhancing the GymManagement API with critical business validations. The system must prevent class overbooking, ensure only members with active memberships can reserve classes, and maintain proper controller organization. These validations are essential for maintaining data integrity and enforcing business rules in a multi-tenant gym management system.

## Glossary

- **Reservation_System**: The component responsible for managing class reservations
- **Capacity_Validator**: The component that validates class capacity constraints
- **Membership_Validator**: The component that validates member membership status
- **Class**: A scheduled fitness session with a defined capacity
- **Member**: A user with gym membership who can reserve classes
- **Membership**: A time-bound subscription that grants access to gym services
- **Active_Membership**: A membership where the current date falls between StartDate and EndDate (inclusive)
- **Reservation**: A booking that associates a member with a specific class
- **MembershipType_Controller**: The API controller responsible for membership type operations
- **Capacity**: The maximum number of reservations allowed for a class
- **Tenant**: An isolated gym organization in the multi-tenant system

## Requirements

### Requirement 1: Class Capacity Validation

**User Story:** As a gym administrator, I want the system to prevent class overbooking, so that classes never exceed their defined capacity and members have a guaranteed spot when they reserve.

#### Acceptance Criteria

1. WHEN a member attempts to reserve a class THEN THE Capacity_Validator SHALL count the existing reservations for that class
2. WHEN the existing reservation count equals or exceeds the class capacity THEN THE Reservation_System SHALL reject the reservation request with a "ClassAtCapacity" error
3. WHEN the existing reservation count is below the class capacity THEN THE Capacity_Validator SHALL allow the reservation to proceed to the next validation step
4. WHEN counting reservations THE Capacity_Validator SHALL only count reservations within the same tenant context
5. THE Capacity_Validator SHALL execute before any reservation entity is created in the database

### Requirement 2: Active Membership Validation

**User Story:** As a gym administrator, I want to ensure only members with active memberships can reserve classes, so that gym access is properly controlled and revenue is protected.

#### Acceptance Criteria

1. WHEN a member attempts to reserve a class THEN THE Membership_Validator SHALL retrieve all memberships for that member
2. WHEN checking membership status THEN THE Membership_Validator SHALL verify at least one membership exists where the current date is greater than or equal to StartDate AND less than or equal to EndDate
3. WHEN no active membership is found THEN THE Reservation_System SHALL reject the reservation request with a "NoActiveMembership" error
4. WHEN an active membership is found THEN THE Membership_Validator SHALL allow the reservation to proceed to the next validation step
5. WHEN retrieving memberships THE Membership_Validator SHALL only query memberships within the same tenant context
6. THE Membership_Validator SHALL execute before any reservation entity is created in the database

### Requirement 3: Duplicate Reservation Prevention

**User Story:** As a gym administrator, I want to prevent members from creating duplicate reservations for the same class, so that capacity is accurately tracked and members don't accidentally double-book.

#### Acceptance Criteria

1. WHEN a member attempts to reserve a class THEN THE Reservation_System SHALL check if a reservation already exists for that member and class combination
2. WHEN a duplicate reservation is detected THEN THE Reservation_System SHALL reject the reservation request with a "DuplicateReservation" error
3. WHEN no duplicate reservation exists THEN THE Reservation_System SHALL allow the reservation to proceed
4. WHEN checking for duplicates THE Reservation_System SHALL only query reservations within the same tenant context

### Requirement 4: Validation Execution Order

**User Story:** As a system architect, I want validations to execute in a specific order, so that the system fails fast and provides clear error messages without unnecessary database queries.

#### Acceptance Criteria

1. WHEN processing a reservation request THEN THE Reservation_System SHALL execute duplicate reservation check first
2. WHEN the duplicate check passes THEN THE Reservation_System SHALL execute membership validation second
3. WHEN membership validation passes THEN THE Reservation_System SHALL execute capacity validation third
4. WHEN capacity validation passes THEN THE Reservation_System SHALL create the reservation entity and persist it to the database
5. IF any validation fails THEN THE Reservation_System SHALL immediately return an error result without executing subsequent validations

### Requirement 5: Controller Organization

**User Story:** As a developer, I want the UpdateMembershipType endpoint in the correct controller, so that the API follows RESTful conventions and is maintainable.

#### Acceptance Criteria

1. THE MembershipType_Controller SHALL contain the UpdateMembershipType endpoint
2. THE Memberships_Controller SHALL NOT contain the UpdateMembershipType endpoint
3. THE UpdateMembershipType endpoint SHALL maintain its existing route pattern "/api/membership-types/{id}"
4. THE UpdateMembershipType endpoint SHALL maintain its existing authorization requirements
5. THE UpdateMembershipType endpoint SHALL maintain its existing request and response contracts

### Requirement 6: Error Response Format

**User Story:** As a client application developer, I want consistent error responses, so that I can handle validation failures uniformly across the API.

#### Acceptance Criteria

1. WHEN a validation fails THEN THE Reservation_System SHALL return a Result object with IsSuccess set to false
2. WHEN returning a validation error THEN THE Reservation_System SHALL include a specific error code identifying the validation failure type
3. THE Reservation_System SHALL use error code "ClassAtCapacity" for capacity validation failures
4. THE Reservation_System SHALL use error code "NoActiveMembership" for membership validation failures
5. THE Reservation_System SHALL use error code "DuplicateReservation" for duplicate reservation failures
6. WHEN a validation succeeds THEN THE Reservation_System SHALL return a Result object with IsSuccess set to true and the created reservation data

### Requirement 7: Multi-Tenant Data Isolation

**User Story:** As a system administrator, I want all validations to respect tenant boundaries, so that data from different gym organizations never intermingles.

#### Acceptance Criteria

1. WHEN querying reservations for capacity validation THEN THE Capacity_Validator SHALL apply tenant filtering using ITenantEntity query filters
2. WHEN querying memberships for validation THEN THE Membership_Validator SHALL apply tenant filtering using ITenantEntity query filters
3. WHEN checking for duplicate reservations THEN THE Reservation_System SHALL apply tenant filtering using ITenantEntity query filters
4. THE Reservation_System SHALL rely on the existing DbContext query filter configuration for tenant isolation
5. THE Reservation_System SHALL NOT implement custom tenant filtering logic in validation code

### Requirement 8: Database Transaction Integrity

**User Story:** As a system architect, I want reservation creation to be atomic, so that partial data is never persisted when validations pass but database operations fail.

#### Acceptance Criteria

1. WHEN all validations pass THEN THE Reservation_System SHALL create the reservation entity within a single database transaction
2. IF the database save operation fails THEN THE Reservation_System SHALL roll back any changes and return an error result
3. THE Reservation_System SHALL use the existing DbContext SaveChangesAsync method for transaction management
4. THE Reservation_System SHALL NOT implement custom transaction management logic
