using Hotel.Web.Models;
using Hotel.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio(HotelContext contexto)
{
    public Task<List<HabitacionDisponibleViewModel>> BuscarDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        var noches = salida.DayNumber - ingreso.DayNumber;
        return contexto.Habitacions.AsNoTracking().Where(h => h.Activo && !h.Reservas.Any(r => r.Estado != "Cancelada" && r.FechaIngreso < salida && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero).Select(h => new HabitacionDisponibleViewModel { IdHabitacion = h.IdHabitacion, Numero = h.Numero, Tipo = h.IdTipoHabitacionNavigation.Nombre, Noches = noches, TotalEstimado = noches * h.IdTipoHabitacionNavigation.PrecioNoche }).ToListAsync();
    }
    public Task<bool> EstaReservadaAsync(int habitacionId, DateOnly ingreso, DateOnly salida, int? excluirId = null) => contexto.Reservas.AsNoTracking().AnyAsync(r => r.IdHabitacion == habitacionId && r.Estado != "Cancelada" && (!excluirId.HasValue || r.IdReserva != excluirId.Value) && r.FechaIngreso < salida && r.FechaSalida > ingreso);
    public Task<List<Huesped>> HuespedesActivosAsync() => contexto.Huespeds.AsNoTracking().Where(h => h.Activo).OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres).ToListAsync();
    public Task<List<Habitacion>> HabitacionesActivasAsync() => contexto.Habitacions.AsNoTracking().Include(h => h.IdTipoHabitacionNavigation).Where(h => h.Activo).OrderBy(h => h.Numero).ToListAsync();
    public Task<List<ReservaListaViewModel>> ListarAsync() => contexto.Reservas.AsNoTracking().Include(r => r.IdHuespedNavigation).Include(r => r.IdHabitacionNavigation).ThenInclude(h => h.IdTipoHabitacionNavigation).OrderByDescending(r => r.FechaRegistro).Select(r => new ReservaListaViewModel { IdReserva = r.IdReserva, Huesped = r.IdHuespedNavigation.Apellidos + ", " + r.IdHuespedNavigation.Nombres, Habitacion = r.IdHabitacionNavigation.Numero, FechaIngreso = r.FechaIngreso, FechaSalida = r.FechaSalida, Estado = r.Estado, Total = (r.FechaSalida.DayNumber - r.FechaIngreso.DayNumber) * r.IdHabitacionNavigation.IdTipoHabitacionNavigation.PrecioNoche }).ToListAsync();
    public async Task CrearAsync(Reserva reserva) { contexto.Reservas.Add(reserva); await contexto.SaveChangesAsync(); }
    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await contexto.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null || (reserva.Estado != "Pendiente" && reserva.Estado != "Confirmada")) return false;
        reserva.Estado = "Cancelada"; await contexto.SaveChangesAsync(); return true;
    }
}
