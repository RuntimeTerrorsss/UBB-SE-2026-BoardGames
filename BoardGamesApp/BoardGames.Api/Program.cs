using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using BoardGames.Data;
using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

// EF Core — two patterns coexist: AppDbContext (scoped) for repos taking AppDbContext,
// IDbContextFactory<AppDbContext> for repos that open a context per call.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IFailedLoginRepository, FailedLoginRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRepositoryPayment, RepositoryPayment>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// GamesRepository implements both IGameRepository and InterfaceGamesRepository.
// Register the concrete once and forward both interfaces to it, so one instance per scope serves both.
builder.Services.AddScoped<GamesRepository>();
builder.Services.AddScoped<IGameRepository>(sp => sp.GetRequiredService<GamesRepository>());
builder.Services.AddScoped<InterfaceGamesRepository>(sp => sp.GetRequiredService<GamesRepository>());

// Mappers
builder.Services.AddScoped<AccountProfileMapper>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddScoped<NotificationMapper>();
builder.Services.AddScoped<RentalMapper>();
builder.Services.AddScoped<RequestMapper>();
builder.Services.AddScoped<UserMapper>();

// Business services
builder.Services.AddScoped<IAvatarStorageService, AvatarStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IConversationApiService, ConversationApiService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddHttpContextAccessor();

// Cookie auth — AuthController issues the cookie on successful login.
// [Authorize(Roles = "Admin")] on AdminController relies on this scheme.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await DevDataSeeder.SeedAsync(app.Services);
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Uploads", "Avatars")),
    RequestPath = "/avatars",
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
