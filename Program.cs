using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OAuthServer.Models;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// DB & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AccountDatabase"));
    // register the entity sets needed by OpenIddict
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// OpenIddict (server)
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetEndSessionEndpointUris("connect/logout")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserInfoEndpointUris("connect/userinfo");

        options.AllowAuthorizationCodeFlow()
               .RequireProofKeyForCodeExchange() // PKCE
               .AllowRefreshTokenFlow();

        // Use JWT access tokens
        options.AddEphemeralEncryptionKey()   // for dev; use persistent keys in prod
               .AddEphemeralSigningKey();

        // Register scopes
        options.RegisterScopes(OpenIddictConstants.Scopes.Email,
                               OpenIddictConstants.Scopes.Profile,
                               "api");

        // Customize claims mapping if needed
        options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        // Use local server for token validation
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// MVC (controllers/views) for consent / login
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OidcCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Apply migrations on startup (optional for dev/ops)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    db.Database.EnsureCreated();

    var sp = scope.ServiceProvider;
    Seed.InitializeAsync(sp).Wait();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("OidcCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
