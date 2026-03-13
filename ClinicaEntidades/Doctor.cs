using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class Doctor
    {
        public int IdDoctor { get; set; }
        public string NumeroDocumentoIdentidad { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public string FechaCreacion { get; set; } = null!;

        // --- CAMPOS NUEVOS ---
        public byte[]? Archivo { get; set; }
        public string? Biografia { get; set; }
        public decimal Valoracion { get; set; } = 3;
        public decimal ValoracionAi { get; set; } = 3;
        public string? ResumenValoracion { get; set; }
        public string HaceLimpiezas { get; set; } = "N";

        // RELACIÓN NUEVA (La que usaremos de ahora en adelante)
        public List<EspecialidadesDoctor> EspecialidadesDoc { get; set; } = new List<EspecialidadesDoctor>();

        // --- PARCHE DE COMPATIBILIDAD ---
        // Esto es para que no falle el código viejo que busca ".Especialidad"
        // Una vez que arreglemos los DAOs y Vistas, podremos quitarlo.
        public Especialidad Especialidad { get; set; } = new Especialidad();

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }


}
