using Eventos.Web.Helpers;
using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Eventos.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventos.Web.Controllers;

public class InscripcionesController : Controller
{
    private readonly InscripcionRepositorio _inscripciones;

    public InscripcionesController(InscripcionRepositorio inscripciones)
    {
        _inscripciones = inscripciones;
    }

    public async Task<IActionResult> Index(string? estado)
    {
        if (estado != Inscripcion.Confirmada && estado != Inscripcion.Cancelada)
            estado = null;

        var modelo = new InscripcionesIndexViewModel
        {
            Inscripciones = await _inscripciones.ListarAsync(estado),
            Estado = estado,
            Confirmadas = await _inscripciones.ContarPorEstadoAsync(Inscripcion.Confirmada),
            Canceladas = await _inscripciones.ContarPorEstadoAsync(Inscripcion.Cancelada)
        };
        return View(modelo);
    }

    public async Task<IActionResult> Create(int? eventoId, int? asistenteId)
    {
        await CargarListasAsync();
        return View(new Inscripcion { EventoId = eventoId ?? 0, AsistenteId = asistenteId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventoId,AsistenteId")] Inscripcion inscripcion)
    {
        await ValidarReglasAsync(inscripcion);

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(inscripcion);
        }

        await _inscripciones.CrearAsync(inscripcion);
        TempData["Mensaje"] = "Inscripción registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? estado)
    {
        var inscripcion = await _inscripciones.CancelarAsync(id);
        if (inscripcion == null)
            TempData["Error"] = "La inscripción no existe o ya estaba cancelada.";
        else
            TempData["Mensaje"] = $"Se canceló la inscripción de {inscripcion.Asistente.Nombres} en \"{inscripcion.Evento.Titulo}\".";

        return RedirectToAction(nameof(Index), new { estado });
    }

    private async Task ValidarReglasAsync(Inscripcion inscripcion)
    {
        if (inscripcion.EventoId > 0)
        {
            var evento = await _inscripciones.ObtenerEventoActivoAsync(inscripcion.EventoId);
            if (evento == null)
                ModelState.AddModelError(nameof(Inscripcion.EventoId), "El evento seleccionado no existe o fue eliminado.");
            else if (evento.Fecha <= DateTime.Now)
                ModelState.AddModelError(nameof(Inscripcion.EventoId), "El evento ya se realizó y no admite inscripciones.");
            else if (await _inscripciones.ContarConfirmadasAsync(evento.Id) >= evento.Aforo)
                ModelState.AddModelError(nameof(Inscripcion.EventoId), "El evento no tiene cupos disponibles: el aforo está completo.");
        }

        if (inscripcion.AsistenteId > 0)
        {
            if (!await _inscripciones.ExisteAsistenteActivoAsync(inscripcion.AsistenteId))
                ModelState.AddModelError(nameof(Inscripcion.AsistenteId), "El asistente seleccionado no existe.");
            else if (inscripcion.EventoId > 0 && await _inscripciones.ExisteConfirmadaAsync(inscripcion.EventoId, inscripcion.AsistenteId))
                ModelState.AddModelError(nameof(Inscripcion.AsistenteId), "El asistente ya está inscrito en este evento.");
        }
    }

    private async Task CargarListasAsync()
    {
        var eventos = new List<SelectListItem>();
        foreach (var evento in await _inscripciones.ListarEventosActivosAsync())
        {
            var estado = evento.Finalizado ? "finalizado"
                : evento.Disponibles == 0 ? "sin cupos"
                : evento.Disponibles == 1 ? "1 cupo"
                : $"{evento.Disponibles} cupos";
            eventos.Add(new SelectListItem($"{evento.Titulo} · {Formato.Fecha(evento.Fecha)} · {estado}", evento.Id.ToString()));
        }

        var asistentes = new List<SelectListItem>();
        foreach (var asistente in await _inscripciones.ListarAsistentesActivosAsync())
            asistentes.Add(new SelectListItem($"{asistente.Nombres} · DNI {asistente.Dni}", asistente.Id.ToString()));

        ViewBag.Eventos = eventos;
        ViewBag.Asistentes = asistentes;
    }
}
