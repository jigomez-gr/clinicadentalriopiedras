using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class DoctorHorarioDetalleCalcom
    {
        // Clave primaria (bigint -> long)
        public long IdHorarioDetalleCalcom { get; set; }

        // Relaciones locales
        public int? IdDoctorHorarioDetalle { get; set; }
        public int? IdDoctor { get; set; }
        public int? IdCita { get; set; }

        // Datos del Slot
        public DateTime? FechaSlot { get; set; }
        public string? TurnoHora { get; set; } // Lo manejamos como string para el formato HH:mm

        // Datos de Cal.com (Booking)
        public string? BookingUid { get; set; }
        public string? BookingStatus { get; set; }

        // Datos del Paciente extraídos
        public string? PacienteNombreCompleto { get; set; }
        public string? PacienteApellido { get; set; }
        public string? PacienteEmail { get; set; }
        public string? PacienteMovil { get; set; }

        // Notas y descripciones
        public string? Razoncitausr { get; set; }
        public string? Indicaciones { get; set; }

        // El JSON completo (jsonb -> string)
        public string? CalJsonFull { get; set; }

        // Auditoría y Sincronización
        public DateTime? UltimaSincronizacion { get; set; } = DateTime.Now;
        public DateTime? FechaSincro { get; set; } = DateTime.Now;
        public string? CalLastError { get; set; }
        public string? ResultSincro { get; set; } // "S" o "N" (o OK/KO según prefieras)
    }

}
