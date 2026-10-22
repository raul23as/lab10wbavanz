namespace Hotel.Web.ViewModels;

public class HabitacionDisponibleViewModel
{
    public int IdHabitacion { get; set; }
    public string Numero { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Noches { get; set; }
    public decimal TotalEstimado { get; set; }
}
