using Microsoft.EntityFrameworkCore;

namespace Proyecto_Integrador_Final.Api;

internal sealed class BaseJuego(DbContextOptions<BaseJuego> opciones) : DbContext(opciones)
{
    internal DbSet<PersonajeCatalogo> Catalogo => Set<PersonajeCatalogo>();
    internal DbSet<PartidaGuardada> Partidas => Set<PartidaGuardada>();
    internal DbSet<ParticipanteGuardado> Participantes => Set<ParticipanteGuardado>();

    internal DbSet<PersonajePartida> Cartas => Set<PersonajePartida>();
    internal DbSet<RondaGuardada> Rondas => Set<RondaGuardada>();
    internal DbSet<CombateGuardado> Combates => Set<CombateGuardado>();
    internal DbSet<SolicitudGuardada> Solicitudes => Set<SolicitudGuardada>();
    internal DbSet<OperacionGuardada> Operaciones => Set<OperacionGuardada>();

    internal DbSet<InvitadoGuardado> Invitados => Set<InvitadoGuardado>();
    internal DbSet<AsignacionInvitado> Asignaciones => Set<AsignacionInvitado>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<InvitadoGuardado>().ToTable("Invitado");
        modelo.Entity<AsignacionInvitado>().ToTable("AsignacionInvitado").HasKey(a => new { a.IdInvitado, a.IdPartida });
        modelo.Entity<AsignacionInvitado>().HasIndex(a => new { a.IdPartida, a.Rol }).IsUnique();
        modelo.Entity<PersonajeCatalogo>().ToTable("PersonajeCatalogo").Property(p => p.Id).ValueGeneratedNever();
        modelo.Entity<PartidaGuardada>().ToTable("Partida");
        modelo.Entity<ParticipanteGuardado>().ToTable("Participante")
            .HasKey(p => new { p.IdPartida, p.Rol });
        modelo.Entity<ParticipanteGuardado>().HasOne<PartidaGuardada>()
            .WithMany().HasForeignKey(p => p.IdPartida);
        modelo.Entity<PersonajePartida>().ToTable("PersonajePartida")
            .HasKey(p => new { p.IdPartida, p.IdPersonaje });
        modelo.Entity<PersonajePartida>().HasOne<PartidaGuardada>().WithMany().HasForeignKey(p => p.IdPartida);
        modelo.Entity<RondaGuardada>().ToTable("Ronda").HasKey(r => new { r.IdPartida, r.Numero });
        modelo.Entity<RondaGuardada>().HasOne<PartidaGuardada>().WithMany().HasForeignKey(r => r.IdPartida);
        modelo.Entity<CombateGuardado>().ToTable("Combate")
            .HasKey(c => new { c.IdPartida, c.NumeroRonda, c.Numero });
        modelo.Entity<CombateGuardado>().HasOne<RondaGuardada>().WithMany()
            .HasForeignKey(c => new { c.IdPartida, Numero = c.NumeroRonda });
        modelo.Entity<SolicitudGuardada>().ToTable("Solicitud").HasIndex(s => s.IdPartida);
        modelo.Entity<SolicitudGuardada>().HasOne<CombateGuardado>().WithMany()
            .HasForeignKey(s => new { s.IdPartida, s.NumeroRonda, Numero = s.NumeroCombate });
        modelo.Entity<OperacionGuardada>().ToTable("Operacion")
            .HasKey(o => new { o.IdPartida, o.Rol, o.IdOperacion });
        modelo.Entity<OperacionGuardada>().HasOne<ParticipanteGuardado>().WithMany()
            .HasForeignKey(o => new { o.IdPartida, o.Rol });
        modelo.Entity<OperacionGuardada>().HasOne<SolicitudGuardada>().WithMany()
            .HasForeignKey(o => o.IdSolicitud);
    }
}

internal sealed class PartidaGuardada
{
    public Guid Id { get; set; }
    public int Revision { get; set; }
    public int? NumeroRonda { get; set; }
    public int? NumeroCombate { get; set; }
    public Guid? IdSolicitud { get; set; }
    public string? MotivoFin { get; set; }
    public string? Ganador { get; set; }
    public string? AbandonoDe { get; set; }
    public string Fase { get; set; } = "esperandoInicio";
}

internal sealed class ParticipanteGuardado
{
    public Guid IdPartida { get; set; }
    public required string Rol { get; set; }
    public required byte[] Hash { get; set; }
    public string? Nombre { get; set; }
    public int RondasGanadas { get; set; }
}
