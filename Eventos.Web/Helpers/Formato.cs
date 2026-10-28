using System.Globalization;

namespace Eventos.Web.Helpers;

public static class Formato
{
    public static string Precio(decimal precio)
    {
        return precio == 0 ? "Gratis" : precio.ToString("C");
    }

    public static string Fecha(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy HH:mm");
    }

    public static string FechaLarga(DateTime fecha)
    {
        var texto = fecha.ToString("dddd d 'de' MMMM 'de' yyyy, HH:mm");
        return CultureInfo.CurrentCulture.TextInfo.ToUpper(texto[0]) + texto[1..];
    }

    public static string Mes(DateTime fecha)
    {
        return fecha.ToString("MMM").TrimEnd('.');
    }

    public static string Dia(DateTime fecha)
    {
        return fecha.ToString("dd");
    }

    public static string Hora(DateTime fecha)
    {
        return fecha.ToString("HH:mm");
    }
}
