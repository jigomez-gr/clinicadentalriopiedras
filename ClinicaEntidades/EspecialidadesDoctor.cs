using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{

    public class EspecialidadesDoctor
    {
        public int IdEspecialidadDoctor { get; set; }
        public int IdDoctor { get; set; }
        public int IdEspecialidad { get; set; }
        public string NombreEspecialidad { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
    }




}
