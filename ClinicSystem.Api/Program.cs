using ClinicSystem.Api.Middlewares;
using ClinicSystem.Application;
using ClinicSystem.Infrastructure;
using ClinicSystem.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// 2. Application & Infrastructure layers dependency injection
builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Multi-tier Caching: .NET 9 HybridCache (L1 Memory + L2 Redis backing)
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});
#pragma warning restore EXTEXP0018

// 4. OpenAPI 3.0 Document Generation
builder.Services.AddOpenApi();

var app = builder.Build();

// 5. Automatic Database Migration & Demo Data Seeding
await DatabaseSeeder.SeedAsync(app.Services);

// 6. Custom Enterprise Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// 7. Interactive API Documentation
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Clinic System Enterprise API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    // Convenient redirect from root to API documentation
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
