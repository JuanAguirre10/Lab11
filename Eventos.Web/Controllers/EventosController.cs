using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Eventos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventos.Web.Controllers;

public class EventosController : Controller
{
    private readonly EventoRepositorio _eventos;

    public EventosController(EventoRepositorio eventos)
    {
        _eventos = eventos;
    }

    public async Task<IActionResult> Index(int? categoriaId)
    {
        var modelo = new EventosIndexViewModel
        {
            Eventos = await _eventos.ListarAsync(categoriaId),
            Categorias = await _eventos.ListarCategoriasActivasAsync(),
            CategoriaId = categoriaId
        };
        return View(modelo);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _eventos.ObtenerConInscritosAsync(id.Value);
        if (evento == null) return NotFound();

        return View(evento);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Evento { Fecha = DateTime.Today.AddDays(7).AddHours(19), Aforo = 50 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Titulo,Fecha,Lugar,Modalidad,Aforo,Precio,CategoriaId")] Evento evento)
    {
        await ValidarReglasAsync(evento);

        if (EsValido(nameof(Evento.Fecha)) && evento.Fecha <= DateTime.Now)
            ModelState.AddModelError(nameof(Evento.Fecha), "La fecha debe ser posterior a la fecha y hora actual.");

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        await _eventos.CrearAsync(evento);
        TempData["Mensaje"] = $"Evento \"{evento.Titulo}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _eventos.ObtenerAsync(id.Value);
        if (evento == null) return NotFound();

        await CargarCategoriasAsync(evento.CategoriaId);
        return View(evento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Fecha,Lugar,Modalidad,Aforo,Precio,CategoriaId")] Evento evento)
    {
        if (id != evento.Id) return NotFound();

        await ValidarReglasAsync(evento);

        if (EsValido(nameof(Evento.Aforo)))
        {
            var confirmados = await _eventos.ContarConfirmadosAsync(evento.Id);
            if (evento.Aforo < confirmados)
                ModelState.AddModelError(nameof(Evento.Aforo),
                    $"El aforo no puede ser menor que las {confirmados} inscripciones confirmadas.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        if (!await _eventos.ActualizarAsync(evento))
            return NotFound();

        TempData["Mensaje"] = $"Evento \"{evento.Titulo}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _eventos.ObtenerAsync(id.Value);
        if (evento == null) return NotFound();

        ViewBag.Confirmados = await _eventos.ContarConfirmadosAsync(evento.Id);
        return View(evento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var evento = await _eventos.ObtenerAsync(id);
        if (evento == null) return NotFound();

        var confirmados = await _eventos.ContarConfirmadosAsync(id);
        if (!ModelState.IsValid)
        {
            ViewBag.Confirmados = confirmados;
            return View(evento);
        }

        if (!await _eventos.DesactivarAsync(id))
            return NotFound();

        TempData["Mensaje"] = confirmados switch
        {
            0 => $"Evento \"{evento.Titulo}\" eliminado correctamente.",
            1 => $"Evento \"{evento.Titulo}\" eliminado. Se canceló 1 inscripción.",
            _ => $"Evento \"{evento.Titulo}\" eliminado. Se cancelaron {confirmados} inscripciones."
        };
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarReglasAsync(Evento evento)
    {
        if (evento.CategoriaId > 0 && !await _eventos.ExisteCategoriaActivaAsync(evento.CategoriaId))
            ModelState.AddModelError(nameof(Evento.CategoriaId), "La categoría seleccionada no existe o está inactiva.");
    }

    private bool EsValido(string campo)
    {
        return ModelState.GetFieldValidationState(campo) != ModelValidationState.Invalid;
    }

    private async Task CargarCategoriasAsync(int? seleccionada = null)
    {
        var categorias = await _eventos.ListarCategoriasActivasAsync();
        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.Id), nameof(Categoria.Nombre), seleccionada);
    }
}
