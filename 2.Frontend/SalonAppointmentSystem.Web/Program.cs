using SalonAppointmentSystem.Web;
using SalonAppointmentSystem.Web.Components;
using SalonAppointmentSystem.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// ===== MEDIATR (CQRS) =====
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ===== BLAZORISE =====
builder.Services.AddBlazoriseCommunity();

// ===== API CLIENT (Aspire Service Discovery) =====
builder.Services.AddApiClient(builder.Configuration);

// Cliente HTTP de ejemplo (Weather) - puede eliminarse cuando no se necesite
builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
