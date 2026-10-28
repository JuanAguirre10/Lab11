using Eventos.Web.Data;
using Eventos.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class AsistenteRepositorio
{
    private readonly EventosContext _context;

    public AsistenteRepositorio(EventosContext context)
    {
        _context = context;
    }

    public async Task<List<Asistente>> ListarAsync(string? buscar)
    {
        var consulta = _context.Asistentes
            .AsNoTracking()
            .Where(a => a.Activo);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(a => a.Nombres.Contains(texto) || a.Dni.StartsWith(texto));
        }

        return await consulta
            .OrderBy(a => a.Nombres)
            .ToListAsync();
    }

    public async Task<bool> ExisteDniAsync(string dni)
    {
        return await _context.Asistentes.AnyAsync(a => a.Dni == dni);
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _context.Asistentes.AnyAsync(a => a.Correo == correo);
    }

    public async Task CrearAsync(Asistente asistente)
    {
        asistente.Activo = true;
        _context.Asistentes.Add(asistente);
        await _context.SaveChangesAsync();
    }
}
