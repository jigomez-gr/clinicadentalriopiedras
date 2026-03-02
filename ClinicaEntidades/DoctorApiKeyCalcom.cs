using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class DoctorApiKeyCalcom
    {
        public int IdDoctor { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? NombreYApellido { get; set; }
        public string ApiKey { get; set; } = "";
        public string? CalUsername { get; set; }
        public int? CalEventTypeId { get; set; }
        public string? CalPublicUrl { get; set; }
        public string EventInbox { get; set; } = "DEFAULT";
        public string? EventInboxUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // --- NUEVOS CAMPOS ---

        /// <summary>
        /// URL base de la API (https://api.cal.com/v2 o https://api.cal.eu/v2)
        /// </summary>
        public string? ApiBase { get; set; }

        /// <summary>
        /// Nombre descriptivo del calendario/evento obtenido de Cal.com
        /// </summary>
        public string? CalEventName { get; set; }

        /// <summary>
        /// Frecuencia de los slots (15, 30, 60 min) obtenida de la API
        /// </summary>
        public int? CalSlotInterval { get; set; }
    }
}