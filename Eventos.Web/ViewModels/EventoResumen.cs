namespace Eventos.Web.ViewModels;

public class EventoResumen
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int Aforo { get; set; }
    public decimal Precio { get; set; }
    public int Confirmados { get; set; }

    public int Disponibles => Math.Max(Aforo - Confirmados, 0);
    public bool Finalizado => Fecha <= DateTime.Now;
    public int Ocupacion => Aforo == 0 ? 0 : Math.Min(Confirmados * 100 / Aforo, 100);
}
