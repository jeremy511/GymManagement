# Design Document: Business Validations Enhancement

## Overview

This feature enhances the GymManagement API with three critical business validations to ensure data integrity and proper business rule enforcement. The system currently has basic CRUD operations but lacks comprehensive validation logic for class reservations and has a misplaced controller endpoint. This design addresses: (1) class capacity validation to prevent overbooking, (2) active membership validation to ensure only members with valid memberships can reserve classes, and (3) proper controller organization by moving the UpdateMembershipType endpoint to its correct location.

The implementation follows the existing vertical slice architecture using MediatR for CQRS, Entity Framework Core 10 for data access, and the Result pattern for error handling. All validations will be implemented within command handlers to maintain consistency with the current architecture.

## Architecture

```mermaid
graph TD
    A[API Controller] --> B[MediatR Command]
    B --> C[Command Handler]
    C --> D{Validation Layer}
    D -->|Check 1| E[Capacity Validation]
    D -->|Check 2| F[Membership Validation]
    D -->|Check 3| G[Duplicate Check]
    E --> H[DbContext Query]
    F --> H
    G --> H
    H --> I{All Valid?}
    I -->|Yes| J[Create Reservation]
    I -->|No| K[Return Error Result]
    J --> L[Save Changes]
    L --> M[Return Success Result]
