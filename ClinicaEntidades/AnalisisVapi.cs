using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ClinicaEntidades
{
    public class AnalisisVapi
    {
        public int IdLlamada { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }

        public string? IdAsistente { get; set; }
        public string? NombreAsistente { get; set; }

        public decimal? Coste { get; set; }

        // Formateado como dd/MM/yyyy HH:mm en el SP
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }

        public decimal? DuracionMinutos { get; set; }

        public string? Transcripcion { get; set; }
        public string? SugerenciaMejora { get; set; }

        public int? Validacion { get; set; }

        public string? Resumen { get; set; }
        public string? Telefono { get; set; }

        // Para no mandar base64 gigante en el listado
        public bool TieneAudio { get; set; }

        public string? EmailCompleto { get; set; }
        public string? NombreCompleto { get; set; }
    }
}
