// <copyright file="Program.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>


using BoardGames.Data.Repositories;
using BoardGames.Shared.ProxyRepositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
string apiBaseUrl = "https://localhost:7027/api/";

// ADD AUTHENTICATION
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "BoardGames.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

// DEPENDENCY INJECTION
builder.Services.AddHttpClient<IConversationRepository, ConversationAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<InterfaceGamesRepository, GamesAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IPaymentRepository, PaymentAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IRentalRepository, RentalAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IRepositoryPayment, RepositoryPaymentAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IUserRepository, UserAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Register your Business Logic Services
builder.Services.AddScoped<InterfaceBookingService, BookingService>();
builder.Services.AddScoped<ICardPaymentService, CardPaymentService>();
builder.Services.AddScoped<ICashPaymentService, CashPaymentService>();
builder.Services.AddScoped<IConversationNotifier, ConversationNotifier>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddSingleton<InterfaceGeographicalService>(provider =>
{
    return GeographicalService.LoadFromFileAsync().GetAwaiter().GetResult();
}); builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<InterfaceSearchAndFilterService, SearchAndFilterService>();
builder.Services.AddScoped<IServicePayment, ServicePayment>();
builder.Services.AddScoped<ICashPaymentMapper, CashPaymentMapper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, CardPaymentService>();

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "images"));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE PIPELINE
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
