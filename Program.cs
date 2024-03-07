using Microsoft.EntityFrameworkCore;

using pixAPI.Services;
using pixAPI.Repositories;
using pixAPI.Middlewares;
using pixAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PaymentProviderRepository>();
builder.Services.AddScoped<PaymentProviderAccountRepository>();
builder.Services.AddScoped<PixKeyRepository>();

// Database
builder.Services.AddDbContext<AppDBContext>(opts =>
{
    string host = builder.Configuration["Database:Host"] ?? string.Empty;
    string port = builder.Configuration["Database:Port"] ?? string.Empty;
    string username = builder.Configuration["Database:Username"] ?? string.Empty;
    string database = builder.Configuration["Database:Name"] ?? string.Empty;
    string password = builder.Configuration["Database:Password"] ?? string.Empty;

    string connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";
    opts.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middlewares
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthorizationHandlerMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
