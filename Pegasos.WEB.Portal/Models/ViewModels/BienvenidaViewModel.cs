namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class BienvenidaViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string HoraIngreso { get; set; } = string.Empty;
    }
}