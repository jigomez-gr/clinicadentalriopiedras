using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClinicaEntidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NumeroDocumentoIdentidad { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Clave { get; set; } = null!;
        public RolUsuario RolUsuario { get; set; } = null!;
        public string FechaCreacion { get; set; } = null!;
        public string Movil { get; set; } = null!;
        // Propiedad plana para capturar el nombre del rol desde el JOIN
        public string NombreRol { get; set; } = null!;
        public string Telegram_Id { get; set; } = null!;
        // ... otros campos (Correo, Clave, etc.)

    }
}
