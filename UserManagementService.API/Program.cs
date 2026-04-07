using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using UserManagementService.API.Middleware;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure;
using UserManagementService.Infrastructure.Persistence;
using UserManagementService.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add Infrastructure services (DbContext + Identity + JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserManagementService.Application.Commands.Auth.LoginCommand).Assembly));

// ✅ Authorization Policies Configuration
builder.Services.AddAuthorization(options =>
{
    // Super Admin Only - requires IsSuperAdmin claim = "True"
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireClaim("IsSuperAdmin", "True"));

    // Super Admin OR any user who has the user-management permission
    // This allows Tenant Admins who were granted user management access
    options.AddPolicy("SuperAdminOrTenantAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("IsSuperAdmin", "True") ||
            ctx.User.HasClaim("Permission", "USERMAN_USERS_CREATE") ||
            ctx.User.HasClaim("Permission", "USERMAN_USERS_UPDATE") ||
            ctx.User.IsInRole("Tenant Admin") ||
            ctx.User.IsInRole("Super Admin")
        ));

});

// Add controllers
builder.Services.AddControllers();

//Health Checks
builder.Services.AddHealthChecks();
// Add OpenAPI - .NET 10 Built-in
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Build the app
var app = builder.Build();

// Use custom global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 🌱 Seed Database (Run on startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Run seeding
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);

        Console.WriteLine("✅ Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding error: {ex.Message}");
        // Don't crash the app if seeding fails - log and continue
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Built-in OpenAPI endpoint at /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use Authentication & Authorization (Order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();