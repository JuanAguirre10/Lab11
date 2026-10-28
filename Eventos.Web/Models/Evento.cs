using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Eventos.Web.Models;

public class Evento
{
    public const string Presencial = "Presencial";
    public const string Virtual = "Virtual";

    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(100, ErrorMessage = "El título admite como máximo 100 caracteres.")]
    public string Titulo { get; set; } = null!;

    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "El lugar es obligatorio.")]
    [StringLength(100, ErrorMessage = "El lugar admite como máximo 100 caracteres.")]
    public string Lugar { get; set; } = null!;

    [Range(1, 5000, ErrorMessage = "El aforo debe estar entre 1 y 5000 personas.")]
    public int Aforo { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 99999999.99, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaId { get; set; }

    [ValidateNever]
    public Categoria Categoria { get; set; } = null!;

    public bool Activo { get; set; } = true;

    [Required(ErrorMessage = "La modalidad es obligatoria.")]
    [StringLength(20)]
    [RegularExpression("^(Presencial|Virtual)$", ErrorMessage = "La modalidad debe ser Presencial o Virtual.")]
    public string Modalidad { get; set; } = Presencial;

    [ValidateNever]
    public List<Inscripcion> Inscripciones { get; set; } = new();
}
