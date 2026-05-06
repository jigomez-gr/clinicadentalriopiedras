using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClinicaEntidades
{
    // Clase para la tabla public.tg_submenu 📑
    public class TgSubmenu
    {
        public int IdSubmenu { get; set; }
        public int IdMenu { get; set; }
        public string? Nombre { get; set; }
        public string? Icono { get; set; }
        public int? ClaveOrdenacion { get; set; }
        public string? Opcion { get; set; }
    }
}
