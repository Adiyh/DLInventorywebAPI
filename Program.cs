//using System;
//using System.Text.Json;
//using LaptopService.Core.Services.ConcreteClass;
//using LaptopService.Core.Services.Interface;
//using LaptopService.Infrastructure.Repositories.ConcreteClass;
//using LaptopService.Infrastructure.Repositories.Interface;
//using LaptopService.Models;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.Configure<EncryptionSettings>(
//    builder.Configuration.GetSection("EncryptionSettings"));

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// 🔹 Register repositories
//builder.Services.AddScoped<ILaptopRepository, LaptopRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();

//// 🔹 Register services
//builder.Services.AddScoped<ILaptopService, LaptopServices>();
//builder.Services.AddScoped<IAuthService, AuthService>();

//// 🔹 Add controllers
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//    });

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngularApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:4200") // frontend origin
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//var app = builder.Build(); // 🔹 Move this line before app.UseCors

//app.UseCors("AllowAngularApp");


////🔹 Apply migrations and create DB if not exists
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.EnsureCreated();
//}

//// 🔹 Middleware
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using LaptopService.Core.Services.ConcreteClass;
using LaptopService.Core.Services.Interface;
using LaptopService.Infrastructure.Repositories.ConcreteClass;
using LaptopService.Infrastructure.Repositories.Interface;
using LaptopService.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure encryption settings
builder.Services.Configure<EncryptionSettings>(
    builder.Configuration.GetSection("EncryptionSettings"));

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ILaptopRepository, LaptopRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<ILaptopService, LaptopServices>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configure CORS to allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger/OpenAPI support for development
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==== JWT Authentication Setup ====
// Read JWT settings from appsettings.json (you need to add these in your config)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings.GetValue<string>("Key");
var issuer = jwtSettings.GetValue<string>("Issuer");
var audience = jwtSettings.GetValue<string>("Audience");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // For dev only, set true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // Make sure roles come from the correct claim
        RoleClaimType = "role"
    };
});

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations and create database if not exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use authentication and authorization middleware (IMPORTANT: order matters)
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS
app.UseCors("AllowAngularApp");

// Map controllers
app.MapControllers();

app.Run();
