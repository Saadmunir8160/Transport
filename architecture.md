# Architecture Document: Smart Transport Management System

## 1. Overview
The Smart Transport Management System is a web-based, multi-tenant application designed for various transport organizations operating in Kashmir. It provides a centralized platform for managing drivers, vehicles, routes, schedules, trips, and passengers.

## 2. Technology Stack
*   **Frontend:** Angular (with PWA capabilities for offline support)
*   **Backend:** .NET (ASP.NET Core Web API)
*   **Database:** SQL Server or PostgreSQL (Suitable for multi-tenancy)
*   **AI/ML Component:** ML.NET (for seamless .NET integration) or a separate Python microservice for ETA prediction.

## 3. System Architecture

### 3.1. Multi-Tenancy Strategy
*   **Data Architecture:** Shared Database, Shared Schema with a `TenantId` (OrganizationId) column on all tenant-specific tables. Entity Framework Core Global Query Filters will be used to enforce strict data isolation.
*   **Routing & Identification:** The system will identify the tenant based on the incoming request's subdomain (e.g., `srinagar.transport-system.com`).

### 3.2. Offline & Low-Connectivity Support
*   **Client-Side (Angular):** Implemented as a Progressive Web App (PWA).
*   **Storage:** Service Workers will cache static assets. IndexedDB will be used to store previously loaded routes, schedules, and active trip data for offline access.
*   **Synchronization:** When the driver's device regains internet connectivity, locally saved trip statuses will sync to the backend using the Background Sync API or custom synchronization logic.

### 3.3. AI - ETA Prediction Component
*   A dedicated module that consumes historical trip data (distance, usual traffic patterns, weather effects).
*   Exposes an endpoint that takes the current trip parameters and returns the predicted Estimated Time of Arrival (ETA).

## 4. User Roles & Access
1.  **Super Admin:** System-wide management, tenant creation, and global reporting.
2.  **Transport Admin:** Tenant-specific daily operations, managing local drivers, vehicles, and schedules.
3.  **Driver:** Mobile-friendly PWA interface for trip execution and status updates (Active, Completed) with offline capabilities.
4.  **Passenger:** Public-facing portal for searching transport, viewing seats, and making bookings.
