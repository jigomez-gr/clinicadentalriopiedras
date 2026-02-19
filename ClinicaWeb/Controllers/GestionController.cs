using System.IO;
using System.Threading.Tasks;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaWeb.Controllers
{
    /// <summary>
    /// Pantalla de administración de citas.
    /// Solo accesible para usuarios con rol "Administrador".
    /// </summary>
    [Authorize(Roles = "Administrador,Doctor")]
    public class GestionController : Controller
    {
        private readonly ICitaRepositorio _repositorioCita;

        public GestionController(ICitaRepositorio repositorioCita)
        {
            _repositorioCita = repositorioCita;
        }

        /// <summary>
        /// GET /Gestion/Index
        /// Muestra la vista de gestión (Views/Gestion/Index.cshtml).
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET /Gestion/ListaCitasGestion
        /// Devuelve todas las citas para el DataTable de administración.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListaCitasGestion()
        {
            var lista = await _repositorioCita.ListaCitasGestion();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        /// <summary>
        /// POST /Gestion/AdminActualizarCita
        /// Actualiza TODOS los datos de la cita (paciente + doctor) como administrador.
        /// Se llama desde el formulario de la vista de Gestión con FormData.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdminActualizarCita(
            int IdCita,
            int IdEstadoCita,
            string OrigenCita,
            string RazonCitaUsr,
            string IndicacionesDoctor,
            IFormFile? DocumentoPaciente,
            IFormFile? DocumentoDoctor)
        {
            if (IdCita <= 0)
            {
                return BadRequest(new { data = "IdCita inválido" });
            }

            // Montamos el objeto Cita igual que haces en CitasController :contentReference[oaicite:0]{index=0}
            var cita = new Cita
            {
                IdCita = IdCita,
                EstadoCita = new EstadoCita { IdEstadoCita = IdEstadoCita },
                OrigenCita = OrigenCita ?? string.Empty,
                RazonCitaUsr = RazonCitaUsr ?? string.Empty,
                Indicaciones = IndicacionesDoctor ?? string.Empty
            };

            // Documento del PACIENTE (opcional)
            if (DocumentoPaciente != null && DocumentoPaciente.Length > 0)
            {
                using var msPac = new MemoryStream();
                await DocumentoPaciente.CopyToAsync(msPac);
                cita.DocumentoCitaUsr = msPac.ToArray();
                cita.ContentType = DocumentoPaciente.ContentType;
            }

            // Documento del DOCTOR (opcional)
            if (DocumentoDoctor != null && DocumentoDoctor.Length > 0)
            {
                using var msDoc = new MemoryStream();
                await DocumentoDoctor.CopyToAsync(msDoc);
                cita.DocIndicacionesDoctor = msDoc.ToArray();
                cita.ContentTypeDoctor = DocumentoDoctor.ContentType;
            }

            // Llamamos al repositorio para actualizar la cita
            var respuesta = await _repositorioCita.AdminActualizarCita(cita);

            // Si el SP devolvió mensaje de error, lo devolvemos como 400
            if (!string.IsNullOrEmpty(respuesta))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = respuesta });
            }

            // OK sin errores
            return StatusCode(StatusCodes.Status200OK, new { data = "" });
        }
    }
}
