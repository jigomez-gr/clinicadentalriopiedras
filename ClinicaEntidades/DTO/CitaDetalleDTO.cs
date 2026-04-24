using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades.DTO
{

    public class CitaPendienteTelegramDTO
    {
        public int IdCita { get; set; }
        public string FechaCita { get; set; }
        public string HoraCita { get; set; }
        public string NombreEspecialidad { get; set; }
        public string NombreDoctor { get; set; }
        public string RazonCitaUsr { get; set; }
    }
}
