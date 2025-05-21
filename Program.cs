using System;
using LaptopService.Core.Services.ConcreteClass;
using LaptopService.Core.Services.Interface;
using LaptopService.Infrastructure.Repositories.ConcreteClass;
using LaptopService.Infrastructure.Repositories.Interface;
using LaptopService.Models;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

 //🔹 Apply migrations and create DB if not exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

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
