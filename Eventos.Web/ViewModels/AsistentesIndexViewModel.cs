using Eventos.Web.Models;

namespace Eventos.Web.ViewModels;

public class AsistentesIndexViewModel
{
    public List<Asistente> Asistentes { get; set; } = new();
    public string? Buscar { get; set; }
}
