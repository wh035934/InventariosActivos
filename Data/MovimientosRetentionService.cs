using System.Data.Common;
using InventarioActivos.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Data;

public sealed class MovimientosRetentionService : BackgroundService
{
    private static readonly TimeSpan IntervaloLimpieza = TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MovimientosRetentionService> _logger;

    public MovimientosRetentionService(
        IServiceScopeFactory scopeFactory,
        ILogger<MovimientosRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EliminarMovimientosAntiguosAsync(stoppingToken);

        using var timer = new PeriodicTimer(IntervaloLimpieza);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await EliminarMovimientosAntiguosAsync(stoppingToken);
    }

    private async Task EliminarMovimientosAntiguosAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<InventarioContext>();
            var fechaLimite = DateTime.UtcNow.AddMonths(-2);

            var eliminados = await context.Movimientos
                .Where(m => m.Fecha < fechaLimite)
                .ExecuteDeleteAsync(cancellationToken);

            if (eliminados > 0)
            {
                _logger.LogInformation(
                    "Se eliminaron {Cantidad} movimientos anteriores a {FechaLimite}.",
                    eliminados,
                    fechaLimite);
            }
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "No fue posible limpiar los movimientos antiguos.");
        }
    }
}
