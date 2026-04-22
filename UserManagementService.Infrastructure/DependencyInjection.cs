using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Text;
using UserManagementService.Application.Services;
using UserManagementService.Application.Services.Password;
using UserManagementService.Domain.Configuration;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;
using UserManagementService.Infrastructure.Services.AppActions;
using UserManagementService.Infrastructure.Services.AppPermissions;
using UserManagementService.Infrastructure.Services.Apps;
using UserManagementService.Infrastructure.Services.Email;
using UserManagementService.Infrastructure.Services.Messaging;
using UserManagementService.Infrastructure.Services.Logging;
using UserManagementService.Infrastructure.Services.Pages;
using UserManagementService.Infrastructure.Services.FileStorage;
using UserManagementService.Infrastructure.Services.Roles;
using UserManagementService.Infrastructure.Services.Token;
using UserManagementService.Infrastructure.Services.User;

namespace UserManagementService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT Key not configured")))
            };
        });

        services.AddAuthorization();

        // Resilience pipelines (Polly v8)
        // "smtp" / "resend": CircuitBreaker → Retry → Timeout per attempt
        // "rabbitmq-connect": Retry with exponential backoff
        // "rabbitmq-publish": CircuitBreaker → Retry
        services.AddResiliencePipeline("smtp", builder =>
            builder
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    FailureRatio = 0.5,
                    BreakDuration = TimeSpan.FromSeconds(60),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddTimeout(TimeSpan.FromSeconds(10)));

        services.AddResiliencePipeline("resend", builder =>
            builder
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    FailureRatio = 0.5,
                    BreakDuration = TimeSpan.FromSeconds(60),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddTimeout(TimeSpan.FromSeconds(10)));

        // Connection retry only — no circuit breaker so startup keeps retrying
        services.AddResiliencePipeline("rabbitmq-connect", builder =>
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = false,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddTimeout(TimeSpan.FromSeconds(5)));

        // Publish: circuit breaker opens after sustained failures to prevent pile-up
        services.AddResiliencePipeline("rabbitmq-publish", builder =>
            builder
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    FailureRatio = 0.5,
                    BreakDuration = TimeSpan.FromSeconds(120),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Linear,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                }));

        // RabbitMQ — persistent connection (Singleton) + publisher (Scoped)
        // Singleton: one connection shared for the entire app lifetime
        // Scoped: one EventPublisher instance per HTTP request
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<ILogPublisher, LogPublisher>();

        // Email Services
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailProvider, SmtpEmailProvider>();

        // Password Generator Service
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();

        // OTP Service
        services.AddScoped<IOtpService, OtpService>();

        // Token Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRefreshTokenStorageService, RefreshTokenStorageService>();

        // User Services
        services.AddScoped<IUserService, UserService>();

        // Permission Resolver Service
        services.AddScoped<IAppPermissionResolverService, AppPermissionResolverService>();

        // App Services
        services.AddScoped<IAppService, AppService>();

        // Page Services
        services.AddScoped<IPageService, PageService>();

        // AppAction Services
        services.AddScoped<IAppActionService, AppActionService>();

        // AppPermission Services
        services.AddScoped<IAppPermissionService, AppPermissionService>();

        // Role Services
        services.AddScoped<IRoleService, RoleService>();

        // File Storage Service
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}