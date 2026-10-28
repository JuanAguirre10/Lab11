using Eventos.Web.Data;
using Eventos.Web.Models;
using Eventos.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class InscripcionRepositorio
{
    private readonly EventosContext _context;

    public InscripcionRepositorio(EventosContext context)
    {
        _context = context;
    }

    public async Task<List<Inscripcion>> ListarAsync(string? estado)
    {
        var consulta = _context.Inscripciones
            .AsNoTracking()
            .Include(i => i.Evento)
            .Include(i => i.Asistente)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            consulta = consulta.Where(i => i.Estado == estado);

        return await consulta
            .OrderByDescending(i => i.FechaInscripcion)
            .ToListAsync();
    }

    public async Task<int> ContarPorEstadoAsync(string estado)
    {
        return await _context.Inscripciones.CountAsync(i => i.Estado == estado);
    }

    public async Task<List<EventoResumen>> ListarEventosActivosAsync()
    {
        return await _context.Eventos
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Fecha)
            .Select(e => new EventoResumen
            {
                Id = e.Id,
                Titulo = e.Titulo,
                Fecha = e.Fecha,
                Aforo = e.Aforo,
                Confirmados = e.Inscripciones.Count(i => i.Estado == Inscripcion.Confirmada)
            })
            .ToListAsync();
    }

    public async Task<List<Asistente>> ListarAsistentesActivosAsync()
    {
        return await _context.Asistentes
            .AsNoTracking()
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombres)
            .ToListAsync();
    }

    public async Task<Evento?> ObtenerEventoActivoAsync(int eventoId)
    {
        return await _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventoId && e.Activo);
    }

    public async Task<bool> ExisteAsistenteActivoAsync(int asistenteId)
    {
        return await _context.Asistentes.AnyAsync(a => a.Id == asistenteId && a.Activo);
    }

    public async Task<bool> ExisteConfirmadaAsync(int eventoId, int asistenteId)
    {
        return await _context.Inscripciones.AnyAsync(i =>
            i.EventoId == eventoId &&
            i.AsistenteId == asistenteId &&
            i.Estado == Inscripcion.Confirmada);
    }

    public async Task<int> ContarConfirmadasAsync(int eventoId)
    {
        return await _context.Inscripciones
            .CountAsync(i => i.EventoId == eventoId && i.Estado == Inscripcion.Confirmada);
    }

    public async Task CrearAsync(Inscripcion inscripcion)
    {
        inscripcion.FechaInscripcion = DateTime.Now;
        inscripcion.Estado = Inscripcion.Confirmada;
        _context.Inscripciones.Add(inscripcion);
        await _context.SaveChangesAsync();
    }

    public async Task<Inscripcion?> CancelarAsync(int id)
    {
        var inscripcion = await _context.Inscripciones
            .Include(i => i.Evento)
            .Include(i => i.Asistente)
            .FirstOrDefaultAsync(i => i.Id == id && i.Estado == Inscripcion.Confirmada);
        if (inscripcion == null)
            return null;

        inscripcion.Estado = Inscripcion.Cancelada;
        await _context.SaveChangesAsync();
        return inscripcion;
    }
}
