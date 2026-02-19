using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class ZadarmaSmsRespuesta
    {
        public int IdRespuestaSms { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? HttpStatusCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Messages { get; set; }
        public decimal CostTotal { get; set; }
        public string Currency { get; set; } = string.Empty;

        public string? CallerId { get; set; }
        public string NumeroDestino { get; set; } = string.Empty;

        public decimal Cost { get; set; }
        public decimal CostMin { get; set; }
        public decimal CostMax { get; set; }

        public string Mensaje { get; set; } = string.Empty;
        public int Parts { get; set; }

        public string? DeniedNumbers { get; set; }
        public string? RawJsonRespuesta { get; set; }
    }
}

