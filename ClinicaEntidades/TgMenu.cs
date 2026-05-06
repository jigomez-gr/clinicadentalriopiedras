using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClinicaEntidades
{
    public class TgMenu
    {
        public int IdMenu { get; set; }
        public string? Nombre { get; set; }
        public string? Icono { get; set; }
        public int? ClaveOrdenacion { get; set; }
        public int? IdRolUsuario { get; set; }

        // Propiedad de navegación para los submenús relacionados
        public List<TgSubmenu> Submenus { get; set; } = new List<TgSubmenu>();
    }
}
