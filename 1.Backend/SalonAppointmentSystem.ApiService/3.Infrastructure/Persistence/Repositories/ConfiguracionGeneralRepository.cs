using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para ConfiguracionGeneral
/// </summary>
public class ConfiguracionGeneralRepository : Repository<ConfiguracionGeneral>, IConfiguracionGeneralRepository
{
    public ConfiguracionGeneralRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ConfiguracionGeneral?> GetByClaveAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Clave == clave, cancellationToken);
    }

    public async Task<int> GetValorEnteroAsync(
        string clave,
        int defaultValue = 0,
        CancellationToken cancellationToken = default)
    {
        var config = await GetByClaveAsync(clave, cancellationToken);
        return config?.ObtenerValorEntero() ?? defaultValue;
    }

    public async Task<bool> GetValorBooleanAsync(
        string clave,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        var config = await GetByClaveAsync(clave, cancellationToken);
        return config?.ObtenerValorBoolean() ?? defaultValue;
    }

    public async Task<string> GetValorTextoAsync(
        string clave,
        string defaultValue = "",
        CancellationToken cancellationToken = default)
    {
        var config = await GetByClaveAsync(clave, cancellationToken);
        return config?.ObtenerValorTexto() ?? defaultValue;
    }

    public async Task UpsertAsync(
        string clave,
        string valor,
        CancellationToken cancellationToken = default)
    {
        var config = await GetByClaveAsync(clave, cancellationToken);

        if (config == null)
        {
            config = new ConfiguracionGeneral
            {
                Clave = clave,
                Valor = valor,
                TipoDato = TipoDatoConfig.Texto
            };
            await AddAsync(config, cancellationToken);
        }
        else
        {
            config.Valor = valor;
            Update(config);
        }
    }
}

