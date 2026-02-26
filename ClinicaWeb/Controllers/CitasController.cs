using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using ClinicaEntidades.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicaWeb.Controllers
{

    public class Conexion
    {
        public static string CN = "Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9;Pooling=true;Maximum Pool Size=20;Minimum Pool Size=0;Timeout=15;";
    }
    public class CitasController : Controller
    {

        private readonly IDoctorRepositorio _repositorioDoctor;
        private readonly ICitaRepositorio _repositorioCita;

        public CitasController(IDoctorRepositorio repositorioDoctor, ICitaRepositorio repositorioCita)
        {
            _repositorioDoctor = repositorioDoctor;
            _repositorioCita = repositorioCita;
        }
       

        // ===================== VISTAS =====================

        [Authorize(Roles = "Paciente")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Vista de nueva cita.
        /// - Paciente: crea su propia cita (ignora idUsuarioPaciente).
        /// - Admin: debe venir idUsuarioPaciente con el Id del paciente para el que se crea la cita.
        /// </summary>
        [Authorize(Roles = "Paciente,Administrador,Doctor")]
        public IActionResult NuevaCita(int? idUsuarioPaciente)
        {
            int idParaView = 0;

            if (User.IsInRole("Paciente"))
            {
                // El paciente siempre crea cita para sí mismo.
                // Lo resolveremos en el backend usando el Claim.
                idParaView = 0;
            }
            else if (User.IsInRole("Administrador") || User.IsInRole("Doctor"))
            {
                // El administrador debe indicar el Id del paciente
                if (!idUsuarioPaciente.HasValue || idUsuarioPaciente.Value <= 0)
                {
                    // Para no romper navegación dejamos 0, pero el Guardar exigirá paciente válido.
                    idParaView = 0;
                }
                else
                {
                    idParaView = idUsuarioPaciente.Value;
                }
            }

            ViewBag.IdUsuarioPaciente = idParaView;
            return View();
        }

        [Authorize(Roles = "Doctor")]
        public IActionResult CitasAsignadas()
        {
            return View();
        }

        // La vista de admin de gestión de citas está en GestionController / Gestion/Index
        // Aquí solo exponemos los endpoints de datos:
        //   - /Citas/ListaCitasGestion
        //   - /Citas/AdminActualizarCita

        // ===================== ENDPOINTS COMUNES (PACIENTE / ADMIN) =====================

        /// <summary>
        /// Devuelve los slots de horario de un doctor.
        /// Usado por NuevaCita.js
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListaDoctorHorarioDetalle(int Id)
        {
            List<FechaAtencionDTO> lista = await _repositorioDoctor.ListaDoctorHorarioDetalle(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        /// <summary>
        /// Guardar una nueva cita
        /// - Paciente: se fuerza el IdUsuario desde el Claim.
        /// - Admin: debe venir objeto.Usuario.IdUsuario en el JSON.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Paciente,Administrador,Doctor")]
        public async Task<IActionResult> Guardar([FromBody] Cita objeto)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            // Usuario autenticado
            ClaimsPrincipal claimuser = HttpContext.User;
            string idUsuarioClaim = claimuser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault()!;

            if (User.IsInRole("Paciente"))
            {
                // El paciente SIEMPRE crea su propia cita
                objeto.Usuario = new Usuario
                {
                    IdUsuario = int.Parse(idUsuarioClaim)
                };
            }
            else if (User.IsInRole("Administrador"))
            {
                // El admin debe indicar el paciente explícitamente
                if (objeto.Usuario == null || objeto.Usuario.IdUsuario <= 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new { data = "Debe indicar un paciente válido para crear la cita (Usuario.IdUsuario)." });
                }
            }

            string respuesta = await _repositorioCita.Guardar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        /// <summary>
        /// Lista de citas pendientes para el Paciente logueado (Mis Citas)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> ListaCitasPendiente()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            string idUsuario = claimuser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault()!;

            List<Cita> lista = await _repositorioCita.ListaCitasPendiente(int.Parse(idUsuario));
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        /// <summary>
        /// Cancelar una cita (Paciente)
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> Cancelar(int Id)
        {
            string respuesta = await _repositorioCita.Cancelar(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        // ===================== DOCTOR / ADMIN: CAMBIAR ESTADO =====================

        /// <summary>
        /// Cambiar el estado de una cita (usado por CitasAsignadas.js)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> CambiarEstado([FromBody] Cita objeto)
        {
            string indicaciones = objeto.Indicaciones ?? string.Empty;

            string respuesta = await _repositorioCita.CambiarEstado(
                objeto.IdCita,
                objeto.EstadoCita.IdEstadoCita,
                indicaciones
            );

            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        // ===================== PACIENTE / ADMIN: ACTUALIZAR MOTIVO + DOC =====================

        /// <summary>
        /// Actualiza el motivo y el documento del paciente (Mis Citas).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Paciente,Administrador")]
        public async Task<IActionResult> ActualizarMotivoPaciente([FromBody] ActualizarMotivoPacienteRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { data = "Modelo inválido en ActualizarMotivoPaciente" });
            }

            byte[]? docBytes = null;

            if (!string.IsNullOrWhiteSpace(modelo.DocumentoBase64))
            {
                try
                {
                    docBytes = Convert.FromBase64String(modelo.DocumentoBase64);
                }
                catch (FormatException ex)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new { data = "DocumentoBase64 inválido", detail = ex.Message });
                }
            }

            string razon = modelo.RazonCitaUsr ?? string.Empty;

            string respuesta = await _repositorioCita.ActualizarMotivoPaciente(
                modelo.IdCita,
                razon,
                docBytes,
                modelo.ContentType
            );

            // respuesta = "" si todo OK
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        // ===================== ADMIN: LISTADO Y ACTUALIZACIÓN COMPLETA =====================

        /// <summary>
        /// Listado global para admin (usado por Gestión de Citas - DataTable)
        /// GET /Citas/ListaCitasGestion
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListaCitasGestion(int idEstadoCita = 0)
        {
            // delegamos en el repositorio; el 0 significa "sin filtro"
            var lista = await _repositorioCita.ListaCitasGestion(idEstadoCita);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        /// <summary>
        /// Actualiza una cita completa como admin:
        /// - Estado
        /// - OrigenCita
        /// - RazonCitaUsr
        /// - Indicaciones (doctor)
        /// - Documento paciente
        /// - Documento doctor
        /// </summary>
        /// [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarCitaAdminConfirmacion(int idCita, string? citaConfirmada)
        {
            // Normaliza a S/N/null
            citaConfirmada = NormalizarSN(citaConfirmada);

            await _repositorioCita.ActualizarCitaConfirmacionAdmin(idCita, citaConfirmada);

            return Json(new { ok = true });
        }

        private static string? NormalizarSN(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            v = v.Trim();
            if (v.Equals("S", StringComparison.OrdinalIgnoreCase)) return "S";
            if (v.Equals("N", StringComparison.OrdinalIgnoreCase)) return "N";
            if (v.Equals("Si", StringComparison.OrdinalIgnoreCase) || v.Equals("Sí", StringComparison.OrdinalIgnoreCase)) return "S";
            if (v.Equals("No", StringComparison.OrdinalIgnoreCase)) return "N";
            return null;
        }

        [Authorize(Roles = "Administrador")]
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
            var cita = new Cita
            {
                IdCita = IdCita,
                EstadoCita = new EstadoCita { IdEstadoCita = IdEstadoCita },
                OrigenCita = OrigenCita ?? string.Empty,
                RazonCitaUsr = RazonCitaUsr ?? string.Empty,
                Indicaciones = IndicacionesDoctor ?? string.Empty
            };

            // Documento del paciente
            if (DocumentoPaciente != null && DocumentoPaciente.Length > 0)
            {
                using var msPac = new MemoryStream();
                await DocumentoPaciente.CopyToAsync(msPac);
                cita.DocumentoCitaUsr = msPac.ToArray();
                cita.ContentType = DocumentoPaciente.ContentType;
            }

            // Documento del doctor
            if (DocumentoDoctor != null && DocumentoDoctor.Length > 0)
            {
                using var msDoc = new MemoryStream();
                await DocumentoDoctor.CopyToAsync(msDoc);
                cita.DocIndicacionesDoctor = msDoc.ToArray();
                cita.ContentTypeDoctor = DocumentoDoctor.ContentType;
            }

            string respuesta = await _repositorioCita.AdminActualizarCita(cita);

            if (!string.IsNullOrEmpty(respuesta))
            {
                // Hubo error en el SP
                return StatusCode(StatusCodes.Status400BadRequest, new { data = respuesta });
            }

            return StatusCode(StatusCodes.Status200OK, new { data = "" });
        }

        // ===================== NUEVA VISTA: DETALLE HORARIO / CITAS =====================

        /// <summary>
        /// Vista para consultar agenda (horarios + citas) por fecha.
        /// - Administrador: puede elegir especialidad/doctor (desde la vista vía JS).
        /// - Doctor: agenda fija de su propio doctor (por NumeroDocumentoIdentidad).
        /// </summary>
        
        private static bool TryParseFechaFlexible(string? input, out DateTime fecha)
        {
            fecha = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();

            var es = CultureInfo.GetCultureInfo("es-ES");
            var formatos = new[]
            {
                "dd/MM/yyyy", "d/M/yyyy",
                "dd-MM-yyyy", "d-M-yyyy",
                "yyyy-MM-dd", "yyyy/M/d"
            };

            // 1) Exacto con formatos conocidos
            if (DateTime.TryParseExact(input, formatos, es, DateTimeStyles.None, out fecha))
                return true;

            // 2) Fallback (cultura es-ES)
            if (DateTime.TryParse(input, es, DateTimeStyles.None, out fecha))
                return true;

            // 3) Último intento: Invariant
            return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        /// <summary>
        /// Devuelve agenda del día (slots AM/PM) para pintar la pantalla Detalle_Horario_Citas.
        /// </summary>
    

        /// <summary>
        /// Devuelve el detalle completo de una cita (para modal editar/ver).
        /// </summary>
       
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> Detalle_Horario_Citas()
        {
            ViewBag.EsAdmin = User.IsInRole("Administrador") ? 1 : 0;
            ViewBag.EsDoctor = User.IsInRole("Doctor") ? 1 : 0;
            ViewBag.DoctorNombreCompleto = "";
            ViewBag.IdDoctorFijo = 0;

            int idDoctorFijo = 0;

            if (User.IsInRole("Doctor"))
            {
                // Usa lo que ya venías usando: NumeroDocumentoIdentidad
                string? numDoc = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "NumeroDocumentoIdentidad")?.Value;
                if (!string.IsNullOrWhiteSpace(numDoc))
                {
                    var doctores = await _repositorioDoctor.Lista();
                    var doc = doctores.FirstOrDefault(d =>
                        (d.NumeroDocumentoIdentidad ?? "").Trim().Equals(numDoc.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (doc != null)
                    {
                        ViewBag.IdDoctorFijo = doc.IdDoctor;
                        ViewBag.DoctorNombreCompleto = $"{doc.Nombres} {doc.Apellidos}".Trim();
                        return View();
                    }

                }
            }

            ViewBag.IdDoctorFijo = idDoctorFijo;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> DetalleCita(int idCita)
        {
            if (idCita <= 0)
                return BadRequest(new { ok = false, msg = "IdCita inválido." });

            var todas = await _repositorioCita.ListaCitasGestion(0);
            var c = todas.FirstOrDefault(x => x != null && x.IdCita == idCita);

            if (c == null)
                return NotFound(new { ok = false, msg = "Cita no encontrada." });

            return Ok(new { ok = true, msg = "", data = c });
        }

        // ===================== ACTUALIZACIONES (SEGURAS POR PARTES) =====================

        /// <summary>
        /// Admin: actualiza SOLO el motivo (texto) del paciente sin tocar el documento.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AdminActualizarMotivoPacienteTexto([FromBody] AdminActualizarMotivoTextoRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Modelo inválido." });

            var cita = new Cita
            {
                IdCita = modelo.IdCita,
                RazonCitaUsr = modelo.RazonCitaUsr ?? string.Empty
            };

            string respuesta = await _repositorioCita.GuardarMotivoPaciente(cita);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        /// <summary>
        /// Admin: actualiza SOLO el documento del paciente sin tocar el motivo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AdminActualizarDocumentoPaciente([FromBody] AdminActualizarDocumentoRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Modelo inválido." });

            byte[]? docBytes = null;

            if (!string.IsNullOrWhiteSpace(modelo.DocumentoBase64))
            {
                try
                {
                    docBytes = Convert.FromBase64String(modelo.DocumentoBase64);
                }
                catch (FormatException ex)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new { data = "DocumentoBase64 inválido", detail = ex.Message });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Debe adjuntar un documento." });
            }

            var cita = new Cita
            {
                IdCita = modelo.IdCita,
                DocumentoCitaUsr = docBytes,
                ContentType = modelo.ContentType
            };

            string respuesta = await _repositorioCita.GuardarDocumentoPaciente(cita);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<JsonResult> ListaCitasGestionServerSide(int start, int length, string search, int idEstadoCita)
        {
            try
            {
                // 1) leer search[value] si viene así (DataTables)
                if (Request.HasFormContentType)
                {
                    var formSearch = Request.Form["search[value]"];
                    if (!string.IsNullOrEmpty(formSearch))
                        search = formSearch;
                }

                // 2) leer draw
                string draw = "1";
                if (Request.HasFormContentType)
                {
                    var formDraw = Request.Form["draw"];
                    if (!string.IsNullOrEmpty(formDraw))
                        draw = formDraw;
                }

                // 3) leer idEstadoCita si viene por form (por si acaso)
                if (Request.HasFormContentType)
                {
                    var formEstado = Request.Form["idEstadoCita"];
                    if (!string.IsNullOrEmpty(formEstado) && int.TryParse(formEstado, out var tmp))
                        idEstadoCita = tmp;
                }

                // 4) llamar repositorio (tupla)
                var (lista, totalRegistros) =
                    await _repositorioCita.ListaCitasGestionServerSide(idEstadoCita, start, length, search ?? "");

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRegistros,
                    recordsFiltered = totalRegistros,
                    data = lista
                });
            }
            catch (Exception ex)
            {
                ErrorHandler.RegistrarError(
                   Conexion.CN,
                    "ListaCitasGestionServerSide",
                    "CitasController",
                    "Token",
                    "DBClinica",
                    ex,
                    "",
                    "",
                    "",
                    "",
                    ""
                );

                return Json(new
                {
                    error = true,
                    message = "Ocurrió un error al obtener los datos.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Doctor/Admin: actualiza SOLO el documento del doctor (indicaciones adjuntas) sin tocar el resto.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult>  ActualizarDocumentoDoctor([FromBody] DoctorActualizarDocumentoRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Modelo inválido." });

            // Seguridad adicional para doctor: solo su cita
            if (User.IsInRole("Doctor"))
            {
                var lista = await _repositorioCita.ListaCitasGestion(0);
                var cita = lista.FirstOrDefault(x => x.IdCita == modelo.IdCita);

                if (cita == null) return StatusCode(StatusCodes.Status404NotFound, new { data = "Cita no encontrada." });

                int idDoctorClaim = 0;

                string? numDoc = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "NumeroDocumentoIdentidad")?.Value;
                if (!string.IsNullOrWhiteSpace(numDoc))
                {
                    var doctores = await _repositorioDoctor.Lista();
                    var doc = doctores.FirstOrDefault(d =>
                        (d.NumeroDocumentoIdentidad ?? string.Empty).Trim().Equals(numDoc.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (doc != null) idDoctorClaim = doc.IdDoctor;
                }

                if (idDoctorClaim <= 0 || cita.Doctor == null || cita.Doctor.IdDoctor != idDoctorClaim)
                    return Forbid();
            }

            byte[]? docBytes = null;

            if (!string.IsNullOrWhiteSpace(modelo.DocumentoBase64))
            {
                try
                {
                    docBytes = Convert.FromBase64String(modelo.DocumentoBase64);
                }
                catch (FormatException ex)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new { data = "DocumentoBase64 inválido", detail = ex.Message });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Debe adjuntar un documento." });
            }

            string respuesta = await _repositorioCita.ActualizarDocIndicacionesDoctor(
                modelo.IdCita,
                docBytes,
                modelo.ContentTypeDoctor
            );

            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        // ===================== REQUESTS (DTOs) =====================
        /* cambio comienzo */
        /* cambio comienzo */
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> ConsultarEstadoAccion(int id)
        {
            // Validación de seguridad para el ID recibido
            if (id <= 0)
            {
                return BadRequest(new { ok = false, data = "ID de acción inválido" });
            }

            try
            {
                // Usamos la cadena de conexión de tu clase Conexion
                await using var conexion = new Npgsql.NpgsqlConnection(Conexion.CN);
                await conexion.OpenAsync();

                using var cmd = new Npgsql.NpgsqlCommand(
                    "SELECT repasado, respuesta_log FROM public.accionescomplementarias WHERE idaccionescomplementarias = @id",
                    conexion);

                cmd.Parameters.AddWithValue("id", id);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Extraemos los valores con seguridad contra nulos
                    string estadoRaw = reader.IsDBNull(0) ? "N" : reader.GetString(0);
                    string logRaw = reader.IsDBNull(1) ? "" : reader.GetString(1);

                    // Blindaje total: 
                    // 1. Trim() elimina espacios en blanco (evita fallos si la columna es CHAR)
                    // 2. ToUpper() asegura que la comparación en JS contra 'S' sea siempre verdadera
                    var respuesta = new
                    {
                        status = estadoRaw.Trim().ToUpper(), // Devuelve 'S', 'N', 'Z', 'P', etc.
                        log = logRaw
                    };

                    return Ok(respuesta);
                }

                // Si el reader no encuentra la fila
                return NotFound(new { ok = false, data = "Acción no encontrada en la base de datos" });
            }
            catch (Exception ex)
            {
                // En caso de error de conexión o SQL
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, data = ex.Message });
            }
        }
        /* cambio comienzo */
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador,Paciente")]
        public async Task<IActionResult> ReprogramarCita([FromBody] ReprogramarCitaRequest modelo)
        {
            if (modelo == null || modelo.IdCitaVieja <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { ok = false, data = "Datos inválidos." });

            string docEjecutor = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "NumeroDocumentoIdentidad")?.Value ?? "";

            var resultado = await _repositorioCita.EncolarReprogramacion(
                modelo.IdCitaVieja,
                modelo.NuevaFecha,
                modelo.NuevaHora,
                modelo.Motivo,
                docEjecutor
            );

            if (!resultado.ok)
            {
                return Ok(new { ok = false, data = resultado.msg });
            }

            // Enviamos idAccion para que el JS pueda hacer el seguimiento
            return Ok(new
            {
                ok = true,
                data = resultado.msg,
                idAccion = resultado.idAccion
            });
        }
        /* cambio fin */
        public class ReprogramarCitaRequest
        {
            public int IdCitaVieja { get; set; }
            public string NuevaFecha { get; set; }
            public string NuevaHora { get; set; }
            public string Motivo { get; set; }
        }
        /* cambio fin */
        private static string NormHora(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();

            // "HH:mm:ss" o TimeSpan
            if (TimeSpan.TryParse(s, out var ts))
                return ts.ToString(@"hh\:mm");

            // DateTime
            if (DateTime.TryParse(s, out var dt))
                return dt.ToString("HH:mm");

            // fallback "HH:mm..."
            if (s.Length >= 5) return s.Substring(0, 5);

            return s;
        }

        public class AdminActualizarMotivoTextoRequest
        {
            public int IdCita { get; set; }
            public string? RazonCitaUsr { get; set; }
        }

        public class AdminActualizarDocumentoRequest
        {
            public int IdCita { get; set; }
            public string? DocumentoBase64 { get; set; }
            public string? ContentType { get; set; }
        }

        public class DoctorActualizarDocumentoRequest
        {
            public int IdCita { get; set; }
            public string? DocumentoBase64 { get; set; }
            public string? ContentTypeDoctor { get; set; }
        }
        private async Task<int> ObtenerIdEspecialidadPorIdDoctor(int idDoctor)
        {
            if (idDoctor <= 0) return 0;

            var doctores = await _repositorioDoctor.Lista();
            var doc = doctores.FirstOrDefault(d => d.IdDoctor == idDoctor);

            return doc?.Especialidad?.IdEspecialidad ?? 0;
        }
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> AgendaDia(string fecha, int idDoctor = 0, int idEspecialidad = 0)
        {
            if (string.IsNullOrWhiteSpace(fecha))
                return BadRequest(new { ok = false, msg = "Debe indicar fecha (dd/MM/yyyy)." });

            if (!TryParseFechaFlexible(fecha, out DateTime fechaSel))
                return BadRequest(new { ok = false, msg = "Fecha inválida. Use dd/MM/yyyy." });

            fechaSel = fechaSel.Date;

            int idDoctorFinal = idDoctor;

            // --- Doctor logueado: fuerza el doctor por claim NumeroDocumentoIdentidad ---
            if (User.IsInRole("Doctor"))
            {
                string? numDoc = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == "NumeroDocumentoIdentidad")?.Value;

                if (!string.IsNullOrWhiteSpace(numDoc))
                {
                    var doctores = await _repositorioDoctor.Lista();
                    var doc = doctores.FirstOrDefault(d =>
                        (d.NumeroDocumentoIdentidad ?? "").Trim()
                            .Equals(numDoc.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (doc != null) idDoctorFinal = doc.IdDoctor;
                }
            }
            else
            {
                // --- Admin: si no viene idDoctor, puede venir idEspecialidad ---
                if (idDoctorFinal <= 0 && idEspecialidad > 0)
                {
                    var doctores = await _repositorioDoctor.Lista();
                    var doc = doctores.FirstOrDefault(d =>
                        d.Especialidad != null && d.Especialidad.IdEspecialidad == idEspecialidad);

                    if (doc != null) idDoctorFinal = doc.IdDoctor;
                }
            }

            if (idDoctorFinal <= 0)
                return BadRequest(new { ok = false, msg = "Debe indicar un doctor válido." });

            // 1) Horarios del doctor (día)
            // IMPORTANTE: este método debe existir en tu repositorio (ya lo estabas usando)
            List<FechaAtencionDTO> listaFechas =
                await _repositorioDoctor.ListaDoctorHorarioDetalleConCitas(idDoctorFinal);

            FechaAtencionDTO? fechaDTO = null;
            foreach (var f in listaFechas)
            {
                if (f == null) continue;
                if (TryParseFechaFlexible(f.Fecha, out var fdt) && fdt.Date == fechaSel)
                {
                    fechaDTO = f;
                    break;
                }
            }

            var horariosDia = fechaDTO?.HorarioDTO ?? new List<HorarioDTO>();

            // 2) Citas del día para ese doctor (filtramos sobre el listado global)
            var todas = await _repositorioCita.ListaCitasGestion(0);

            bool EsMismaFecha(string? s)
                => !string.IsNullOrWhiteSpace(s) && TryParseFechaFlexible(s, out var d) && d.Date == fechaSel;

            var citasDiaDoctor = todas
                .Where(c => c != null
                            && c.Doctor != null
                            && c.Doctor.IdDoctor == idDoctorFinal
                            && EsMismaFecha(c.FechaCita))
                .ToList();

            // Map por hora "HH:mm" -> cita
            var mapHora = new Dictionary<string, Cita>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in citasDiaDoctor)
            {
                var key = NormHora(c.HoraCita);
                if (!string.IsNullOrWhiteSpace(key) && !mapHora.ContainsKey(key))
                    mapHora[key] = c;
            }

            var manana = new List<object>();
            var tarde = new List<object>();

            foreach (var h in horariosDia)
            {
                var hora = NormHora(h.TurnoHora);

                mapHora.TryGetValue(hora, out var cita);

                string paciente =
                    cita?.Usuario != null
                        ? $"{cita.Usuario.Nombre} {cita.Usuario.Apellido}".Trim()
                        : "";

                string estado = cita?.EstadoCita?.Nombre ?? "";

                object? citaCompacta = null;
                if (cita != null)
                {
                    citaCompacta = new
                    {
                        idCita = cita.IdCita,
                        fecha = cita.FechaCita,
                        hora = NormHora(cita.HoraCita),
                        paciente = paciente,
                        estado = estado,
                        origenCita = cita.OrigenCita ?? "",
                        razonCitaUsr = cita.RazonCitaUsr ?? "",
                        indicaciones = cita.Indicaciones ?? ""
                    };
                }

                var slot = new
                {
                    idDoctorHorarioDetalle = h.IdDoctorHorarioDetalle,
                    turno = h.Turno,
                    // INYECTAMOS EL NUEVO CAMPO QUE VIENE DEL DTO
                   // idDoctorHorarioDetalleCalcom = h.IdDoctorHorarioDetalleCalcom,
                    // BLINDAJE: Si h es null o la propiedad falla, enviamos 0
                    idDoctorHorarioDetalleCalcom = h?.IdDoctorHorarioDetalleCalcom ?? 0,

                    // NOMBRE ORIGINAL
                    turnoHora = hora,
                    // ALIAS PARA FRONT (muchas vistas usan "hora")
                    hora = hora,

                    tieneCita = (cita != null),
                    idCita = cita?.IdCita ?? 0,
                    paciente = paciente,
                    estado = estado,
                    cita = citaCompacta
                };

                if (string.Equals(h.Turno, "AM", StringComparison.OrdinalIgnoreCase))
                    manana.Add(slot);
                else
                    tarde.Add(slot);
            }

            return Ok(new
            {
                ok = true,
                msg = "",
                fecha = fechaSel.ToString("dd/MM/yyyy"),
                idDoctor = idDoctorFinal,
                am = manana,
                pm = tarde
            });
        }


        // ===================== AGENDA: ELIMINAR SLOT =====================
        [HttpDelete]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> EliminarSlot(int idDoctorHorarioDetalle)
        {
            if (idDoctorHorarioDetalle <= 0)
                return BadRequest(new { ok = false, msg = "IdDoctorHorarioDetalle inválido." });

            // Repositorio doctor: borrado seguro
            string resp = await _repositorioDoctor.EliminarSlot(idDoctorHorarioDetalle);

            if (!string.IsNullOrEmpty(resp))
                return BadRequest(new { ok = false, msg = resp });

            return Ok(new { ok = true, msg = "" });
        }
        // ===================== AGENDA: AGREGAR SLOT =====================
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> AgregarSlot(string fecha, int idDoctor, string hora)
        {
            if (idDoctor <= 0)
                return BadRequest(new { ok = false, msg = "IdDoctor inválido." });

            if (string.IsNullOrWhiteSpace(fecha))
                return BadRequest(new { ok = false, msg = "Fecha requerida." });

            if (!DateTime.TryParseExact(fecha.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var fechaDt))
                return BadRequest(new { ok = false, msg = "Fecha inválida. Formato dd/MM/yyyy." });

            if (string.IsNullOrWhiteSpace(hora))
                return BadRequest(new { ok = false, msg = "Hora requerida (HH:MM)." });

            // ✅ CAMBIO MÍNIMO: parsear a TimeOnly (Postgres time -> TimeOnly con Npgsql moderno)
            if (!TimeOnly.TryParseExact(hora.Trim(), "HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var t))
                return BadRequest(new { ok = false, msg = "Hora inválida. Formato HH:MM." });

            // Normaliza segundos a 00
            var turnoHora = new TimeOnly(t.Hour, t.Minute, 0);

            string resp = await _repositorioDoctor.AgregarSlot(idDoctor, fechaDt.Date, turnoHora);

            if (!string.IsNullOrEmpty(resp))
                return BadRequest(new { ok = false, msg = resp });

            return Ok(new { ok = true, msg = "" });
        }
        // ==========================
        // DESCARGA DE DOCUMENTOS (Paciente/Doctor)
        // ==========================
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> DescargarDocumentoPaciente(int idCita)
        {
            try
            {
                if (idCita <= 0) return NotFound();

                // Reutiliza el mismo origen de datos que Gestión de Citas (incluye base64 + content-type)
                var lista = await _repositorioCita.ListaCitasGestion(0);
                var cita = lista.FirstOrDefault(x => x.IdCita == idCita);
                if (cita == null) return NotFound();
                var bytes = cita.DocumentoCitaUsr;
              
                if (bytes == null || bytes.Length == 0) return NotFound();

                var contentType = string.IsNullOrWhiteSpace(cita.ContentType) ? "application/octet-stream" : cita.ContentType;
                var fileName = $"documento_paciente_cita_{idCita}{ContentTypeToExtension(contentType)}";

                return File(bytes, contentType, fileName);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> ObtenerDetalleCita(int idCita)
        {
            var dto = await _repositorioCita.ObtenerDetalleCita(idCita);

            if (dto == null)
                return Json(new { ok = false, msg = "No encontrada" });

            return Json(new { ok = true, data = dto });
        }


        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> DescargarDocumentoDoctor(int idCita)
        {
            try
            {
                if (idCita <= 0) return NotFound();

                var lista = await _repositorioCita.ListaCitasGestion(0);
                var cita = lista.FirstOrDefault(x => x.IdCita == idCita);
                if (cita == null) return NotFound();

               
                var bytes = cita.DocIndicacionesDoctor;
                if (bytes == null || bytes.Length == 0) return NotFound();

                var contentType = string.IsNullOrWhiteSpace(cita.ContentTypeDoctor) ? "application/octet-stream" : cita.ContentTypeDoctor;
                var fileName = $"documento_doctor_cita_{idCita}{ContentTypeToExtension(contentType)}";

                return File(bytes, contentType, fileName);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static byte[]? TryBase64ToBytes(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return null;

            var s = base64.Trim();

            // por si viene como "data:application/pdf;base64,AAAA..."
            var comma = s.IndexOf(',');
            if (comma >= 0 && s.Substring(0, comma).Contains("base64", StringComparison.OrdinalIgnoreCase))
                s = s[(comma + 1)..];

            try
            {
                return Convert.FromBase64String(s);
            }
            catch
            {
                return null;
            }
        }

        private static string ContentTypeToExtension(string contentType)
        {
            contentType = (contentType ?? "").ToLowerInvariant();

            if (contentType.Contains("pdf")) return ".pdf";
            if (contentType.Contains("png")) return ".png";
            if (contentType.Contains("jpeg") || contentType.Contains("jpg")) return ".jpg";
            if (contentType.Contains("msword")) return ".doc";
            if (contentType.Contains("officedocument.wordprocessingml")) return ".docx";

            return ".bin";
        }

        private static byte[]? ToBytes(object? value)
        {
            if (value == null) return null;

            // Caso 1: ya es byte[]
            if (value is byte[] b) return b;

            // Caso 2: viene como string base64
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                s = s.Trim();

                // Por si viniera tipo "data:application/pdf;base64,...."
                var idx = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) s = s.Substring(idx + "base64,".Length);

                try { return Convert.FromBase64String(s); }
                catch { return null; }
            }

            return null;
        }
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador,Paciente")]
        public async Task<IActionResult> GetSlotsLibres(int idCitaVieja, string fecha)
        {
            // 1. Averiguar qué doctor tiene esa cita original
            // (Puedes obtenerlo de tu lógica de BD actual)
            int idDoctor = await _repositorioCita.ObtenerIdDoctorDeCita(idCitaVieja);
            var slots = await _repositorioCita.ObtenerSlotsLibres(idDoctor, fecha);
            return Json(slots);
        }
        [HttpGet]
       
        [Authorize(Roles = "Doctor,Administrador,Paciente")]
        public async Task<IActionResult> GetCalPublicUrl(int idDoctor)
        {
            if (idDoctor <= 0)
                return Json(new { ok = false });

            // OJO: pon aquí tu connection string real (appsettings)
            var cs = Conexion.CN;

            const string sql = @"
SELECT cal_public_url
FROM public.doctoresapikeycalcom
WHERE iddoctor = @iddoctor
  AND is_active = true
LIMIT 1;
";

            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@iddoctor", idDoctor);

            var url = (await cmd.ExecuteScalarAsync()) as string;

            url = (url ?? "").Trim();
            if (string.IsNullOrWhiteSpace(url))
                return Json(new { ok = false });

            return Json(new { ok = true, url });
        }
        /* comienzo cambio */
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]

        public async Task<JsonResult> CancelarCitaPersonal(int idCita)
        {
            try
            {
                // 1. Cambia estado a 3 y lanza el SP
                string resultado = await _repositorioCita.CambiarEstadoCitaBD(idCita, 3);

                if (!string.IsNullOrEmpty(resultado))
                    return Json(new { ok = false, mensaje = resultado });

                // 2. Busca el ID en la tabla que acabamos de corregir
                int idAccion = await _repositorioCita.ObtenerUltimaAccionCancelacion(idCita);

                return Json(new { ok = true, idAccion = idAccion });
            }
            catch (Exception ex)
            {
                // Esto evitará el error 500 genérico y te mostrará el error real en la web
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> CancelarCitaPersonal([FromBody] CancelarCitaRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { ok = false, data = "Datos inválidos." });

            string docEjecutor = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "NumeroDocumentoIdentidad")?.Value ?? "";

            var resultado = await _repositorioCita.EncolarCancelacion(
                modelo.IdCita,
                modelo.Motivo,
                docEjecutor
            );

            if (!resultado.ok)
            {
                return Ok(new { ok = false, data = resultado.msg });
            }

            return Ok(new
            {
                ok = true,
                data = resultado.msg,
                idAccion = resultado.idAccion
            });
        }
        /* comienzo cambio sincro */
        [HttpPost]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> LanzarChequeoCalcom([FromBody] ChkCalcomRequest modelo)
        {
            if (modelo == null || modelo.IdCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { ok = false, data = "Datos inválidos." });

            // Encolamos la acción de chequeo
            var resultado = await _repositorioCita.EncolarChequeoCita(modelo.IdCita, modelo.Refresh);

            if (!resultado.ok)
            {
                return Ok(new { ok = false, data = "No se pudo encolar el chequeo." });
            }

            return Ok(new
            {
                ok = true,
                data = "Chequeo encolado correctamente",
                idAccion = resultado.idAccion
            });
        }

        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> VerResultadoSincro(int idCita)
        {
            if (idCita <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, new { ok = false, data = "ID de cita inválido." });

            var resultado = await _repositorioCita.ObtenerResultadoSincroDetalle(idCita);

            if (!resultado.ok)
            {
                return Ok(new { ok = false, data = "No hay datos de sincronización disponibles." });
            }

            return Ok(new
            {
                ok = true,
                data = resultado.data // Esto contiene resultsincro, fecha_sincro, etc.
            });
        }
        /* fin cambio sincro */
        public class ChkCalcomRequest
        {
            public int IdCita { get; set; }
            public string Refresh { get; set; } = "N";
        }
        /* fin cambio sincro */
        public class CancelarCitaRequest
        {
            public int IdCita { get; set; }
            public string Motivo { get; set; }
        }
        [HttpPost]
        [Authorize(Roles = "Paciente,Doctor,Administrador")]
        public async Task<JsonResult> CancelarCitaAgenda(int idCita, string motivo)
        {
            var resultado = await _repositorioCita.CancelarDesdeAgenda(idCita, motivo);
            return Json(new { ok = resultado.ok, idAccion = resultado.idAccion });
        }

    }
}
