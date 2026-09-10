using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Infrastructure.Persistence;
using ClinicSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClinicSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. SQL Server Database Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));

        // 2. Distributed L2 Redis Caching for HybridCache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "ClinicSystem_";
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints = { redisConnection },
                    AbortOnConnectFail = false,
                    ConnectTimeout = 1000,
                    SyncTimeout = 1000
                };
            });
        }

        // 3. Unit of Work & Repositories Registration
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IMedicalServiceRepository, MedicalServiceRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();

        // 4. Analytics & Document Reporting Services
        services.AddScoped<IClinicAnalyticsService, ClinicSystem.Infrastructure.Services.ClinicAnalyticsService>();
        services.AddScoped<IPdfReportService, ClinicSystem.Infrastructure.Services.PdfReportService>();

        // 5. Authentication & Security Services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, ClinicSystem.Infrastructure.Services.PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, ClinicSystem.Infrastructure.Services.JwtTokenGenerator>();

        return services;
    }

    public static IServiceCollection AddInfrastructureSerivices(this IServiceCollection services, IConfiguration configuration)
        => services.AddInfrastructureServices(configuration);
}
