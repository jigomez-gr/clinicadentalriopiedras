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
    }
}