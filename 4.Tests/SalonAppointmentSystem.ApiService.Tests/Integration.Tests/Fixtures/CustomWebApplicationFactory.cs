using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.Common.Settings;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;
using SalonAppointmentSystem.ApiService.Infrastructure.Services;
using SalonAppointmentSystem.ApiService.Presentation.Authorization;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;

/// <summary>
/// Factory personalizada para tests de integración
/// Configura la aplicación con base de datos en memoria
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Agregar configuración para tests
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:salondb"] = "InMemory",
                ["JwtSettings:SecretKey"] = "TestSecretKeyMuyLargaParaPruebasDeIntegracion2024!@#$%",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover servicios existentes que queremos reemplazar
            RemoveExistingServices(services);

            // Agregar DbContext con InMemory
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Registrar Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Registrar servicios
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEstacionRepository, EstacionRepository>();

            // Registrar AutoMapper
            services.AddAutoMapper(typeof(SalonAppointmentSystem.ApiService.Application.Mappings.UserMappingProfile).Assembly);

            // Configurar JWT
            var jwtSettings = new JwtSettings
            {
                SecretKey = "TestSecretKeyMuyLargaParaPruebasDeIntegracion2024!@#$%",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };
            services.Configure<JwtSettings>(opts =>
            {
                opts.SecretKey = jwtSettings.SecretKey;
                opts.Issuer = jwtSettings.Issuer;
                opts.Audience = jwtSettings.Audience;
                opts.AccessTokenExpirationMinutes = jwtSettings.AccessTokenExpirationMinutes;
                opts.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
            });

            // Configurar autenticación JWT
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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Registrar authorization handler
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, OperationAuthorizationHandler>();

            // Configurar políticas
            services.AddAuthorizationBuilder()
                .AddPolicy(AppPolicies.RequireAdmin, policy =>
                    policy.RequireRole(ApplicationRoles.Admin))
                .AddPolicy(AppPolicies.RequireAdminOrBarbero, policy =>
                    policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Barbero))
                .AddPolicy(AppPolicies.RequireAuthenticated, policy =>
                    policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente))
                .AddOperationPolicies();

            // Construir el service provider y seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureCreated();
            SeedTestData(scopedServices).GetAwaiter().GetResult();
        });
    }

    private static void RemoveExistingServices(IServiceCollection services)
    {
        // Remover DbContext
        var dbContextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

        services.RemoveAll(typeof(ApplicationDbContext));
        services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

        // Remover Identity
        services.RemoveAll(typeof(UserManager<ApplicationUser>));
        services.RemoveAll(typeof(RoleManager<IdentityRole>));
        services.RemoveAll(typeof(SignInManager<ApplicationUser>));

        // Remover servicios de autenticación
        services.RemoveAll(typeof(IAuthService));
        services.RemoveAll(typeof(IUserService));
    }

    /// <summary>
    /// Seed de datos iniciales para tests
    /// </summary>
    private static async Task SeedTestData(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Crear roles si no existen
        var roles = new[] { ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Crear usuario Admin para tests
        if (await userManager.FindByEmailAsync(TestUsers.AdminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = TestUsers.AdminEmail,
                Email = TestUsers.AdminEmail,
                NombreCompleto = "Admin Test",
                Activo = true,
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, TestUsers.DefaultPassword);
            await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
        }

        // Crear usuario Barbero para tests
        if (await userManager.FindByEmailAsync(TestUsers.BarberoEmail) == null)
        {
            var barbero = new ApplicationUser
            {
                UserName = TestUsers.BarberoEmail,
                Email = TestUsers.BarberoEmail,
                NombreCompleto = "Barbero Test",
                Activo = true,
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow
            };
            await userManager.CreateAsync(barbero, TestUsers.DefaultPassword);
            await userManager.AddToRoleAsync(barbero, ApplicationRoles.Barbero);
        }

        // Crear usuario Cliente para tests
        if (await userManager.FindByEmailAsync(TestUsers.ClienteEmail) == null)
        {
            var cliente = new ApplicationUser
            {
                UserName = TestUsers.ClienteEmail,
                Email = TestUsers.ClienteEmail,
                NombreCompleto = "Cliente Test",
                Activo = true,
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow
            };
            await userManager.CreateAsync(cliente, TestUsers.DefaultPassword);
            await userManager.AddToRoleAsync(cliente, ApplicationRoles.Cliente);
        }

        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Constantes de usuarios de prueba
/// </summary>
public static class TestUsers
{
    public const string AdminEmail = "admin.test@barberia.com";
    public const string BarberoEmail = "barbero.test@barberia.com";
    public const string ClienteEmail = "cliente.test@barberia.com";
    public const string DefaultPassword = "Test123!";
}

