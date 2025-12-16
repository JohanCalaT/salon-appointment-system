using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using SalonAppointmentSystem.Web.Services.Http;

namespace SalonAppointmentSystem.Web.Extensions;

/// <summary>
/// Extensiones para configuración de servicios
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura Blazorise con Bootstrap 5 y FontAwesome
    /// </summary>
    public static IServiceCollection AddBlazoriseCommunity(this IServiceCollection services)
    {
        services
            .AddBlazorise(options =>
            {
                options.Immediate = true; // Validación inmediata en formularios
            })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();

        return services;
    }

    /// <summary>
    /// Configura el cliente HTTP para comunicarse con la API usando Aspire Service Discovery
    /// </summary>
    public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            // Usa Aspire Service Discovery para resolver "apiservice"
            // El esquema "https+http://" indica preferencia por HTTPS sobre HTTP
            client.BaseAddress = new Uri("https+http://apiservice");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}

