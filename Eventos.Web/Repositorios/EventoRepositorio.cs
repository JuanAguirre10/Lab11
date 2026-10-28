using Eventos.Web.Data;
using Eventos.Web.Models;
using Eventos.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class EventoRepositorio
{
    private readonly EventosContext _context;

    public EventoRepositorio(EventosContext context)
    {
        _context = context;
    }

    public async Task<List<EventoResumen>> ListarAsync(int? categoriaId)
    {
        var ahora = DateTime.Now;
        var consulta = _context.Eventos
            .AsNoTracking()
            .Where(e => e.Activo);

        if (categoriaId.HasValue)
            consulta = consulta.Where(e => e.CategoriaId == categoriaId.Value);

        return await consulta
            .OrderBy(e => e.Fecha <= ahora)
            .ThenBy(e => e.Fecha)
            .Select(e => new EventoResumen
            {
                Id = e.Id,
                Titulo = e.Titulo,
                Fecha = e.Fecha,
                Lugar = e.Lugar,
                Categoria = e.Categoria.Nombre,
                Aforo = e.Aforo,
                Precio = e.Precio,
                Confirmados = e.Inscripciones.Count(i => i.Estado == Inscripcion.Confirmada)
            })
            .ToListAsync();
    }

    public async Task<List<Categoria>> ListarCategoriasActivasAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteCategoriaActivaAsync(int categoriaId)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == categoriaId && c.Activo);
    }

    public async Task<Evento?> ObtenerAsync(int id)
    {
        return await _context.Eventos
            .AsNoTracking()
            .Include(e => e.Categoria)
            .FirstOrDefaultAsync(e => e.Id == id && e.Activo);
    }

    public async Task<Evento?> ObtenerConInscritosAsync(int id)
    {
        return await _context.Eventos
            .AsNoTracking()
            .Include(e => e.Categoria)
            .Include(e => e.Inscripciones
                .Where(i => i.Estado == Inscripcion.Confirmada)
                .OrderBy(i => i.FechaInscripcion))
                .ThenInclude(i => i.Asistente)
            .FirstOrDefaultAsync(e => e.Id == id && e.Activo);
    }

    public async Task<int> ContarConfirmadosAsync(int eventoId)
    {
        return await _context.Inscripciones
            .CountAsync(i => i.EventoId == eventoId && i.Estado == Inscripcion.Confirmada);
    }

    public async Task CrearAsync(Evento evento)
    {
        evento.Activo = true;
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Evento evento)
    {
        var actual = await _context.Eventos.FindAsync(evento.Id);
        if (actual == null || !actual.Activo)
            return false;

        actual.Titulo = evento.Titulo;
        actual.Fecha = evento.Fecha;
        actual.Lugar = evento.Lugar;
        actual.Aforo = evento.Aforo;
        actual.Precio = evento.Precio;
        actual.CategoriaId = evento.CategoriaId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var evento = await _context.Eventos
            .Include(e => e.Inscripciones.Where(i => i.Estado == Inscripcion.Confirmada))
            .FirstOrDefaultAsync(e => e.Id == id && e.Activo);
        if (evento == null)
            return false;

        evento.Activo = false;
        foreach (var inscripcion in evento.Inscripciones)
            inscripcion.Estado = Inscripcion.Cancelada;

        await _context.SaveChangesAsync();
        return true;
    }
}
