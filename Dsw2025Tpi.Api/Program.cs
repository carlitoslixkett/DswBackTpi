
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
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        // HABILITAR CORS PARA EL FRONTEND VITE
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });


        builder.Services.AddSwaggerGen(c =>
        {

            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Dsw2025Tpi", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",                   
                Type = SecuritySchemeType.Http,           
                Scheme = "bearer",                         
                BearerFormat = "JWT",                      
                In = ParameterLocation.Header,             
                Description = "Ingrese el token:"
            });

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


 
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

        builder.Services.AddDbContext<AuthenticateContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));



        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthenticateContext>() 
            .AddDefaultTokenProviders();                     

        // i.d.
        builder.Services.AddScoped<IAuthService, AuthService>();                     
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();             
        builder.Services.AddScoped<IProductsManagementService, ProductsManagementService>(); 
        builder.Services.AddScoped<IRepository, EfRepository>();                    
        builder.Services.AddScoped<IOrdersManagementService, OrdersManagementService>(); 

    
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
                ValidateIssuer = true,                       
                ValidateAudience = true,                    
                ValidateLifetime = true,                     
                ValidateIssuerSigningKey = true,             
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero                    
            };
        });

        builder.Services.AddTransient<GlobalExceptionMiddleware>();

  
        var app = builder.Build();

      
        using (var scope = app.Services.CreateScope())
        {
   
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.SeedDatabase(); 

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = builder.Configuration;

    
            var roles = config.GetSection("Roles").Get<string[]>();
            if (roles != null)
            {
                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
            }

   
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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

 
        app.UseMiddleware<GlobalExceptionMiddleware>();

  
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}
