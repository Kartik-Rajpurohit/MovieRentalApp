using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;
using MovieRental.Repository.Repositories;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Add Services ──────────────────────────────────────────────

builder.Services.AddControllers(options =>
{
    // Apply [Authorize] globally — all endpoints require valid JWT by default
    // Use [AllowAnonymous] on specific endpoints to make them public (e.g. Login, SignUp)
    options.Filters.Add(new AuthorizeFilter());
})
.AddJsonOptions(options =>
{
    // Ignore circular references — prevents infinite loops when serializing
    // entities with bidirectional navigation properties (e.g. Store ↔ Staff)
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();

// ── Swagger with JWT support ──────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// ── CORS Configuration ────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["Cors:AllowedOrigins"]!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for HttpOnly cookie to be sent with Axios withCredentials: true
    });
});

// ── Rate Limiting ─────────────────────────────────────────────
// Protects auth endpoints from brute-force and abuse.
// Fixed window: max 10 requests per IP per 60 seconds.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthRateLimit", limiterOptions =>
    {
        limiterOptions.PermitLimit         = 10;                          // max 10 requests
        limiterOptions.Window              = TimeSpan.FromMinutes(1);     // per 60 seconds
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit          = 0;                           // no queuing — reject immediately
    });

    // Return 429 Too Many Requests with a Retry-After header
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again in 60 seconds.", cancellationToken);
    };
});

// ── Database Configuration ────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repository & Service Registration ─────────────────────────

// Auth
builder.Services.AddScoped<IAuthService, AuthService>();

// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Role
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Staff
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

// Customer
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Film
builder.Services.AddScoped<IFilmRepository, FilmRepository>();
builder.Services.AddScoped<IFilmService, FilmService>();

// Actor
builder.Services.AddScoped<IActorRepository, ActorRepository>();
builder.Services.AddScoped<IActorService, ActorService>();

// Category
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Language
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ILanguageService, LanguageService>();

// Inventory
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Rental
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalService, RentalService>();

// Payment
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Store
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();

// Country
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICountryService, CountryService>();

// City
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityService, CityService>();

// Address
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();

// Dashboard
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddHttpContextAccessor();

// ── JWT Authentication Configuration ──────────────────────────
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer    = builder.Configuration["Jwt:Issuer"];
var jwtAudience  = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey!)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Default policy — any plain [Authorize] requires a real assigned role.
    // This blocks Unassigned users from all endpoints that don't specify a role explicitly.
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("Admin", "Staff", "Customer")
        .Build();

    // Named policy for Logout — only requires a valid JWT (any role including Unassigned).
    // Unassigned users must be able to log out and have their refresh token revoked.
    options.AddPolicy("AuthenticatedOnly", policy =>
        policy.RequireAuthenticatedUser());
});

// ── Build App ─────────────────────────────────────────────────
var app = builder.Build();

// ── Development Middleware ────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Pipeline Setup ────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseCors("ReactPolicy");

// Rate limiting must come before auth in the pipeline
app.UseRateLimiter();

// Authentication must come before Authorization in pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
