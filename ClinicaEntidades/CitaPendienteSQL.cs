using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    // Este DTO sirve para mapear la salida del SQL en el Repositorio
  
    public class CitaPendienteSQL
    {
        public int idcita { get; set; }
        public DateTime fechacita { get; set; }
        public TimeOnly turnohora { get; set; }
        public string nombre_especialidad { get; set; }
        public string nombre_doctor { get; set; } // Campo para el nombre del médico
        public string razon { get; set; }
    }
}
