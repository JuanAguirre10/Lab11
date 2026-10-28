using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Eventos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Eventos.Web.Controllers;

public class AsistentesController : Controller
{
    private readonly AsistenteRepositorio _asistentes;

    public AsistentesController(AsistenteRepositorio asistentes)
    {
        _asistentes = asistentes;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        var modelo = new AsistentesIndexViewModel
        {
            Asistentes = await _asistentes.ListarAsync(buscar),
            Buscar = buscar
        };
        return View(modelo);
    }

    public IActionResult Create()
    {
        return View(new Asistente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombres,Dni,Correo")] Asistente asistente)
    {
        asistente.Nombres = asistente.Nombres?.Trim()!;
        asistente.Dni = asistente.Dni?.Trim()!;
        asistente.Correo = asistente.Correo?.Trim().ToLowerInvariant()!;

        if (EsValido(nameof(Asistente.Dni)) && await _asistentes.ExisteDniAsync(asistente.Dni))
            ModelState.AddModelError(nameof(Asistente.Dni), "Ya existe un asistente registrado con este DNI.");

        if (EsValido(nameof(Asistente.Correo)) && await _asistentes.ExisteCorreoAsync(asistente.Correo))
            ModelState.AddModelError(nameof(Asistente.Correo), "Ya existe un asistente registrado con este correo.");

        if (!ModelState.IsValid)
            return View(asistente);

        await _asistentes.CrearAsync(asistente);
        TempData["Mensaje"] = $"Asistente \"{asistente.Nombres}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private bool EsValido(string campo)
    {
        return ModelState.GetFieldValidationState(campo) != ModelValidationState.Invalid;
    }
}
