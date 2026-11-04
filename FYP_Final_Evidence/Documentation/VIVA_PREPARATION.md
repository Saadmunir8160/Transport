# Final Viva Examination Preparation — 18 Questions & Answers
## Smart Transport Management System for Azad Jammu & Kashmir (AJK)

---

### Q1: What is the core problem your system solves in Azad Jammu & Kashmir?
**Answer:** AJK's rugged terrain and dispersed valley routes make inter-district travel difficult. Traditional bus stands lack centralized schedules, seat transparency, and emergency alerts for landslides or weather delays. Our system provides real-time seat booking, multi-tenant fleet management for private operators, and road advisories.

---

### Q2: What is your technology stack and why did you choose it?
**Answer:** 
- **Frontend:** Angular 20 with Standalone Components and Signals for high-performance reactive state management and fast UI rendering.
- **Backend:** ASP.NET Core 9 Web API for type safety, enterprise security, and async high-throughput performance.
- **ORM:** Entity Framework Core 8.0 for code-first migrations and LINQ queries.
- **Database:** Microsoft SQL Server Express for robust ACID compliance, transaction isolation, and row-level locking.

---

### Q3: How do you handle race conditions when two passengers try to book the same seat at the exact same millisecond?
**Answer:** We implement real SQL Server row-level locking (`UPDLOCK, ROWLOCK`) with `IsolationLevel.Serializable` inside an Entity Framework Core execution strategy:
```csharp
await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM Trips WITH (UPDLOCK, ROWLOCK) WHERE TripId = {0}", dto.TripId);
```
This forces concurrent booking attempts on the same trip to serialize. The first transaction commits Seat 1A, while the second transaction reads the updated seat map, finds the collision, rolls back, and returns an HTTP 400 error (`"Seats already booked"`).

---

### Q4: How is Multi-Tenancy implemented? Can Operator A see Operator B's buses or revenue?
**Answer:** No. We utilize **Entity Framework Core Global Query Filters**. In `AppDbContext.OnModelCreating`, entities like `Vehicles`, `Routes`, `Trips`, and `Bookings` are filtered automatically by `OrganizationId == TenantId`:
```csharp
modelBuilder.Entity<Vehicle>().HasQueryFilter(e => IsSuperAdmin || TenantId == null || e.OrganizationId == TenantId);
```
When a Transport Admin logs in, their JWT token contains their `OrganizationId`. The `TenantProvider` injects this claim into EF Core, guaranteeing they cannot query or mutate another organization's records even with direct ID lookups.

---

### Q5: How do unscoped passengers search for trips across multiple organizations?
**Answer:** Passengers are consumers rather than tenant admins. When a passenger searches for trips, their `TenantId` is `null`. The global filter evaluates `TenantId == null`, allowing public scheduled trips across all active transport operators to be visible in the search results while preserving booking ownership so passengers can only view and cancel their own tickets.

---

### Q6: How does the system handle booking cancellations? How do you prevent double-cancellation?
**Answer:** In `PassengerController.CancelBooking`, an exclusive row-level lock is acquired on the `Booking` record. If the status is already `"Cancelled"`, the request is immediately rejected. If confirmed, the status transitions to `"Cancelled"`, and the associated `Trip.BookedSeats` is decremented exactly once inside an atomic transaction.

---

### Q7: How does driver offline synchronization work in remote AJK mountain passes?
**Answer:** In remote valleys where cellular coverage drops, the driver's web client queues actions (stop check-ins, trip starts, incident reports) in browser `localStorage`. When the browser detects network restoration (`navigator.onLine`), the `syncOfflineData()` method dispatches pending actions sequentially. The backend enforces idempotency: if an action was already recorded before a network drop, the server accepts it safely without duplicating logs or increments.

---

### Q8: What automated testing did you perform?
**Answer:** We wrote 17 automated xUnit tests covering two distinct tiers:
1. **12 InMemory Unit Tests:** Controller validation, role authorization, capacity boundary checks, and driver assignment scoping.
2. **5 Real SQL Server Integration Tests:** Executed against a live SQL Server test database (`AjkTransport_TestDb`) verifying real transaction isolation, same-seat collision rejection, capacity ceilings, and multi-tenant isolation. All 17 tests passed with zero failures.

---

### Q9: What happens if a fresh database needs to be created on a new server?
**Answer:** All tables, indexes, constraints, and relationships are defined in EF Core migrations (up to `20261029014102_AddDocumentsTable`). Running `dotnet ef database update` against an empty SQL Server instance recreates all 17 base tables cleanly without any manual SQL DDL scripts.

---

### Q10: How are passwords stored and authenticated?
**Answer:** Passwords are never stored in plaintext. They are hashed using cryptographic salt and hash algorithms. On login, the system validates credentials and issues a signed JSON Web Token (JWT) containing user ID, role claims, and organization tenancy ID with a configured expiration time.

---

### Q11: How do you verify vehicle safety and compliance before trip scheduling?
**Answer:** The `Vehicles` entity tracks legal compliance dates including `FitnessExpiry`, `InsuranceExpiry`, `PermitExpiry`, and `PUCExpiry`. Transport Admins and Super Admins receive automated visual warnings for vehicles with documents expiring within 30 days or already expired, preventing expired vehicles from being assigned to passenger trips.

---

### Q12: How are fares calculated for passengers boarding at intermediate route stops?
**Answer:** Routes contain ordered `RouteStops`. Each stop has a `FareFromStart` and `MinutesFromDeparture`. When a passenger selects a boarding and dropping stop, the fare is computed based on the stop-to-stop differential or baseline route fare.

---

### Q13: What is the purpose of the Super Admin role?
**Answer:** The Super Admin acts as the regulatory authority (like the AJK Transport Authority). They can register and onboard new private transport companies, inspect compliance across all organizations, broadcast system-wide road advisories (e.g. Kohala-Muzaffarabad highway closure), and view consolidated revenue and complaints.

---

### Q14: How does your frontend adhere to modern design and accessibility standards?
**Answer:** The frontend is built on a custom design system ("Kashmir Slate") using semantic HTML5, high-contrast accessible typography, WCAG AA color ratios, interactive SVG seat layouts, and responsive layouts that adapt smoothly between mobile smartphones and desktop administrative dashboards.

---

### Q15: Is live GPS tracking currently connected to physical hardware?
**Answer:** In our current release, GPS and route progress are tracked via driver stop check-ins and scheduled baseline time offsets. While the backend architecture has endpoints ready for coordinate streaming, physical IoT hardware trackers (like OBD-II GSM devices) are planned for phase two hardware integration.

---

### Q16: How is online payment handled?
**Answer:** The system currently simulates mobile digital payments (JazzCash / Easypaisa mock payment gateways) with transaction token generation and instant boarding pass issuance. Real merchant API keys can be connected to the payment service without restructuring the core booking model.

---

### Q17: What happens if the database connection drops during booking?
**Answer:** We wrap database operations in an `IExecutionStrategy` (`strategy.ExecuteAsync(...)`). EF Core automatically detects transient connection drops and safely retries the transaction. If the connection fails permanently, the transaction rolls back cleanly, ensuring partial data is never committed.

---

### Q18: What is your database normalization level?
**Answer:** The database is normalized to Third Normal Form (3NF). Redundant customer and vehicle information is separated into respective parent entities (`Users`, `Vehicles`, `Organizations`, `Routes`) and referenced via foreign keys to prevent update, insertion, and deletion anomalies.
