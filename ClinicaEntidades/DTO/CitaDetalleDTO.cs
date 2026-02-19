using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades.DTO
{
    
    public class CitaDetalleDTO
    {
        public int IdCita { get; set; }
        public string? CitaConfirmada { get; set; }         // "S"/"N" o "1"/"0"
        public DateTime? FechaPeticion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public string? MetodoPeticion { get; set; }
    }
}
