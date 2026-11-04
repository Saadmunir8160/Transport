# Automated Test & Production Build Verification Results
## AJK Smart Transport Management System

**Test Run Date:** October 29, 2026  
**Execution Environment:** .NET 8.0 / Microsoft SQL Server Express (`localhost\SQLEXPRESS`)  
**Overall Status:** **`100% PASSED (0 FAILURES, 0 REGRESSIONS)`**  

---

## 1. Automated Test Suite Metrics
- **Command:** `dotnet test --logger "console;verbosity=detailed"`
- **Total Tests Discovered:** 17
- **Total Tests Passed:** 17
- **Total Tests Failed:** 0
- **Total Tests Skipped:** 0
- **Execution Duration:** 7.68 seconds

### Breakdown of Executed Tests

#### A. Real Microsoft SQL Server Integration Tests (`SqlServerIntegrationTests.cs`)
*Executed directly against isolated database `AjkTransport_TestDb` with live transactions and locking:*
1. **`Concurrency_SimultaneousRequestsForSameSeat_OnlyOneSucceedsAndBookedSeatsAccurate`** (`PASSED` - 120ms):
   Simultaneous requests dispatched for Seat `"05"`. Exactly 1 succeeded (HTTP 200), 1 rejected (HTTP 400 `"Seats already booked"`). Verified `BookedSeats = 1`.
2. **`Concurrency_NearCapacityCeiling_CapacityNeverExceeded`** (`PASSED` - 5s):
   Trip capacity = 2 with 1 booked seat. Two simultaneous requests dispatched for remaining seats. Exactly 1 succeeded; 1 rejected. Maximum capacity was strictly respected.
3. **`Concurrency_CancellationRestoresSeatCapacityExactlyOnce`** (`PASSED` - 84ms):
   Duplicate concurrent cancellations sent for the same booking. First request succeeded; second returned HTTP 400 (`"already cancelled"`). Seat count was restored by booking seat count exactly once.
4. **`Security_OrganizationIsolation_TransportAdminCannotQueryAnotherOrganization`** (`PASSED` - 133ms):
   Verified that Transport Admin scoped to Organization A cannot view or mutate vehicles or trips belonging to Organization B via EF Core Global Query Filters.
5. **`Security_PassengerOwnership_PassengerCannotCancelOtherPassengersBooking`** (`PASSED` - 49ms):
   Verified that Passenger B attempting to cancel Passenger A's booking receives HTTP 404 (`"Booking not found or access denied"`).

#### B. Controller & Unit Integration Tests (`BackendCoreTests.cs` & `CrudAndRoleIntegrationTests.cs`)
6. `TransportAdmin_CanCreateAndRetrieveVehicle_WithinOrganization` (`PASSED`)
7. `TransportAdmin_CanCreateRoute_AndAddStops` (`PASSED`)
8. `TransportAdmin_CannotSeeVehiclesOfOtherOrganizations` (`PASSED`)
9. `ConcurrentBooking_SameSeatSimultaneousRequests_OnlyOneSucceeds` (`PASSED`)
10. `BookTrip_SuccessfulBooking_DecreasesAvailableSeatsAndConfirmsBooking` (`PASSED`)
11. `BookTrip_DuplicateSeatBooking_ReturnsBadRequest` (`PASSED`)
12. `BookTrip_ExceedingVehicleCapacity_ReturnsBadRequest` (`PASSED`)
13. `CancelBooking_RestoresCapacity_AndPreventsRepeatedCancellation` (`PASSED`)
14. `CancelBooking_UnauthorizedUser_ReturnsNotFound` (`PASSED`)
15. `MultiTenantIsolation_TransportAdminCannotAccessOtherOrgVehicles` (`PASSED`)
16. `DriverTripAssignment_DriverOnlyAccessesAssignedTrips` (`PASSED`)
17. `UnitTest1.Test1` (`PASSED`)

---

## 2. Angular 20 Production Build Results
- **Command:** `npx ng build --configuration production`
- **Exit Code:** 0
- **Build Duration:** 49.9 seconds
- **Output Directory:** `D:\fyp\AjkTransportSystem-UI\dist\AjkTransportSystem-UI`
- **Bundle Optimization:**
  - `main-SSIQ5TFG.js`: 1.37 MB (Estimated transfer size: 267.91 kB)
  - `polyfills-5CFQRCPP.js`: 34.59 kB (Estimated transfer size: 11.33 kB)
  - `styles-CUMATH57.css`: 29.91 kB (Estimated transfer size: 2.76 kB)
  - **Initial Total:** 1.43 MB (282.00 kB compressed)
- **Status:** Production bundle successfully verified without TypeScript or CSS compilation errors.
