using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController(HabitacionRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Index(int? tipoId)
    {
        ViewBag.Tipos = new SelectList(await repositorio.TiposActivosAsync(), "IdTipoHabitacion", "Nombre", tipoId);
        return View(await repositorio.ListarActivasAsync(tipoId));
    }
    public async Task<IActionResult> Details(int? id) { if (id is null) return NotFound(); var h = await repositorio.ObtenerAsync(id.Value); return h is null ? NotFound() : View(h); }
    public async Task<IActionResult> Create() { await CargarTiposAsync(); return View(new Habitacion()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation)); ModelState.Remove(nameof(Habitacion.Reservas));
        if (await repositorio.NumeroExisteAsync(habitacion.Numero)) ModelState.AddModelError("Numero", "Ya existe una habitación con ese número.");
        if (ModelState.IsValid) { habitacion.Activo = true; await repositorio.CrearAsync(habitacion); TempData["Exito"] = "Habitación creada correctamente."; return RedirectToAction(nameof(Index)); }
        await CargarTiposAsync(habitacion.IdTipoHabitacion); return View(habitacion);
    }
    public async Task<IActionResult> Edit(int? id) { if (id is null) return NotFound(); var h = await repositorio.ObtenerAsync(id.Value); if (h is null) return NotFound(); await CargarTiposAsync(h.IdTipoHabitacion); return View(h); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return NotFound();
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation)); ModelState.Remove(nameof(Habitacion.Reservas));
        if (await repositorio.NumeroExisteAsync(habitacion.Numero, id)) ModelState.AddModelError("Numero", "Ya existe una habitación con ese número.");
        if (ModelState.IsValid) { if (await repositorio.ActualizarAsync(id, habitacion.Numero, habitacion.Piso, habitacion.IdTipoHabitacion)) { TempData["Exito"] = "Habitación actualizada correctamente."; return RedirectToAction(nameof(Index)); } return NotFound(); }
        await CargarTiposAsync(habitacion.IdTipoHabitacion); return View(habitacion);
    }
    public async Task<IActionResult> Delete(int? id) { if (id is null) return NotFound(); var h = await repositorio.ObtenerAsync(id.Value); return h is null ? NotFound() : View(h); }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) { if (!await repositorio.DesactivarAsync(id)) return NotFound(); TempData["Exito"] = "Habitación desactivada correctamente."; return RedirectToAction(nameof(Index)); }
    private async Task CargarTiposAsync(int? seleccionado = null) => ViewBag.Tipos = new SelectList(await repositorio.TiposActivosAsync(), "IdTipoHabitacion", "Nombre", seleccionado);
}
