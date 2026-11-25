using System.Threading.Channels;

namespace Proyecto_Integrador_Final.Api;

// Avisos coalescentes: nunca transportan vistas, identidades, manos ni secretos.
internal sealed class Avisos
{
    private readonly object puerta = new();
    private readonly Dictionary<Guid, List<Channel<bool>>> lectores = [];
    internal (ChannelReader<bool> Lector, Action Cerrar) Suscribir(Guid invitado)
    {
        var canal = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (puerta)
        {
            if (!lectores.TryGetValue(invitado, out var lista)) lectores[invitado] = lista = [];
            lista.Add(canal);
            canal.Writer.TryWrite(true);
        }
        return (canal.Reader, () =>
        {
            lock (puerta)
            {
                if (lectores.TryGetValue(invitado, out var lista))
                { lista.Remove(canal); if (lista.Count == 0) lectores.Remove(invitado); }
                canal.Writer.TryComplete();
            }
        });
    }
    internal void PublicarPartida(AlmacenPartidas almacen, Guid partida)
    {
        var ids = almacen.Infraestructura.Ejecutar(false, db => db.Asignaciones.Where(a => a.IdPartida == partida).Select(a => a.IdInvitado).ToArray());
        foreach (var id in ids) Publicar(id);
    }
    internal void Publicar(Guid invitado)
    {
        lock (puerta)
            if (lectores.TryGetValue(invitado, out var lista))
                foreach (var canal in lista) canal.Writer.TryWrite(true);
    }
}

internal sealed class VigilanciaAcceso(Emparejamiento acceso, ILogger<VigilanciaAcceso> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try { acceso.Vencer(); }
                catch (Exception error) when (error is Microsoft.Data.Sqlite.SqliteException or Microsoft.EntityFrameworkCore.DbUpdateException)
                { logger.LogWarning("No se pudo comprobar el vencimiento de accesos; se reintentará."); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
