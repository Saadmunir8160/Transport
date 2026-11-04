# AJK Smart Transport Management System (AJK-STMS)
## Final Year Project (FYP) — Comprehensive System Documentation

**System Title:** Smart Transport Management System for Azad Jammu & Kashmir  
**Technology Stack:** Angular 20 (Standalone Components, Signals, Reactive State), ASP.NET Core 9 Web API, Entity Framework Core 8.0, Microsoft SQL Server Express  
**Release Readiness:** READY FOR SUBMISSION & VIVA (`PASS`)  

---

## 1. Project Overview & Objectives
Azad Jammu & Kashmir's mountainous terrain and inter-district travel present unique transportation challenges—unpredictable weather, road blockages, uncoordinated private transport operators, and lack of passenger transparency.

**AJK-STMS** solves these critical pain points through:
1. **Centralized Multi-Tenant Scheduling:** Enabling independent transport operators (organizations) to manage fleets, routes, timings, and fares in isolation.
2. **Transparent Digital Ticketing:** Real-time seat layouts, digital boarding passes with QR codes, and automated collision prevention.
3. **Driver Assistance & Road Advisories:** Real-time route stop progression, emergency weather/road notifications, and resilient offline synchronization.
4. **Super Admin Regulatory Oversight:** Cross-organization audit logs, compliance tracking, and route dispute monitoring.

---

## 2. System Architecture
```
[ Angular 20 SPA Frontend ]
          │  (JWT Bearer, Reactive Signals, Vanilla Design System)
          ▼
[ ASP.NET Core 9 Web API ]
   ├── Authentication & Claims Scoping (SuperAdmin, TransportAdmin, Driver, Passenger)
   ├── Concurrency Layer (UPDLOCK, ROWLOCK, Serializable Isolation)
   ├── Multi-Tenant Filter Engine (TenantProvider + EF Core Global Query Filters)
   └── Execution Strategy (Transient Retry Resilience)
          │
          ▼
[ Microsoft SQL Server Express ]
   └── 17 Core Tables, ACID Transactions, Relational Cascades & Constraints
```

---

## 3. Four User Roles & Capabilities

| Role | Access Scope | Key Capabilities |
| :--- | :--- | :--- |
| **Super Admin** | System-wide (All Organizations) | Register transport operators, monitor fleet compliance, view system-wide revenue, dispatch cross-district road advisories. |
| **Transport Admin** | Organization Tenant Scoped | Manage assigned vehicles, drivers, route stops, schedules, fares, and view localized tenant revenue/audit logs. |
| **Driver** | Assigned Vehicle & Trip Scoped | View daily trip assignments, record stop arrivals, report mechanical/road incidents, and sync actions when reconnecting from mountain dead-zones. |
| **Passenger** | Public + Self-Booking Scoped | Search available inter-district trips, select specific seats in real time, generate digital tickets, cancel bookings, and lodge service complaints. |

---

## 4. Database Entities & Relationships
1. **Organizations:** Core tenant model (`OrganizationId`, `OrganizationCode`, `OrganizationName`, `Location`, `District`, `Status`).
2. **Users:** Role-based identity (`UserId`, `OrganizationId`, `Name`, `Email`, `Role`, `PasswordHash`, `LicenseNumber`).
3. **Vehicles:** Fleet assets (`VehicleId`, `OrganizationId`, `VehicleNumber`, `VehicleType`, `Capacity`, `FitnessExpiry`, `InsuranceExpiry`).
4. **Routes & RouteStops:** Inter-district paths with intermediate boarding points (`RouteId`, `StartPoint`, `EndPoint`, `Distance`, `MinutesFromDeparture`).
5. **Trips:** Scheduled journeys (`TripId`, `RouteId`, `VehicleId`, `DriverId`, `TravelDate`, `DepartureTime`, `Status`, `BookedSeats`).
6. **Bookings:** Passenger reservations (`BookingId`, `UserId`, `TripId`, `SeatNumber`, `Seats`, `TotalFare`, `TicketCode`, `Status`).
7. **Advisories & Notifications:** Weather, roadblock, and departure alerts (`AdvisoryId`, `NotificationId`).
8. **Documents, Feedbacks, Complaints, UserSessions, AuditLogs:** Regulatory and security tracking.

---

## 5. Concurrency, Seat Capacity & Transaction Integrity
- **Serializable Locking:** Simultaneous seat booking requests execute under `IsolationLevel.Serializable` with explicit row-level locks:
  ```csharp
  await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM Trips WITH (UPDLOCK, ROWLOCK) WHERE TripId = {0}", dto.TripId);
  ```
- **Collision Prevention:** If two passengers request Seat `05` simultaneously, exactly one reservation succeeds; the second receives an explicit HTTP 400 error (`"Seats already booked"`).
- **Capacity Ceiling:** Vehicle maximum capacity is strictly validated inside the atomic transaction, guaranteeing `Trips.BookedSeats` never exceeds `Vehicles.Capacity`.
- **Cancellation Idempotency:** Duplicate cancellation requests acquire row locks on `Bookings`, ensuring seats are restored to the trip exactly once.

---

## 6. Fresh Environment Setup Guide

### Prerequisites
- .NET 8.0 / 9.0 SDK
- Node.js (v18+) & npm
- Microsoft SQL Server 2019/2022 Express (`localhost\SQLEXPRESS`)
- `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef`)

### Database Setup
1. Open `d:\fyp\AjkTransportSystem.API\appsettings.json`.
2. Confirm the connection string points to your local SQL Server:
   `"Server=localhost\\SQLEXPRESS;Database=AjkTransport;Trusted_Connection=True;TrustServerCertificate=True;"`
3. Apply all migrations from scratch:
   ```powershell
   cd d:\fyp\AjkTransportSystem.API
   dotnet ef database update
   ```

### Running the System
1. **Start Backend API:**
   ```powershell
   cd d:\fyp\AjkTransportSystem.API
   dotnet run --urls "http://127.0.0.1:5242"
   ```
   Swagger UI available at `http://127.0.0.1:5242/swagger`.

2. **Start Frontend UI:**
   ```powershell
   cd d:\fyp\AjkTransportSystem-UI
   npm start
   ```
   Application available at `http://localhost:4200`.

---

## 7. Known Limitations
1. **Payment Gateway Integration:** Payment checkout currently simulates digital wallet transactions (JazzCash / Easypaisa mock processing) with unique token generation. Real merchant credentials must be plugged into production gateways.
2. **GPS Tracking:** Vehicle position updates currently rely on driver stop check-ins and predicted ETA calculation based on historical route averages rather than direct IoT hardware OBD-II telemetry.
3. **Mountain Offline Sync:** Offline queue operates in browser `localStorage` and retries on reconnection; prolonged offline sessions exceeding token lifetime require silent token re-authentication.
