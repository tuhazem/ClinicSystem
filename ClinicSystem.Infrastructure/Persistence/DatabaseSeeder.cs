using ClinicSystem.Domain.Appointments;
using ClinicSystem.Domain.Billing;
using ClinicSystem.Domain.Doctors;
using ClinicSystem.Domain.MedicalRecords;
using ClinicSystem.Domain.Patients;
using ClinicSystem.Domain.Patients.ValueObjects;
using ClinicSystem.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Automatically apply any pending migrations
            if (context.Database.IsSqlServer())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Seed Doctors
            if (!await context.Doctors.AnyAsync())
            {
                logger.LogInformation("Seeding Doctors...");
                var doctors = new[]
                {
                    Doctor.Create("Dr. Gregory House", Specialization.InternalMedicine, "DOC-MD-001", "+1-555-0101", 200.00m, "house@clinic.org"),
                    Doctor.Create("Dr. Allison Cameron", Specialization.InternalMedicine, "DOC-MD-002", "+1-555-0102", 150.00m, "cameron@clinic.org"),
                    Doctor.Create("Dr. Robert Chase", Specialization.Cardiology, "DOC-MD-003", "+1-555-0103", 180.00m, "chase@clinic.org"),
                    Doctor.Create("Dr. Lisa Cuddy", Specialization.InternalMedicine, "DOC-MD-004", "+1-555-0104", 250.00m, "cuddy@clinic.org"),
                    Doctor.Create("Dr. Eric Foreman", Specialization.Neurology, "DOC-MD-005", "+1-555-0105", 220.00m, "foreman@clinic.org")
                };

                foreach (var doc in doctors)
                {
                    doc.SetDefaultWeeklySchedule(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 30);
                }

                await context.Doctors.AddRangeAsync(doctors);
                await context.SaveChangesAsync();
            }

            // 2. Seed Medical Services
            if (!await context.MedicalServices.AnyAsync())
            {
                logger.LogInformation("Seeding Medical Services...");
                var services = new[]
                {
                    MedicalService.Create("SRV-CONS-01", "General Medical Consultation", 150.00m, "Clinical", "Comprehensive primary health examination."),
                    MedicalService.Create("SRV-LAB-CBC", "Complete Blood Count (CBC)", 45.00m, "Laboratory", "Full blood panel analysis."),
                    MedicalService.Create("SRV-RAD-XRAY", "Chest X-Ray Digital", 85.00m, "Radiology", "High resolution chest radiography."),
                    MedicalService.Create("SRV-CARD-ECG", "12-Lead Electrocardiogram", 70.00m, "Cardiology", "Diagnostic heart rhythm evaluation."),
                    MedicalService.Create("SRV-DENT-CLN", "Comprehensive Dental Cleaning", 120.00m, "Dentistry", "Deep scaling and oral hygiene prophylaxis.")
                };
                await context.MedicalServices.AddRangeAsync(services);
                await context.SaveChangesAsync();
            }

            // 3. Seed Patients
            if (!await context.Patients.AnyAsync())
            {
                logger.LogInformation("Seeding Patients...");
                var patient1 = Patient.Register(
                    "Johnathan Vance",
                    "PAT-2026-0001",
                    new DateTime(1985, 3, 12),
                    PatientContactInfo.Create("+1-555-0201", "jvance@example.com", "742 Evergreen Terrace, Springfield")
                );
                patient1.UpdateMedicalHistory("Seasonal pollen allergy; Mild asthma managed with salbutamol.");

                var patient2 = Patient.Register(
                    "Emma Watson",
                    "PAT-2026-0002",
                    new DateTime(1994, 8, 21),
                    PatientContactInfo.Create("+1-555-0202", "emma.w@example.com", "221B Baker Street, Cityville")
                );

                var patient3 = Patient.Register(
                    "David Miller",
                    "PAT-2026-0003",
                    new DateTime(1978, 11, 4),
                    PatientContactInfo.Create("+1-555-0203", "david.m@example.com", "45 Ocean Drive, Metro City")
                );

                await context.Patients.AddRangeAsync(patient1, patient2, patient3);
                await context.SaveChangesAsync();

                // 4. Seed Appointments for seeded patients and doctors
                var house = await context.Doctors.FirstAsync(d => d.LicenseNumber == "DOC-MD-001");
                var cameron = await context.Doctors.FirstAsync(d => d.LicenseNumber == "DOC-MD-002");

                logger.LogInformation("Seeding Appointments...");
                var appt1 = Appointment.Schedule(
                    patient1.Id,
                    house.Id,
                    DateTime.UtcNow.AddHours(-2),
                    DateTime.UtcNow.AddHours(-1.5),
                    AppointmentType.GeneralConsultation,
                    "Persistent headache and facial pressure.",
                    1
                );
                appt1.Confirm();
                appt1.StartConsultation();
                appt1.Complete();

                var appt2 = Appointment.Schedule(
                    patient2.Id,
                    cameron.Id,
                    DateTime.UtcNow.AddDays(1).Date.AddHours(10),
                    DateTime.UtcNow.AddDays(1).Date.AddHours(10.5),
                    AppointmentType.RoutineCheckup,
                    "Annual wellness health screening.",
                    1
                );
                appt2.Confirm();

                var appt3 = Appointment.Schedule(
                    patient3.Id,
                    house.Id,
                    DateTime.UtcNow.AddDays(2).Date.AddHours(14),
                    DateTime.UtcNow.AddDays(2).Date.AddHours(14.5),
                    AppointmentType.FollowUp,
                    "Blood pressure monitoring review.",
                    1
                );

                await context.Appointments.AddRangeAsync(appt1, appt2, appt3);
                await context.SaveChangesAsync();

                // 5. Seed Invoices
                logger.LogInformation("Seeding Invoices...");
                var invoice1 = Invoice.Create("INV-2026-00001", patient1.Id, appt1.Id);
                invoice1.AddItem("General Medical Consultation", 1, 150.00m);
                invoice1.AddItem("Complete Blood Count (CBC)", 1, 45.00m);
                invoice1.Issue();
                invoice1.RecordPayment(195.00m, PaymentMethod.CreditCard);

                var invoice2 = Invoice.Create("INV-2026-00002", patient2.Id, appt2.Id);
                invoice2.AddItem("Annual Wellness Health Screening", 1, 150.00m);
                invoice2.Issue();

                await context.Invoices.AddRangeAsync(invoice1, invoice2);
                await context.SaveChangesAsync();

                // 6. Seed Consultation Records & Prescriptions
                logger.LogInformation("Seeding Consultation Records...");
                var consult = ConsultationRecord.Create(
                    patient1.Id,
                    house.Id,
                    "Severe headache, facial pressure around maxillary sinuses, low-grade fever for 4 days.",
                    "Acute Bacterial Rhinosinusitis",
                    "7-day antibiotic course, nasal corticosteroids, rest and hydration.",
                    "Patient advised to return if fever exceeds 39C or symptoms persist past 7 days.",
                    appt1.Id
                );
                consult.AddPrescription("Amoxicillin-Clavulanate 875mg", "1 tablet", "Twice daily after meals", 7, "Complete full antibiotic course.");
                consult.AddPrescription("Fluticasone Nasal Spray 50mcg", "2 sprays/nostril", "Once daily in the morning", 14, "Use after clearing nasal passages.");

                // 7. Seed Default Security Users
                logger.LogInformation("Seeding Users...");
                var adminUser = ClinicSystem.Domain.Users.User.Create("admin", "admin@clinic.com", BCrypt.Net.BCrypt.HashPassword("Admin@123"), "Admin");
                var houseUser = ClinicSystem.Domain.Users.User.Create("dr.house", "house@clinic.org", BCrypt.Net.BCrypt.HashPassword("Doctor@123"), "Doctor", associatedDoctorId: house.Id);
                var staffUser = ClinicSystem.Domain.Users.User.Create("receptionist", "staff@clinic.com", BCrypt.Net.BCrypt.HashPassword("Staff@123"), "Receptionist");
                var cashierUser = ClinicSystem.Domain.Users.User.Create("cashier", "cashier@clinic.com", BCrypt.Net.BCrypt.HashPassword("Cashier@123"), "Cashier");
                var patientUser = ClinicSystem.Domain.Users.User.Create("patient.jvance", "jvance@example.com", BCrypt.Net.BCrypt.HashPassword("Patient@123"), "Patient", associatedPatientId: patient1.Id);

                await context.Users.AddRangeAsync(adminUser, houseUser, staffUser, cashierUser, patientUser);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database seed verification complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}
