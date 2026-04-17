using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class BitacoraViewModel
    {
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Fecha { get; set; }
        public List<BitacoraItemViewModel> Resultados { get; set; } = new();
    }

    public class BitacoraItemViewModel
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

}