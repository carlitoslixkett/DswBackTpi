// 🔁 Imports de namespaces propios y del framework
using Dsw2025Tpi.Api.Middleware;                         // Middleware personalizado para manejar excepciones globales
using Dsw2025Tpi.Application.Interfaces;                 // Interfaces de servicios de la capa de aplicación
using Dsw2025Tpi.Application.Services;                   // Implementaciones de servicios de la capa de aplicación
using Dsw2025Tpi.Data;                                   // DbContext principal y clases relacionadas a datos
using Dsw2025Tpi.Data.Helpers;                           // Métodos auxiliares como el seeding
using Dsw2025Tpi.Data.Repositories;                      // Repositorio genérico
using Dsw2025Tpi.Domain.Interfaces;                      // Interfaces del dominio
using Microsoft.AspNetCore.Authentication.JwtBearer;     // JWT para autenticación
using Microsoft.AspNetCore.Identity;                     // Identity para manejo de usuarios/roles
using Microsoft.EntityFrameworkCore;                     // Entity Framework Core
using Microsoft.IdentityModel.Tokens;                    // Validación y firma de tokens
using Microsoft.OpenApi.Models;                          // Swagger
using System.Text;                                       // Para codificar la clave JWT

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 🧱 1. Crear builder para configurar servicios y app
        var builder = WebApplication.CreateBuilder(args);

        // 🧩 Habilitar controladores (usaremos Web API)
        builder.Services.AddControllers();

        // 🧪 Habilitar endpoints para exploración Swagger
        builder.Services.AddEndpointsApiExplorer();

        // 📝 2. Configurar Swagger para documentación de la API
        builder.Services.AddSwaggerGen(c =>
        {
            // Título y versión de la API
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Dsw2025Tpi", Version = "v1" });

            // Configurar autenticación JWT en Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",                   // Nombre del header
                Type = SecuritySchemeType.Http,           // Tipo de autenticación
                Scheme = "bearer",                         // Formato de autenticación
                BearerFormat = "JWT",                      // Tipo de token
                In = ParameterLocation.Header,             // Ubicación del token
                Description = "Ingrese el token JWT en el formato: Bearer {tu token}"
            });

            // Aplicar autenticación a todos los endpoints
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
                    Array.Empty<string>() // No se requieren scopes adicionales
                }
            });
        });


        // 🗄️ 3. Configurar bases de datos

        // Base de datos principal con productos, órdenes, etc.
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

        // Base de datos separada solo para usuarios/roles
        builder.Services.AddDbContext<AuthenticateContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

        // 🔐 4. Configuración de Identity usando AuthDb
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthenticateContext>() // Usa AuthDb para guardar usuarios y roles
            .AddDefaultTokenProviders();                     // Tokens para recuperar contraseña, etc.

        // 🧩 5. Registrar servicios y repositorios (Inyección de dependencias)
        builder.Services.AddScoped<IAuthService, AuthService>();                     // Servicio para login/registro
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();             // Servicio para generar tokens JWT
        builder.Services.AddScoped<IProductsManagementService, ProductsManagementService>(); // Lógica de productos
        builder.Services.AddScoped<IRepository, EfRepository>();                     // Repositorio genérico
        builder.Services.AddScoped<IOrdersManagementService, OrdersManagementService>(); // Lógica de órdenes

        // 🔐 6. Configurar autenticación JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new Exception("Missing JWT Key"));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // no requiere HTTPS en dev
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,                       // ¿Quién emite el token?
                ValidateAudience = true,                     // ¿Para quién es el token?
                ValidateLifetime = true,                     // ¿Está vencido?
                ValidateIssuerSigningKey = true,             // ¿Tiene firma válida?
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero                    // Sin tiempo extra tras vencimiento
            };
        });

        // ⚠️ 7. Middleware global de manejo de errores
        builder.Services.AddTransient<GlobalExceptionMiddleware>();

        // 🏗️ 8. Construir la aplicación (WebApplication)
        var app = builder.Build();

        // 🌱 9. Seeding: cargar datos iniciales (productos, admin, roles)
        using (var scope = app.Services.CreateScope())
        {
            // Obtener instancia de contexto principal
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.SeedDatabase(); // Insertar productos y clientes desde SeedData.cs

            // Identity: crear roles y usuario admin desde configuración
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = builder.Configuration;

            // Obtener roles desde appsettings.json
            var roles = config.GetSection("Roles").Get<string[]>();
            if (roles != null)
            {
                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Crear usuario admin si no existe
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
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }

        // 🚦 10. Pipeline de la aplicación HTTP

        // Swagger habilitado solo en entorno de desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Manejo global de errores con nuestro middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Forzar HTTPS
        app.UseHttpsRedirection();

        // Activar autenticación y autorización
        app.UseAuthentication();
        app.UseAuthorization();

        // Mapear controladores a rutas REST (/api/xxx)
        app.MapControllers();

        // 🏁 Iniciar la aplicación
        await app.RunAsync();
    }
}
