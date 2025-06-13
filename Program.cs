using System;
using System.Text.Json;
using LaptopService.Core.Services.ConcreteClass;
using LaptopService.Core.Services.Interface;
using LaptopService.Infrastructure.Repositories.ConcreteClass;
using LaptopService.Infrastructure.Repositories.Interface;
using LaptopService.Models;
using LaptopService.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<EncryptionSettings>(
    builder.Configuration.GetSection("EncryptionSettings"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 🔹 Register repositories
builder.Services.AddScoped<ILaptopRepository, LaptopRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// 🔹 Register services
builder.Services.AddScoped<ILaptopService, LaptopServices>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔹 Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // frontend origin
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build(); 

app.UseCors("AllowAngularApp");



// 🔹 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
