using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Eventos.Web.Models;

public class Inscripcion
{
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";

    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un evento.")]
    public int EventoId { get; set; }

    [ValidateNever]
    public Evento Evento { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un asistente.")]
    public int AsistenteId { get; set; }

    [ValidateNever]
    public Asistente Asistente { get; set; } = null!;

    public DateTime FechaInscripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado admite como máximo 20 caracteres.")]
    [RegularExpression("^(Confirmada|Cancelada)$", ErrorMessage = "El estado debe ser Confirmada o Cancelada.")]
    public string Estado { get; set; } = Confirmada;
}
