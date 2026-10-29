using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Domain.Entities;
using ClaimServiceSystem.Middleware;
using ClaimServiceSystem.Infrastructure.Persistence;
using ClaimServiceSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlite("Data Source=claims-lifecycle.db"));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200", "http://localhost:4201"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaimServiceSystem-Local-Dev-Secret-Key-Change-Me-123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ClaimServiceSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ClaimServiceSystem.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IClaimsService, ClaimsService>();
builder.Services.AddScoped<IStaffClaimsService, StaffClaimsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    await SeedUsers(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<ClaimantContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedUsers(ClaimsDbContext dbContext)
{
    if (!await dbContext.Claimants.AnyAsync(c => c.ExternalUserId == "claimant1"))
    {
        dbContext.Claimants.Add(new Claimant
        {
            Id = Guid.NewGuid(),
            ExternalUserId = "claimant1",
            FullName = "Claimant One",
            Email = "claimant1@local"
        });
    }

    if (!await dbContext.StaffUsers.AnyAsync(s => s.ExternalUserId == "staff1"))
    {
        dbContext.StaffUsers.Add(new StaffUser
        {
            Id = Guid.NewGuid(),
            ExternalUserId = "staff1",
            FullName = "Staff One",
            Email = "staff1@local",
            Role = StaffRole.Officer
        });
    }

    if (!await dbContext.AuthUsers.AnyAsync(u => u.Username == "claimant1"))
    {
        dbContext.AuthUsers.Add(new AuthUser
        {
            Id = Guid.NewGuid(),
            Username = "claimant1",
            PasswordHash = AuthService.ComputeHash("Pass@123"),
            Role = "Claimant",
            SubjectId = "claimant1"
        });
    }

    if (!await dbContext.AuthUsers.AnyAsync(u => u.Username == "staff1"))
    {
        dbContext.AuthUsers.Add(new AuthUser
        {
            Id = Guid.NewGuid(),
            Username = "staff1",
            PasswordHash = AuthService.ComputeHash("Pass@123"),
            Role = "ClaimStaff",
            SubjectId = "staff1"
        });
    }

    await dbContext.SaveChangesAsync();
}
