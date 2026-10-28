using Eventos.Web.Models;

namespace Eventos.Web.ViewModels;

public class EventosIndexViewModel
{
    public List<EventoResumen> Eventos { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public int? CategoriaId { get; set; }
}
