using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades.DTO
{
    public class CitaActualizarPacienteDto
    {
        public int IdCita { get; set; }
        public string? RazonCitaUsr { get; set; }

        /// <summary>
        /// Documento del paciente en Base64 (opcional)
        /// </summary>
        public string? DocumentoCitaUsrBase64 { get; set; }

        /// <summary>
        /// content-type del archivo (application/pdf, image/png, …)
        /// </summary>
        public string? ContentType { get; set; }
    }
    public class FechaAtencionDTO
    {
        public string Fecha { get; set; } = null!;
        public List<HorarioDTO> HorarioDTO { get; set; } = null!;
    }
    public class ActualizarMotivoPacienteRequest
    {
        public int IdCita { get; set; }
        public string? RazonCitaUsr { get; set; }
        public string? OrigenCita { get; set; }
        public string? DocumentoBase64 { get; set; }
        public string? ContentType { get; set; }
    }
}
