using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Models;

[Index(nameof(Dni), IsUnique = true)]
[Index(nameof(Correo), IsUnique = true)]
public class Asistente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres admiten como máximo 100 caracteres.")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8)]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos.")]
    public string Dni { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El correo admite como máximo 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string Correo { get; set; } = null!;

    public bool Activo { get; set; } = true;

    [ValidateNever]
    public List<Inscripcion> Inscripciones { get; set; } = new();
}
