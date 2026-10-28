using Eventos.Web.Models;

namespace Eventos.Web.ViewModels;

public class InscripcionesIndexViewModel
{
    public List<Inscripcion> Inscripciones { get; set; } = new();
    public string? Estado { get; set; }
    public int Confirmadas { get; set; }
    public int Canceladas { get; set; }

    public int Total => Confirmadas + Canceladas;
}
