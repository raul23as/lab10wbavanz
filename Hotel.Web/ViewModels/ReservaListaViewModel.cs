namespace Hotel.Web.ViewModels;

public class ReservaListaViewModel
{
    public int IdReserva { get; set; }
    public string Huesped { get; set; } = "";
    public string Habitacion { get; set; } = "";
    public DateOnly FechaIngreso { get; set; }
    public DateOnly FechaSalida { get; set; }
    public string Estado { get; set; } = "";
    public decimal Total { get; set; }
}
