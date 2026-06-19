using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class Cita
    {
        public int IdCita { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public DoctorHorarioDetalle DoctorHorarioDetalle { get; set; } = null!;
        public EstadoCita EstadoCita { get; set; } = null!;
        public string FechaCita { get; set; } = null!;
        public string FechaCreacion { get; set; } = null!;
        public string Indicaciones { get; set; } = string.Empty;

        public Especialidad Especialidad { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public string HoraCita { get; set; } = null!;

        // Campos del paciente
        public string OrigenCita { get; set; } = string.Empty;      // [OrigenCita] varchar(255) NULL
        public string RazonCitaUsr { get; set; } = string.Empty;    // [RazonCitaUsr] varchar(max) NULL
        public byte[]? DocumentoCitaUsr { get; set; }               // [DocumentoCitaUsr] varbinary(max) NULL
        public string? ContentType { get; set; }                    // [contenttype] varchar(50) NULL

        // Campos del doctor
        public byte[]? DocIndicacionesDoctor { get; set; }          // [DocIndicacionesDoctor] varbinary(max) NULL
        public string? ContentTypeDoctor { get; set; }              // [contenttype_doctor] varchar(50) NULL

        // ===== NUEVOS CAMPOS (confirmación) =====
        // varchar(1): por ejemplo "S"/"N" o "1"/"0" según uses
        public string? CitaConfirmada { get; set; }
        public DateTime? FechaPeticion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public string? MetodoPeticion { get; set; }
        public DateTime? FechaCitaOrden { get; set; }
        // ===== CAMPOS DE VALORACIÓN =====
        public int ValDoctorCita { get; set; } = 3;                 // valdoctorcita integer DEFAULT 3
        public string? OpinionDoctorYClinica { get; set; }          // opiniondoctoryclinica text NULL

        // ===== CAMPOS TIPO, PRECIO Y PAGO =====
        public string TipoDeCita { get; set; } = "P";           // tipodecita char(1) DEFAULT 'P'
        public decimal? Precio { get; set; }                     // precio numeric(10,2) NULL
        public string Pagada { get; set; } = "N";               // pagada varchar(1) DEFAULT 'N'
        public string? UrlCitaOTelefono { get; set; }           // urlcitaotelefono varchar(80) NULL
        public string? DiagnosticoIA { get; set; }              // diagnosticoia text NULL
        public string? ResumenCitaVirtual { get; set; }         // resumencitavirtual text NULL
        public string? CalBookingUid { get; set; }      // cal_booking_uid text NULL

    }
}
