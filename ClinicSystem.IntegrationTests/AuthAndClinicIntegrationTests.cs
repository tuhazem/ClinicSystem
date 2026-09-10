using ClinicSystem.Application.Appointments.Queries.GetDoctorAvailableSlots;
using ClinicSystem.Application.Auth.Commands.Login;
using ClinicSystem.Application.Auth.Commands.RegisterUser;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Application.Doctors.Queries;
using ClinicSystem.Domain.Billing;
using ClinicSystem.Domain.MedicalRecords;
using ClinicSystem.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ClinicSystem.IntegrationTests;

public class AuthAndClinicIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthAndClinicIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync(string role = "Admin")
    {
        var username = $"test_{role.ToLower()}_{Guid.NewGuid():N}"[..12];
        var regCommand = new RegisterUserCommand(
            Username: username,
            Email: $"{username}@test.com",
            Password: "Password123!",
            Role: role
        );

        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", regCommand);
        regResponse.EnsureSuccessStatusCode();
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", regResult!.AccessToken);
        return client;
    }

    [Fact]
    public async Task UnauthenticatedAccess_ToSecuredEndpoint_ShouldReturn401Unauthorized()
    {
        var response = await _client.GetAsync("/api/dashboard/summary");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthFlow_RegisterAndLogin_ShouldReturnValidJwtToken()
    {
        // 1. Register
        var username = $"user_{Guid.NewGuid():N}"[..12];
        var registerCommand = new RegisterUserCommand(
            Username: username,
            Email: $"{username}@test.com",
            Password: "Password123!",
            Role: "Receptionist"
        );

        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var regBody = await regResponse.Content.ReadAsStringAsync();
        regResponse.StatusCode.Should().Be(HttpStatusCode.Created, because: regBody);

        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        regResult.Should().NotBeNull();
        regResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        regResult.Role.Should().Be("Receptionist");

        // 2. Login
        var loginCommand = new LoginCommand(username, "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        loginResult.Should().NotBeNull();
        loginResult!.AccessToken.Should().NotBeNullOrWhiteSpace();

        // 3. Authenticated endpoint
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var meResponse = await _client.SendAsync(request);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DashboardSummary_ShouldReturnSummaryStatistics()
    {
        var client = await GetAuthenticatedClientAsync("Admin");
        var response = await client.GetAsync("/api/dashboard/summary");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        summary.Should().NotBeNull();
        summary!.Appointments.Should().NotBeNull();
        summary.Financials.Should().NotBeNull();
    }

    [Fact]
    public async Task AvailableSlots_ShouldReturnSlotsForDoctor()
    {
        var client = await GetAuthenticatedClientAsync("Receptionist");
        // Fetch seeded doctors
        var doctorsResponse = await client.GetAsync("/api/doctors");
        doctorsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var doctors = await doctorsResponse.Content.ReadFromJsonAsync<List<DoctorDto>>();
        doctors.Should().NotBeNull().And.NotBeEmpty();

        var doctorId = doctors![0].Id;
        var date = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

        var slotsResponse = await client.GetAsync($"/api/appointments/doctor/{doctorId}/available-slots?date={date}");
        slotsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var slots = await slotsResponse.Content.ReadFromJsonAsync<List<AvailableSlotDto>>();
        slots.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public async Task InvoicePdfDownload_ShouldReturnValidPdfStream()
    {
        var client = await GetAuthenticatedClientAsync("Cashier");
        // Obtain seeded invoice ID
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invoice = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Invoices);

        if (invoice != null)
        {
            var pdfResponse = await client.GetAsync($"/api/invoices/{invoice.Id}/pdf");
            pdfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            pdfResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

            var bytes = await pdfResponse.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
            // PDF signature bytes %PDF
            bytes[0..4].Should().Equal((byte)'%', (byte)'P', (byte)'D', (byte)'F');
        }
    }

    [Fact]
    public async Task PrescriptionPdfDownload_ShouldReturnValidPdfStream()
    {
        var client = await GetAuthenticatedClientAsync("Doctor");
        // Obtain seeded consultation record ID
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var record = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.ConsultationRecords);

        if (record != null)
        {
            var pdfResponse = await client.GetAsync($"/api/consultations/{record.Id}/pdf");
            pdfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            pdfResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

            var bytes = await pdfResponse.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
            // PDF signature bytes %PDF
            bytes[0..4].Should().Equal((byte)'%', (byte)'P', (byte)'D', (byte)'F');
        }
    }
}
