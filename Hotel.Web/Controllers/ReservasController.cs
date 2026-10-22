using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController(ReservaRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso?.ToString("yyyy-MM-dd"); ViewBag.Salida = salida?.ToString("yyyy-MM-dd");
        if (!ingreso.HasValue || !salida.HasValue) return View(new List<ViewModels.HabitacionDisponibleViewModel>());
        if (salida <= ingreso) { ModelState.AddModelError("salida", "La salida debe ser posterior al ingreso."); return View(new List<ViewModels.HabitacionDisponibleViewModel>()); }
        return View(await repositorio.BuscarDisponiblesAsync(ingreso.Value, salida.Value));
    }
    public async Task<IActionResult> Index() { ViewBag.Exito = TempData["Exito"]; ViewBag.Error = TempData["Error"]; return View(await repositorio.ListarAsync()); }
    public async Task<IActionResult> Create() { await CargarListasAsync(); return View(new Reserva()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida")] Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation)); ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        if (reserva.FechaSalida <= reserva.FechaIngreso) ModelState.AddModelError(nameof(Reserva.FechaSalida), "La salida debe ser posterior al ingreso.");
        if (reserva.FechaIngreso < DateOnly.FromDateTime(DateTime.Today)) ModelState.AddModelError(nameof(Reserva.FechaIngreso), "El ingreso no puede ser anterior a hoy.");
        if (reserva.FechaSalida > reserva.FechaIngreso && await repositorio.EstaReservadaAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida)) ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango.");
        if (ModelState.IsValid) { reserva.Estado = "Pendiente"; await repositorio.CrearAsync(reserva); TempData["Exito"] = "Reserva creada correctamente."; return RedirectToAction(nameof(Index)); }
        await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion); return View(reserva);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id) { if (!await repositorio.CancelarAsync(id)) TempData["Error"] = "No se encontró la reserva o su estado no permite cancelarla."; else TempData["Exito"] = "Reserva cancelada correctamente."; return RedirectToAction(nameof(Index)); }
    private async Task CargarListasAsync(int? huesped = null, int? habitacion = null)
    {
        var huespedes = await repositorio.HuespedesActivosAsync(); var habitaciones = await repositorio.HabitacionesActivasAsync();
        ViewBag.Huespedes = huespedes.Select(h => new SelectListItem { Value = h.IdHuesped.ToString(), Text = $"{h.Apellidos}, {h.Nombres}", Selected = h.IdHuesped == huesped }).ToList();
        ViewBag.Habitaciones = habitaciones.Select(h => new SelectListItem { Value = h.IdHabitacion.ToString(), Text = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre}", Selected = h.IdHabitacion == habitacion }).ToList();
    }
}
