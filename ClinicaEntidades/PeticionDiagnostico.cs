using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class PeticionDiagnostico
    {
        public int IdPeticion { get; set; }
        public Usuario Usuario { get; set; } = null!;                 // contacto (landingclinica.usuario)
        public int? IdServicioIA { get; set; }
        public string ServicioCodigo { get; set; } = string.Empty;    // 'dental','rx','derma','belleza','general'
                                                                      // Doctor destino (validado contra public.usuario con idrolusuario = 2)
        public int? IdUsuarioDoctor { get; set; }
        public string DoctorNombre { get; set; } = string.Empty;
        public string DoctorCorreo { get; set; } = string.Empty;
        // Canal por el que el paciente quiere que le respondan
        public string CanalRespuesta { get; set; } = string.Empty;    // 'email' | 'whatsapp' | 'telegram'
        public string ContactoRespuesta { get; set; } = string.Empty; // snapshot correo/movil/telegram_id usado
                                                                      // Contenido de la petición
        public string? MotivoPaciente { get; set; }
        public byte[]? Imagen { get; set; }
        public string? ImagenContentType { get; set; }
        public string? DiagnosticoIA { get; set; }
        // Consentimiento explícito
        public bool Consentimiento { get; set; }
        public DateTime? FechaConsentimiento { get; set; }
        public string? IpOrigen { get; set; }
        // Trazabilidad de envío
        public string Estado { get; set; } = "pendiente";             // pendiente|enviada|respondida|error
        public bool CorreoEnviado { get; set; }
        public DateTime? FechaEnvioCorreo { get; set; }
        public string? MensajeError { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }

}
