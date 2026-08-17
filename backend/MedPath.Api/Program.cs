using System.Text;
using MedPath.Application;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlServer = string.Equals(builder.Configuration.GetValue<string>("Database:Provider") ?? "InMemory", "SqlServer", StringComparison.OrdinalIgnoreCase);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlServer && !string.IsNullOrWhiteSpace(connectionString)) options.UseSqlServer(connectionString);
    else options.UseInMemoryDatabase("MedPathAI");
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "local-only-change-this-key-at-least-32-chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MedPathAI",
        ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MedPathAI.Web",
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CourseManagement", policy => policy.RequireRole("Educator", "Administrator"));
    options.AddPolicy("UserManagement", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("StudentLearning", policy => policy.RequireRole("Student", "Educator", "Administrator"));
});
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddSingleton<IAiStudyService, MockAiStudyService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.AddSecurityDefinition("Bearer", new() { Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" }));
builder.Services.AddProblemDetails();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "medpath-api" }));
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordService>());
}
app.Run();

public partial class Program { }
