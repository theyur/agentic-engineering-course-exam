using HT.Application;
using HT.Persistence;
using HT.Persistence.Identity;
using HT.Ui.Account;
using HT.Ui.Components;
using HT.Ui.Habits;
using HT.Ui.Operator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Read before the services are built: host settings (UseSetting in host tests) are already visible here.
var connectionString = builder.Configuration.GetConnectionString("HabitTracker");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("The connection string 'HabitTracker' is not configured.");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHtApplication();
builder.Services.AddHtPersistence(connectionString);

// The session is the host's only job (research R-02, R-09).
new IdentityBuilder(typeof(HtUser), builder.Services).AddSignInManager();
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.LoginPath = "/sign-in";
        options.ReturnUrlParameter = "returnUrl";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
        };
    })
    // Not used for sign-in: SecurityStampValidator signs this scheme out when it rejects a stale session
    // (HP-007), and an unregistered scheme would throw instead of sending the browser to Sign in.
    .AddCookie(IdentityConstants.TwoFactorRememberMeScheme);
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
// The circuit's session is revalidated against the security stamp every 30 minutes (HP-007).
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<AccountSession>();
builder.Services.AddScoped<ActingUserAccessor>();
// Interactive pages call each use case in a scope of its own (research R-15).
builder.Services.AddSingleton<UseCaseScope>();

var app = builder.Build();

await StartupTasks.RunAsync(
    app.Services,
    app.Environment,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(StartupTasks.LoggerCategory));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapSignOut();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
