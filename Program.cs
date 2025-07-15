using System.Text.Json;
using LaptopService.Core.Services.ConcreteClass;
using LaptopService.Core.Services.Interface;
using LaptopService.Infrastructure.Repositories.ConcreteClass;
using LaptopService.Infrastructure.Repositories.Interface;
using LaptopService.Models;
using LaptopService.Utility;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configure strongly typed encryption settings
builder.Services.Configure<EncryptionSettings>(
    builder.Configuration.GetSection("EncryptionSettings"));

// 🔧 Configure EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔧 Register repositories
builder.Services.AddScoped<ILaptopRepository, LaptopRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 🔧 Register services
builder.Services.AddScoped<ILaptopService, LaptopServices>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔧 Add controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {

        policy.AllowAnyOrigin()
           
             .AllowAnyHeader()
             .AllowAnyMethod()
             .WithExposedHeaders("*");

    });
});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("FrontendTunnel", policy =>
//    {
//        policy.WithOrigins("https://1zgmf9zp-4200.inc1.devtunnels.ms") // Your frontend tunnel URL
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors("FrontendTunnel");

app.UseCors("AllowAllOrigins");


app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
