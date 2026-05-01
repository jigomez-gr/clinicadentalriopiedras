using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class TelegramCitaTemp
    {
        // ID es el único no nulable según el esquema identity
        public int IdTemp { get; set; }

        public string? WorkflowId { get; set; }
        public string? ChatId { get; set; }
        public string? MessageId { get; set; }
        public string? TelefonoMovil { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdDoctorHorarioDetalle { get; set; }
        public int? IdEstadoCita { get; set; }
        public DateTime? FechaCita { get; set; }
        public string? Indicaciones { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? OrigenCita { get; set; }
        public string? RazonCitaUsr { get; set; }
        public byte[]? DocumentoCitaUsr { get; set; }
        public string? ContentType { get; set; }
        public byte[]? DocIndicacionesDoctor { get; set; }
        public string? ContentTypeDoctor { get; set; }
        public string? MetodoPeticion { get; set; }
        public string? NombreEspecialidad { get; set; }
        public string? NombreYValDoctor { get; set; }
        public int? IdEspecialidad { get; set; }
        public int? IdDoctor { get; set; }
        public string? Fecha { get; set; }
        public string? Hora { get; set; }
        public string? ArchivoCaption { get; set; }
        public string? CitaConfirmada { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public int? ValDoctorCita { get; set; }
        public string? OpinionDoctorYClinica { get; set; }
        public DateTime? FechaPeticion { get; set; }
        public string? Repasado { get; set; }
        public string? CalBookingUid { get; set; }
        public string? CalSyncStatus { get; set; }
        public DateTimeOffset? CalLastSyncAt { get; set; } // Timestamp with time zone
        public string? CalLastError { get; set; }
        public long? CalLastOutboxId { get; set; } // Bigint
    }
}
