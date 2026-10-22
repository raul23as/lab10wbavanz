using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio(HotelContext contexto)
{
    public async Task<List<Habitacion>> ListarActivasAsync(int? tipoId = null)
    {
        var consulta = contexto.Habitacions.AsNoTracking().Include(h => h.IdTipoHabitacionNavigation).Where(h => h.Activo);
        if (tipoId.HasValue) consulta = consulta.Where(h => h.IdTipoHabitacion == tipoId.Value);
        return await consulta.OrderBy(h => h.Numero).ToListAsync();
    }

    public Task<Habitacion?> ObtenerAsync(int id) => contexto.Habitacions.AsNoTracking().Include(h => h.IdTipoHabitacionNavigation).FirstOrDefaultAsync(h => h.IdHabitacion == id);
    public Task<bool> NumeroExisteAsync(string numero, int? excluirId = null) => contexto.Habitacions.AsNoTracking().AnyAsync(h => h.Numero == numero && (!excluirId.HasValue || h.IdHabitacion != excluirId.Value));
    public Task<List<TipoHabitacion>> TiposActivosAsync() => contexto.TipoHabitacions.AsNoTracking().Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync();
    public async Task CrearAsync(Habitacion habitacion) { contexto.Habitacions.Add(habitacion); await contexto.SaveChangesAsync(); }
    public async Task<bool> ActualizarAsync(int id, string numero, int piso, int tipoId)
    {
        var entidad = await contexto.Habitacions.FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (entidad is null) return false;
        entidad.Numero = numero; entidad.Piso = piso; entidad.IdTipoHabitacion = tipoId;
        await contexto.SaveChangesAsync(); return true;
    }
    public async Task<bool> DesactivarAsync(int id)
    {
        var entidad = await contexto.Habitacions.FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (entidad is null) return false;
        entidad.Activo = false; await contexto.SaveChangesAsync(); return true;
    }
}
