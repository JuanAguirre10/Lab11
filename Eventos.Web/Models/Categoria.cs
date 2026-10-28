using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Models;

[Index(nameof(Nombre), IsUnique = true)]
public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre admite como máximo 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; } = true;

    [ValidateNever]
    public List<Evento> Eventos { get; set; } = new();
}
