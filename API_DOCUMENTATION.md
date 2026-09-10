# 📡 Clinic Management System - Complete API Reference & Guide

Welcome to the official REST API documentation for the **Clinic Management System**. This guide provides endpoint descriptions, required headers, authentication details, JSON request/response bodies, cURL examples, and end-to-end user workflows.

---

## 📑 Table of Contents
- [🌐 General Information & Base URL](#-general-information--base-url)
- [🔐 Authentication & Authorization Header](#-authentication--authorization-header)
- [⚠️ Error Handling (RFC 7807)](#-error-handling-rfc-7807)
- [1. Authentication Module (`/api/auth`)](#1-authentication-module-apiauth)
- [2. Clinic Dashboard & Analytics (`/api/dashboard`)](#2-clinic-dashboard--analytics-apidashboard)
- [3. Patients Module (`/api/patients`)](#3-patients-module-apipatients)
- [4. Doctors & Availability Module (`/api/doctors`)](#4-doctors--availability-module-apidoctors)
- [5. Appointments & Slot Engine (`/api/appointments`)](#5-appointments--slot-engine-apiappointments)
- [6. Medical Services Catalog (`/api/services`)](#6-medical-services-catalog-apiservices)
- [7. Medical Records & Prescriptions (`/api/consultations`)](#7-medical-records--prescriptions-apiconsultations)
- [8. Billing & Invoices (`/api/invoices`)](#8-billing--invoices-apiinvoices)
- [🔄 End-to-End Workflow Scenario](#-end-to-end-workflow-scenario)

---

## 🌐 General Information & Base URL

* **Development Base URL**: `https://localhost:7198` (or `http://localhost:5266`)
* **Interactive Scalar Docs**: `https://localhost:7198/scalar/v1`
* **OpenAPI 3.0 Spec**: `https://localhost:7198/openapi/v1.json`
* **Content Type**: `application/json` (or `application/pdf` for document downloads)

---

## 🔐 Authentication & Authorization Header

Protected endpoints require a valid JWT token sent in the `Authorization` header:

```http
Authorization: Bearer <your_jwt_access_token>
```

### Pre-Seeded Test Credentials

| Username | Password | Role |
| :--- | :--- | :--- |
| `admin` | `Admin@123` | `Admin` |
| `dr.house` | `Doctor@123` | `Doctor` |
| `receptionist` | `Staff@123` | `Receptionist` |
| `cashier` | `Cashier@123` | `Cashier` |
| `patient.jvance` | `Patient@123` | `Patient` |

---

## ⚠️ Error Handling (RFC 7807)

When validation or business rules fail, the API responds with a standard `application/problem+json` format:

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "ScheduledStartTimeUtc": [
      "Start time must be in the future."
    ],
    "PatientId": [
      "Patient ID is required."
    ]
  }
}
```

---

## 1. Authentication Module (`/api/auth`)

### 1.1 Login User
Authenticates credentials and returns a JWT access token along with a refresh token and user roles.

* **Endpoint**: `POST /api/auth/login`
* **Auth**: None (Public)

#### Request:
```bash
curl -X POST https://localhost:7198/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "dr.house",
    "password": "Doctor@123"
  }'
```

#### Response (`200 OK`):
```json
{
  "userId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "username": "dr.house",
  "email": "house@clinic.org",
  "role": "Doctor",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "4Z1uQ3F...",
  "associatedDoctorId": "2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90",
  "associatedPatientId": null
}
```

---

### 1.2 Register User
Creates a new staff or patient account with specified role (`Admin`, `Doctor`, `Receptionist`, `Cashier`, `Patient`).

* **Endpoint**: `POST /api/auth/register`
* **Auth**: None (Public)

#### Request:
```bash
curl -X POST https://localhost:7198/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "nurse.joy",
    "email": "joy@clinic.com",
    "password": "Password123!",
    "role": "Receptionist"
  }'
```

#### Response (`201 Created`):
```json
{
  "userId": "7c1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6e",
  "username": "nurse.joy",
  "email": "joy@clinic.com",
  "role": "Receptionist",
  "accessToken": "eyJhbGciOiJIUzI1NiIsIn...",
  "refreshToken": "8F3vR2K...",
  "associatedDoctorId": null,
  "associatedPatientId": null
}
```

---

### 1.3 Get Current User Profile
Returns claims, roles, and contextual IDs for the bearer token holder.

* **Endpoint**: `GET /api/auth/me`
* **Auth**: `Bearer <token>`

#### Request:
```bash
curl -X GET https://localhost:7198/api/auth/me \
  -H "Authorization: Bearer <your_access_token>"
```

#### Response (`200 OK`):
```json
{
  "userId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "username": "dr.house",
  "email": "house@clinic.org",
  "role": "Doctor",
  "doctorId": "2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90",
  "patientId": null
}
```

---

## 2. Clinic Dashboard & Analytics (`/api/dashboard`)

### 2.1 Get Real-Time Dashboard Summary
Returns instant operational and financial KPIs including today's appointment statuses, live doctor queues, revenue collected, and top medical services.

* **Endpoint**: `GET /api/dashboard/summary?date=YYYY-MM-DD`
* **Auth**: None / Staff

#### Request:
```bash
curl -X GET "https://localhost:7198/api/dashboard/summary"
```

#### Response (`200 OK`):
```json
{
  "appointments": {
    "totalToday": 14,
    "scheduled": 4,
    "confirmed": 5,
    "inProgress": 2,
    "completed": 2,
    "cancelled": 1
  },
  "financials": {
    "todayRevenue": 890.00,
    "monthRevenue": 14350.00,
    "totalOutstandingBalance": 1250.00,
    "unpaidInvoicesCount": 6
  },
  "totalActivePatients": 340,
  "totalActiveDoctors": 5,
  "topServices": [
    {
      "serviceName": "General Medical Consultation",
      "category": "Clinical",
      "usageCount": 42,
      "totalRevenue": 6300.00
    },
    {
      "serviceName": "Complete Blood Count (CBC)",
      "category": "Laboratory",
      "usageCount": 28,
      "totalRevenue": 1260.00
    },
    {
      "serviceName": "Chest X-Ray Digital",
      "category": "Radiology",
      "usageCount": 15,
      "totalRevenue": 1275.00
    }
  ]
}
```

---

## 3. Patients Module (`/api/patients`)

### 3.1 Register Patient
* **Endpoint**: `POST /api/patients`

#### Request:
```json
{
  "fullName": "Sarah Jenkins",
  "phoneNumber": "+123456789012",
  "dateOfBirth": "1992-07-24T00:00:00.000Z",
  "email": "sarah.jenkins@example.com",
  "address": "452 Elm Street, Springfield"
}
```

#### Response (`201 Created`):
```json
{
  "patientId": "e3a89045-8120-413e-862d-0570b556f8f7"
}
```

---

### 3.2 Get Patient by ID (Cached)
* **Endpoint**: `GET /api/patients/{id}`

#### Response (`200 OK`):
```json
{
  "id": "e3a89045-8120-413e-862d-0570b556f8f7",
  "fullName": "Sarah Jenkins",
  "medicalRecordNumber": "PAT-2026-0004",
  "dateOfBirth": "1992-07-24T00:00:00",
  "phoneNumber": "+123456789012",
  "email": "sarah.jenkins@example.com",
  "address": "452 Elm Street, Springfield",
  "medicalHistory": "No known allergies."
}
```

---

## 4. Doctors & Availability Module (`/api/doctors`)

### 4.1 Register Doctor
* **Specialization Enum**:
  * `0`: GeneralPractice, `1`: Cardiology, `2`: Dermatology, `3`: Pediatrics, `4`: Orthopedics, `5`: Neurology, `6`: Gynecology, `7`: Ophthalmology, `8`: Dentistry, `9`: InternalMedicine, `10`: Psychiatry.

* **Endpoint**: `POST /api/doctors`

#### Request:
```json
{
  "fullName": "Dr. Allison Cameron",
  "specialization": 9,
  "licenseNumber": "DOC-MD-002",
  "phoneNumber": "+1-555-0102",
  "consultationFee": 150.00,
  "email": "cameron@clinic.org"
}
```

#### Response (`201 Created`):
```json
{
  "doctorId": "5d2b1f89-913a-4e20-bf4d-17a42b10e95c"
}
```

---

### 4.2 List All Doctors (Cached via HybridCache)
* **Endpoint**: `GET /api/doctors`

#### Response (`200 OK`):
```json
[
  {
    "id": "2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90",
    "fullName": "Dr. Gregory House",
    "specialization": 9,
    "specializationName": "InternalMedicine",
    "licenseNumber": "DOC-MD-001",
    "phoneNumber": "+1-555-0101",
    "email": "house@clinic.org",
    "consultationFee": 200.00,
    "isActive": true
  }
]
```

---

## 5. Appointments & Slot Engine (`/api/appointments`)

### 5.1 Get Available Slots for Doctor (Conflict-Free Calculation)
Computes real-time available time slots based on doctor shift schedules and existing non-cancelled bookings.

* **Endpoint**: `GET /api/appointments/doctor/{doctorId}/available-slots?date=YYYY-MM-DD`

#### Request:
```bash
curl -X GET "https://localhost:7198/api/appointments/doctor/2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90/available-slots?date=2026-09-15"
```

#### Response (`200 OK`):
```json
[
  {
    "startTimeUtc": "2026-09-15T09:00:00Z",
    "endTimeUtc": "2026-09-15T09:30:00Z",
    "isAvailable": true
  },
  {
    "startTimeUtc": "2026-09-15T09:30:00Z",
    "endTimeUtc": "2026-09-15T10:00:00Z",
    "isAvailable": false
  },
  {
    "startTimeUtc": "2026-09-15T10:00:00Z",
    "endTimeUtc": "2026-09-15T10:30:00Z",
    "isAvailable": true
  }
]
```

---

### 5.2 Book Appointment
* **AppointmentType Enum**: `1`: GeneralConsultation, `2`: FollowUp, `3`: Emergency, `4`: RoutineCheckup, `5`: Procedure.
* Automatically assigns the daily ascending Queue Number.
* Rejects conflicting overlaps with `409 Conflict`.

* **Endpoint**: `POST /api/appointments/book`

#### Request:
```json
{
  "patientId": "e3a89045-8120-413e-862d-0570b556f8f7",
  "doctorId": "2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90",
  "scheduledStartTimeUtc": "2026-09-15T09:00:00Z",
  "scheduledEndTimeUtc": "2026-09-15T09:30:00Z",
  "type": 1,
  "reasonForVisit": "Persistent fever and headache for 3 days."
}
```

#### Response (`201 Created`):
```json
{
  "appointmentId": "f48b8120-0570-413e-862d-e3a89045b556"
}
```

---

### 5.3 Update Appointment Status
* **Status Enum**: `1`: Scheduled, `2`: Confirmed, `3`: InProgress, `4`: Completed, `5`: Cancelled.

* **Endpoint**: `PATCH /api/appointments/{id}/status`

#### Request:
```json
{
  "newStatus": 2,
  "reason": "Confirmed by patient via SMS."
}
```

#### Response (`200 OK`):
```json
{
  "message": "Status updated successfully."
}
```

---

## 6. Medical Services Catalog (`/api/services`)

### 6.1 Create Medical Service
* **Endpoint**: `POST /api/services`

#### Request:
```json
{
  "code": "SRV-CBC-01",
  "name": "Complete Blood Count (CBC)",
  "basePrice": 45.00,
  "category": "Laboratory",
  "description": "Comprehensive diagnostic full blood count panel."
}
```

#### Response (`201 Created`):
```json
{
  "serviceId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
}
```

---

## 7. Medical Records & Prescriptions (`/api/consultations`)

### 7.1 Record Consultation & Prescriptions
* **Endpoint**: `POST /api/consultations`

#### Request:
```json
{
  "patientId": "e3a89045-8120-413e-862d-0570b556f8f7",
  "doctorId": "2c9b4e78-3a1f-4b45-9a8d-71b3e94a8f90",
  "appointmentId": "f48b8120-0570-413e-862d-e3a89045b556",
  "symptoms": "High grade fever (38.8C), severe maxillary facial pain, nasal congestion.",
  "diagnosis": "Acute Bacterial Sinusitis",
  "treatmentPlan": "7-day oral antibiotic therapy, saline nasal irrigation, rest.",
  "notes": "Follow up if no clinical improvement after 5 days.",
  "prescriptions": [
    {
      "medicationName": "Amoxicillin-Clavulanate 875mg",
      "dosage": "1 tablet",
      "frequency": "Twice daily after meals",
      "durationInDays": 7,
      "instructions": "Complete full antibiotic course."
    },
    {
      "medicationName": "Paracetamol 500mg",
      "dosage": "1-2 tablets",
      "frequency": "Every 6 hours as needed for fever/pain",
      "durationInDays": 3,
      "instructions": "Do not exceed 4000mg/day."
    }
  ]
}
```

#### Response (`201 Created`):
```json
{
  "consultationId": "8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"
}
```

---

### 7.2 Download Printable Prescription PDF (QuestPDF Engine)
Streams a professional prescription document with attending doctor credentials, diagnosis, structured Rx table, and signature line.

* **Endpoint**: `GET /api/consultations/{id}/pdf`
* **Response Header**: `Content-Type: application/pdf`

#### Request:
```bash
curl -X GET "https://localhost:7198/api/consultations/8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e/pdf" \
  --output "Prescription_Sarah_Jenkins.pdf"
```

---

## 8. Billing & Invoices (`/api/invoices`)

### 8.1 Create Patient Invoice
* **Endpoint**: `POST /api/invoices`

#### Request:
```json
{
  "patientId": "e3a89045-8120-413e-862d-0570b556f8f7",
  "appointmentId": "f48b8120-0570-413e-862d-e3a89045b556",
  "items": [
    {
      "description": "Specialist Medical Consultation",
      "quantity": 1,
      "unitPrice": 200.00
    },
    {
      "description": "Complete Blood Count (CBC) Lab Test",
      "quantity": 1,
      "unitPrice": 45.00
    }
  ]
}
```

#### Response (`201 Created`):
```json
{
  "invoiceId": "3f4a5b6c-7d8e-9f0a-1b2c-3d4e5f6a7b8c"
}
```

---

### 8.2 Record Invoice Payment
* **PaymentMethod Enum**: `1`: Cash, `2`: CreditCard, `3`: Insurance, `4`: BankTransfer.

* **Endpoint**: `POST /api/invoices/{id}/pay`

#### Request:
```json
{
  "amount": 245.00,
  "paymentMethod": 2
}
```

#### Response (`200 OK`):
```json
{
  "message": "Payment recorded successfully."
}
```

---

### 8.3 Download Official Invoice PDF Receipt (QuestPDF Engine)
Streams a branded receipt with clinic info, patient details, breakdown of itemized charges, and payment status stamp (`PAID` / `UNPAID`).

* **Endpoint**: `GET /api/invoices/{id}/pdf`
* **Response Header**: `Content-Type: application/pdf`

#### Request:
```bash
curl -X GET "https://localhost:7198/api/invoices/3f4a5b6c-7d8e-9f0a-1b2c-3d4e5f6a7b8c/pdf" \
  --output "Invoice_INV-2026-00001.pdf"
```

---

## 🔄 End-to-End Workflow Scenario

Here is the standard clinical encounter lifecycle from patient intake to billing:

```
Step 1: Patient Intake
  POST /api/patients ─────────────────────────────► Returns patientId

Step 2: Slot Lookup & Booking
  GET /api/appointments/doctor/{docId}/available-slots
  POST /api/appointments/book ────────────────────► Returns appointmentId + Queue #

Step 3: Consultation & Prescription
  POST /api/consultations ────────────────────────► Returns consultationId
  GET  /api/consultations/{id}/pdf ───────────────► Downloads Printable Rx PDF

Step 4: Invoicing & Payment
  POST /api/invoices ─────────────────────────────► Returns invoiceId
  POST /api/invoices/{id}/pay ────────────────────► Records Payment (Cash/Card)
  GET  /api/invoices/{id}/pdf ────────────────────► Downloads Official Receipt PDF
```
