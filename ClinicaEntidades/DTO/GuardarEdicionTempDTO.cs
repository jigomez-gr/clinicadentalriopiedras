using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades.DTO
{
    public class GuardarEdicionTempDTO
    {
        public string ChatId { get; set; }
        public string Notas { get; set; }
        public string ImagenBase64 { get; set; }
        // Añadimos estos para la visualización en el HTML
        public string NombreDoctor { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
    }
}
