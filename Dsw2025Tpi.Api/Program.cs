using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // -------------------------------------------------------------
        // 1️⃣ Crear el builder de la aplicación
        // -------------------------------------------------------------
        var builder = WebApplication.CreateBuilder(args);

        // Agregar soporte para controladores (API REST)
        builder.Services.AddControllers();

        // Habilitar generación de endpoints para exploración de API
        builder.Services.AddEndpointsApiExplorer();

        // -------------------------------------------------------------
        // 2️⃣ Configuración de Swagger (documentación de la API)
        // -------------------------------------------------------------
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Dsw2025Tpi", Version = "v1" });

            // Definición de seguridad para usar JWT en Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese el token JWT en el formato: Bearer {tu token}"
            });

            // Aplicar seguridad global a todos los endpoints
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Agregar Health Checks (verificación de salud de la API)
        builder.Services.AddHealthChecks();

        // -------------------------------------------------------------
        // 3️⃣ Configuración de bases de datos
        // -------------------------------------------------------------
        // Base de datos principal (productos, órdenes, clientes)
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

        // Base de datos de autenticación (usuarios y roles de Identity)
        builder.Services.AddDbContext<AuthenticateContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

        // -------------------------------------------------------------
        // 4️⃣ Configuración de Identity (manejo de usuarios y roles)
        // -------------------------------------------------------------
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthenticateContext>() // Usa AuthDb para usuarios y roles
            .AddDefaultTokenProviders();                     // Tokens para recuperación de cuenta, etc.

        // -------------------------------------------------------------
        // 5️⃣ Registro de servicios propios de la aplicación
        // -------------------------------------------------------------
        builder.Services.AddScoped<IAuthService, AuthService>(); // Servicio de autenticación
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>(); // Generación de tokens JWT
        builder.Services.AddScoped<IProductsManagementService, ProductsManagementService>();
        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddScoped<IOrdersManagementService, OrdersManagementService>();

        // -------------------------------------------------------------
        // 6️⃣ Configuración de autenticación con JWT
        // -------------------------------------------------------------
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new Exception("Missing JWT Key"));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,              // Validar quién emitió el token
                ValidateAudience = true,            // Validar para quién es el token
                ValidateLifetime = true,            // Validar que no esté vencido
                ValidateIssuerSigningKey = true,    // Validar firma del token
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero           // No permitir tiempo extra tras vencimiento
            };
        });

        // Registrar middleware global para manejo de excepciones
        builder.Services.AddTransient<GlobalExceptionMiddleware>();

        // -------------------------------------------------------------
        // 7️⃣ Construir la aplicación
        // -------------------------------------------------------------
        var app = builder.Build();

        // -------------------------------------------------------------
        // 8️⃣ Seeding inicial: cargar datos y usuario admin desde appsettings.json
        // -------------------------------------------------------------
        using (var scope = app.Services.CreateScope())
        {
            // Sembrar datos de productos/clientes en la base principal
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.SeedDatabase();

            // Crear roles y usuario administrador si no existen
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = builder.Configuration;

            // Crear roles (Admin, User)
            var roles = config.GetSection("Roles").Get<string[]>();
            if (roles != null)
            {
                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Crear usuario admin
            var adminConfig = config.GetSection("AdminUser");
            string adminEmail = adminConfig["Email"]!;
            string adminUsername = adminConfig["Username"]!;
            string adminPassword = adminConfig["Password"]!;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newAdmin, "Admin"); // Asignar rol Admin
            }
        }

        // -------------------------------------------------------------
        // 9️⃣ Configuración del pipeline de la aplicación
        // -------------------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();      // Documentación Swagger
            app.UseSwaggerUI();    // Interfaz Swagger UI
        }

        app.UseMiddleware<GlobalExceptionMiddleware>(); // Captura errores globales
        app.UseHttpsRedirection();                      // Forzar HTTPS

        app.UseAuthentication();   // Habilitar autenticación JWT
        app.UseAuthorization();    // Habilitar validación de roles

        app.MapControllers();      // Mapear controladores (rutas API)
        app.MapHealthChecks("/healthcheck"); // Endpoint para verificar salud del servicio

        // Iniciar la aplicación
        await app.RunAsync();
    }
}