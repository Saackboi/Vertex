using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vertex.Application.Interfaces;
using Vertex.Application.Services;
using Vertex.Domain.Entities;
using Vertex.Infrastructure.Data;
using Vertex.Infrastructure.Repositories;
using Vertex.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURACIÓN DE SERVICIOS ==========

// Configuración de DbContext con SQL Server
builder.Services.AddDbContext<VertexDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Configuración de Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configuración de contraseñas
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Configuración de usuarios
    options.User.RequireUniqueEmail = true;
    
    // Configuración de bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<VertexDbContext>()
.AddDefaultTokenProviders();

// Configuración de JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero // Elimina el margen de 5 minutos por defecto
    };
});

// Inyección de Dependencias: Unit of Work (Transacciones)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Inyección de Dependencias: Repositorios
builder.Services.AddScoped<IOnboardingRepository, OnboardingRepository>();
builder.Services.AddScoped<IProfessionalProfileRepository, ProfessionalProfileRepository>();

// Inyección de Dependencias: Servicios de Infraestructura
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Inyección de Dependencias: Servicios de Aplicación
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuración de Controladores
builder.Services.AddControllers();

// Configuración de OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de CORS (para conectar con frontend Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",      // Angular dev server
                "http://localhost:3000",      // React (por si acaso)
                "http://localhost:5173"       // Vite
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ========== CONFIGURACIÓN DEL PIPELINE HTTP ==========

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplicar política de CORS (ANTES de Authentication)
app.UseCors("AllowAngular");

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
