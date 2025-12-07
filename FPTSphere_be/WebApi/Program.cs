using BusinessLayer.DTOs;
using BusinessLayer.Helpers;
using BusinessLayer.Mappings;
using BusinessLayer.Services;
using BusinessLayer.Services.Implementations;
using BusinessLayer.Services.Interfaces;
using DataLayer.Data;
using DataLayer.Repositories.Implementations;
using DataLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISystemRoleRepository, SystemRoleRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IExternalLocationRepository, ExternalLocationRepository>();
builder.Services.AddScoped<IEventStatusRepository, EventStatusRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IExternalServiceRepository, ExternalServiceRepository>();
builder.Services.AddScoped<IEventResourceRepository, EventResourceRepository>();
builder.Services.AddScoped<IEventApprovalRepository, EventApprovalRepository>();
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();


// AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddAutoMapper(typeof(UserMappingProfile), typeof(LocationMappingProfile));
builder.Services.AddAutoMapper(
    typeof(UserMappingProfile),
    typeof(LocationMappingProfile),
    typeof(ResourceMappingProfile),
    typeof(EventStatusMappingProfile),
    typeof(EventMappingProfile),
    typeof(ExternalServiceMappingProfile),
    typeof(EventResourceMappingProfile),
    typeof(ExternalLocationMappingProfile),
    typeof(EventApprovalMappingProfile));


// Services
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IExternalLocationService, ExternalLocationService>();
builder.Services.AddScoped<IEventStatusService, EventStatusService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IExternalServiceService, ExternalServiceService>();
builder.Services.AddScoped<IEventResourceService, EventResourceService>();
builder.Services.AddScoped<EventValidationHelper>();
builder.Services.AddScoped<EventPermissionHelper>();
builder.Services.AddScoped<EventFilterHelper>();

// JWT Authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(

                   "http://localhost:3000",      // Original
                   "http://127.0.0.1:3000",
                   "https://localhost:3000",
                   "http://localhost:3001",      // ✅ ADD THIS!
                   "http://127.0.0.1:3001",      // ✅ ADD THIS!
                   "https://localhost:3001",     // ✅ ADD THIS!
                   "http://localhost:5173",
                   "http://127.0.0.1:5173",
                   "https://localhost:5173",
                                     // Port 5174 (Your current frontend) ← ADDED
                   "http://localhost:5174",
                   "http://127.0.0.1:5174",
                   "https://localhost:5174"

               )
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FPTSphere API",
        Version = "v1.0",
        Description = "Event Management System API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// Middleware Pipeline
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Startup Validation
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("Database connected successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Startup error: {ex.Message}");
    }
}

app.Run();