using CoopQueue.Api.Data;
using CoopQueue.Api.Middleware;
using CoopQueue.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service Registration ---

builder.Services.AddControllers();

//For Blazor and Razorpages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Swagger/OpenAPI for API documentation and testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context (SQL Server)
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Dependency Injection: Register Application Services
builder.Services.AddScoped<IIgdbService, IgdbService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// --- 2. Security Configuration (JWT & CORS) ---

var tokenKey = builder.Configuration["AppSettings:Token"];
var key = Encoding.UTF8.GetBytes(tokenKey!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // Set to false for development simplicity; strictly validate these in production environments
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// --- 3. HTTP Request Pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    // So the docker container works in dev mode without certificate
    app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

// Automatic Migration on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();

        // Applies any pending migrations or creates the database if it doesn't exist.
        // This ensures the database schema is always up-to-date with the code.
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log errors instead of crashing the application.
        // Crucial in containerized environments where the DB might not be ready yet.
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration.");
    }
}

app.Run();