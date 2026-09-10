# Clinic Management System - Architecture & Technical Documentation

Welcome to the comprehensive technical documentation for the **Clinic Management System**. This document outlines the architectural blueprints, design decisions, domain aggregates, database model, security policies, and testing methodologies.

---

## 1. Architectural Overview

The system is constructed following the principles of **Clean Architecture** (Onion/Hexagonal Architecture) coupled with **CQRS** (Command Query Responsibility Segregation) powered by **MediatR**.

```
┌─────────────────────────────────────────────────────────────────┐
│                    ClinicSystem.Api                             │
│       Controllers, Middlewares, Auth Handlers, Scalar OpenAPI   │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                 ClinicSystem.Application                        │
│   Commands, Queries, Validators, Pipeline Behaviors, DTOs       │
└────────────────┬───────────────────────────────┬────────────────┘
                 │                               │
                 ▼                               ▼
┌────────────────────────────────┐ ┌──────────────────────────────┐
│     ClinicSystem.Domain        │ │ ClinicSystem.Infrastructure  │
│  Entities, Value Objects,      │ │ EF Core, HybridCache, Redis, │
│  Aggregates, Domain Events     │ │ QuestPDF, BCrypt, Security   │
└────────────────────────────────┘ └──────────────────────────────┘
```

### Layer Responsibilities

| Project | Purpose & Responsibilities |
| :--- | :--- |
| **`ClinicSystem.Domain`** | Enterprise Core: Independent of external frameworks. Encapsulates business invariants, state transitions, Value Objects, and Domain Events (`Patient`, `Doctor`, `DoctorWorkingSchedule`, `Appointment`, `Invoice`, `ConsultationRecord`, `User`). |
| **`ClinicSystem.Application`** | Business Logic Orchestration: Defines use cases as CQRS Commands and Queries. Hosts FluentValidation validators, cross-cutting MediatR behaviors, and abstraction interfaces. |
| **`ClinicSystem.Infrastructure`** | External Integrations & Persistence: Implements database access via EF Core 9 (SQL Server / InMemory), Distributed Caching (HybridCache + Redis), PDF Document Generation (QuestPDF), and Security services (BCrypt, JWT Token Generator). |
| **`ClinicSystem.Api`** | Presentation & Entry Point: Exposes RESTful HTTP endpoints, manages JWT bearer token authentication, request logging, RFC 7807 ProblemDetails middleware, and interactive OpenAPI documentation via Scalar. |
| **`ClinicSystem.UnitTests`** | Isolated unit testing of domain aggregate rules and application handlers using xUnit, FluentAssertions, and Moq. |
| **`ClinicSystem.IntegrationTests`** | End-to-end API integration testing via `WebApplicationFactory<Program>` with in-memory database execution. |

---

## 2. Core Domain Models & Aggregates

### 1. Appointments & Slot Engine (`Appointment`)
* **Aggregate Invariants**:
  * Scheduling ensures `ScheduledStartTimeUtc < ScheduledEndTimeUtc` and validates future time windows.
  * Auto-assigns daily ascending queue numbers per doctor.
  * Overlap Conflict Detection via `HasConflictAsync`: Prevents overlapping appointments for the same doctor.
  * Strict Lifecycle State Machine:
    $$\text{Scheduled} \longrightarrow \text{Confirmed} \longrightarrow \text{InProgress} \longrightarrow \text{Completed}$$
    $$\text{Scheduled / Confirmed} \longrightarrow \text{Cancelled}$$
    $$\text{Scheduled / Confirmed} \longrightarrow \text{Rescheduled}$$

### 2. Doctor Availability & Shifts (`Doctor` & `DoctorWorkingSchedule`)
* Doctors maintain configurable weekly shifts with:
  * `DayOfWeek`
  * `StartTime` / `EndTime` (TimeSpan)
  * `SlotDurationMinutes` (e.g., 15, 30, 45, 60 mins)
* The **Slot Engine** (`GetDoctorAvailableSlotsQuery`) dynamically computes available booking intervals by projecting scheduled slots against existing non-cancelled appointments.

### 3. Billing & Financial Ledger (`Invoice` & `InvoiceItem`)
* Invoices start in `Draft` state, recalculating `TotalAmount = Sum(Quantity * UnitPrice)` dynamically.
* When issued (`Issue()`), an invoice transitions to `Issued`.
* Payments (`RecordPayment`):
  * Partial payment updates status to `PartiallyPaid` and computes `BalanceDue = TotalAmount - PaidAmount`.
  * Full payment updates status to `Paid` and timestamps `PaidAtUtc`.

### 4. Electronic Medical Records (`ConsultationRecord` & `PrescriptionItem`)
* Doctors record patient medical encounters including symptoms, clinical diagnoses, treatment plans, and medical notes.
* Encapsulates structured prescription items: medication name, dosage, frequency, duration in days, and patient instructions.

### 5. Identity & Access Control (`User`)
* Supports secure user accounts with BCrypt password hashing.
* Roles: `Admin`, `Doctor`, `Receptionist`, `Cashier`, `Patient`.
* Associated with optional `AssociatedDoctorId` or `AssociatedPatientId` for contextual self-service access.

---

## 3. Cross-Cutting Engineering Concerns

### 1. MediatR Validation Pipeline (`ValidationBehavior<TRequest, TResponse>`)
All commands entering the system automatically execute their corresponding `FluentValidation` rules before reaching business handlers:
```
HTTP Request ──► ExceptionMiddleware ──► ValidationBehavior ──► CachingBehavior ──► CommandHandler
```
If validation fails, a `ValidationException` is thrown and mapped by `ExceptionHandlingMiddleware` into a structured RFC 7807 ProblemDetails payload with an `errors` dictionary.

### 2. Multi-Tier Hybrid Caching (`HybridCache`)
The solution utilizes the native **.NET 9 `HybridCache`**:
* **L1 Cache**: In-process high-speed memory cache.
* **L2 Cache**: Distributed Redis backing store (with stampede protection).
* Automatic cache invalidation via `ICacheInvalidator` pipeline behavior upon state mutations.

### 3. Automatic Auditing & Soft Deletion
* Entities inheriting from `BaseEntity` implement `ISoftDeletable`.
* In `ApplicationDbContext.SaveChangesAsync`:
  * Entity deletions are automatically intercepted and converted into soft-deletes (`IsDeleted = true`, `DeletedAtUtc = DateTime.UtcNow`).
  * Modifications automatically refresh `LastModifiedAtUtc = DateTime.UtcNow`.
* Global EF Core Query Filters (`HasQueryFilter(e => !e.IsDeleted)`) guarantee soft-deleted records are excluded from queries across all application layers.

### 4. High-Performance PDF Generation (`QuestPDF`)
* Built using `QuestPDF` fluent document composition engine directly into in-memory byte streams.
* **Invoice Receipts**: Generates itemized clinic invoices with tax details, patient info, payment breakdown, and status stamps (`GET /api/invoices/{id}/pdf`).
* **Medical Prescriptions**: Generates Rx sheets with attending doctor credentials, clinical diagnosis, medication table, and signature line (`GET /api/consultations/{id}/pdf`).

---

## 4. Security & Role-Based Access Control (RBAC)

### Role Matrix

| Endpoint Area | Admin | Doctor | Receptionist | Cashier | Patient |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Authentication & Profile** (`/api/auth/*`) | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Clinic Dashboard** (`/api/dashboard/*`) | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Patient Registration** (`/api/patients/*`) | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Doctor Management & Schedules** (`/api/doctors/*`) | ✅ | ✅ | ✅ | ❌ | View |
| **Appointment Booking & Queue** (`/api/appointments/*`) | ✅ | ✅ | ✅ | ❌ | View |
| **Clinical Diagnoses & Rx** (`/api/consultations/*`) | ✅ | ✅ | View | ❌ | View |
| **Invoicing & Billing Payments** (`/api/invoices/*`) | ✅ | ❌ | View | ✅ | View |
| **PDF Document Downloads** (`*.pdf`) | ✅ | ✅ | ✅ | ✅ | Self |

### Pre-Seeded Accounts

| Username | Password | Role | Description |
| :--- | :--- | :--- | :--- |
| `admin` | `Admin@123` | `Admin` | System Administrator with full clinic privileges |
| `dr.house` | `Doctor@123` | `Doctor` | Doctor Gregory House (Internal Medicine) |
| `receptionist` | `Staff@123` | `Receptionist` | Front desk receptionist for bookings and queue |
| `cashier` | `Cashier@123` | `Cashier` | Billing and invoice payment processor |
| `patient.jvance` | `Patient@123` | `Patient` | Registered patient Johnathan Vance |

---

## 5. Testing & Quality Assurance Strategy

The solution includes automated unit and integration tests:

```powershell
# Run all test suites
dotnet test ClinicSystem.sln --logger "console;verbosity=normal"
```

1. **`ClinicSystem.UnitTests`**:
   * Aggregate invariant assertions (appointment schedule validation, state machine transitions, invoice balance calculations).
   * MediatR `ValidationBehavior` validation enforcement.
   * Command handlers conflict checks and mock repository verification.
2. **`ClinicSystem.IntegrationTests`**:
   * Uses `WebApplicationFactory<Program>` with in-memory database replacement.
   * End-to-end testing of user registration, JWT authentication, protected endpoints, dashboard aggregation, available slot computations, and PDF streaming.
