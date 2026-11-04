# FYP Presentation & Live Demonstration Script (5–7 Minutes)
## Smart Transport Management System for Azad Jammu & Kashmir (AJK)

---

### Phase 1: Introduction & Problem Context (1 Minute)
- **Speaker:** "Respected evaluators, inter-district public transport across Azad Jammu & Kashmir—from Muzaffarabad to Rawalpindi, Kotli, and Mirpur—has historically suffered from three major problems:
  1. Lack of centralized schedules and transparent seat availability.
  2. Frequent road blockages and mountain weather delays with zero passenger notification.
  3. No digital booking infrastructure, forcing passengers to physically queue at bus stands.
- Today, we present **AJK Smart Transport Management System (AJK-STMS)**: an enterprise cloud platform built with Angular 20, ASP.NET Core 9, and Microsoft SQL Server."

---

### Phase 2: Architecture & Multi-Tenant Security (1 Minute)
- **Action:** Open Swagger UI (`http://127.0.0.1:5242/swagger`) or landing page (`http://localhost:4200`).
- **Speaker:** "Our architecture implements strict multi-tenant isolation. Transport operators in AJK are independent companies. Using Entity Framework Core Global Query Filters, each Transport Admin's queries are automatically scoped by their `OrganizationId`. Transport Admin A can never see or mutate Transport Admin B's fleet or revenue."

---

### Phase 3: Passenger Search & Real-Time Booking (2.5 Minutes)
- **Action:**
  1. Go to `http://localhost:4200`.
  2. In the trip search widget, enter:
     - **Origin:** `Muzaffarabad`
     - **Destination:** `Rawalpindi`
  3. Click **Search Trips**.
  4. Point out the available trip: **Muzaffarabad - Rawalpindi Express** (Fare: Rs. 650, Vehicle: Coaster, Status: Scheduled).
  5. Click **Select Seats**.
  6. Highlight the real-time interactive vehicle seat layout. Select Seat **1A**.
  7. Fill in passenger details:
     - Name: `Muhammad Hamza`
     - Phone: `03001234567`
     - CNIC: `82201-1234567-1`
  8. Click **Pay Rs. 700 / Confirm Booking**.
  9. Show the generated **Digital Boarding Pass** with verified QR code, Ticket ID, and seat assignment.
- **Speaker:** "Notice that our booking engine uses real SQL Server row-level locking (`UPDLOCK, ROWLOCK`) with Serializable isolation. If another passenger attempts to book Seat 1A at the exact same millisecond, the engine rejects the duplicate and preserves 100% seat integrity."

---

### Phase 4: Driver Workflow & Mountain Offline Sync (1.5 Minutes)
- **Action:** Navigate to Driver portal or Driver Dashboard tabs.
- **Speaker:** "A unique reality of AJK travel is network dead-zones along mountain passes. Our driver interface provides offline persistence:
  - When offline, stop arrival check-ins, trip starts, and road incident reports are securely queued in local storage.
  - As soon as cellular connectivity returns, the synchronization engine dispatches idempotent updates to the server without duplicate event creation."

---

### Phase 5: Super Admin Oversight & Conclusion (1 Minute)
- **Action:** Show Super Admin monitoring view or automated test logs (`dotnet test` 17/17 passing).
- **Speaker:** "The system is fully audited:
  - 17 out of 17 automated unit and SQL Server integration tests are passing.
  - The Angular 20 production build has compiled cleanly.
  - All database migrations are verified against fresh instances.
  Thank you, we now invite your questions."
