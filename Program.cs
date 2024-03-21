using Microsoft.EntityFrameworkCore;

using pixAPI.Services;
using pixAPI.Repositories;
using pixAPI.Middlewares;
using pixAPI.Data;
using Prometheus;
using Microsoft.OpenApi.Models;
using pixAPI.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PixApi", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "PSP token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "PSP token",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

// Services - Repositories
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<PixKeyService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ConcilliationService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PaymentProviderRepository>();
builder.Services.AddScoped<PaymentProviderAccountRepository>();
builder.Services.AddScoped<PixKeyRepository>();
builder.Services.AddScoped<PaymentRepository>();

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

// Configs
IConfigurationSection queueConfigurationSection = builder.Configuration.GetSection("QueueSettings");
builder.Services.Configure<QueueConfig>(queueConfigurationSection);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMetricServer();
app.UseHttpMetrics(options => options.AddCustomLabel("host", context => context.Request.Host.Host));

app.UseHttpsRedirection();

// Middlewares
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthorizationHandlerMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();
