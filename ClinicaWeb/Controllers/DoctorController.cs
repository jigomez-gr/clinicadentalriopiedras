using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Claims;
using System.Text.Json;

namespace ClinicaWeb.Controllers
{

    public class DoctorController : Controller
    {
        // =========================================================
        // aqui empieza la modificacion
        // =========================================================
        // Necesitas que con NO sea null. En este proyecto suele venir por IOptions<ConnectionStrings>
        private readonly ConnectionStrings con;
        // =========================================================
        // aqui termina la modificacion
        // =========================================================

        private readonly IDoctorRepositorio _repositorio;
        private readonly ICitaRepositorio _repositorioCita;

        // =========================================================
        // aqui empieza la modificacion
        // =========================================================
        public DoctorController(
            IDoctorRepositorio repositorio,
            ICitaRepositorio repositorioCita,
            IOptions<ConnectionStrings> options // <-- inyectamos opciones
        )
        {
            _repositorio = repositorio;
            _repositorioCita = repositorioCita;

            // <-- aqui se rellena con.CadenaSQL desde appsettings (ConnectionStrings)
            con = options.Value;
        }


        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            // El repositorio ya devuelve los nuevos campos (Bio, Valoración, etc.)
            List<Doctor> lista = await _repositorio.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpPost]
        public async Task<IActionResult> Guardar([FromForm] Doctor objeto, IFormFile FotoFile)
        {
            try
            {
                // Si el usuario subió una foto, la convertimos a byte[] para el repositorio
                if (FotoFile != null && FotoFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await FotoFile.CopyToAsync(ms);
                        objeto.Archivo = ms.ToArray();
                    }
                }

                string respuesta = await _repositorio.Guardar(objeto);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = ex.Message });
            }
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpGet]
        public async Task<IActionResult> VerFoto(int id)
        {
            // Obtener la lista y buscar al doctor por su ID
            var listaDoctores = await _repositorio.Lista();
            var doctor = listaDoctores.FirstOrDefault(d => d.IdDoctor == id);

            // Si el doctor no existe o no tiene archivo (BLOB)
            if (doctor?.Archivo == null || doctor.Archivo.Length == 0)
            {
                return NotFound(); // O puedes devolver una imagen por defecto desde wwwroot
            }

            // Retornamos el array de bytes como un archivo de imagen
            // El navegador detectará automáticamente si es PNG o JPG
            return File(doctor.Archivo, "image/jpeg");
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpGet]
        public async Task<IActionResult> DescargarFotoDirecta(int id)
        {
            // Recupera la lista completa o un objeto específico por ID
            var lista = await _repositorio.Lista();
            var doctor = lista.FirstOrDefault(d => d.IdDoctor == id);

            // Verificación crítica: el campo 'Archivo' debe contener el byte[] de la DB
            if (doctor?.Archivo == null || doctor.Archivo.Length == 0)
            {
                return Content("El doctor no tiene ninguna foto guardada.");
            }

            // Se utiliza 'application/octet-stream' para forzar la descarga del BLOB
            return File(doctor.Archivo, "application/octet-stream", $"doctor_{id}_foto.jpg");
        }

        public async Task<IActionResult> Editar([FromForm] Doctor objeto, IFormFile FotoFile)
        {
            try
            {
                // 1. Si se adjunta una foto nueva, la procesamos
                if (FotoFile != null && FotoFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await FotoFile.CopyToAsync(ms);
                        objeto.Archivo = ms.ToArray();
                    }
                }
                else
                {
                    // 2. CRÍTICO: Si NO hay foto nueva, recuperamos la actual de la DB 
                    // para que 'objeto.Archivo' no sea null y no borre la foto en el UPDATE.
                    var listaCompleta = await _repositorio.Lista();
                    var doctorExistente = listaCompleta.FirstOrDefault(d => d.IdDoctor == objeto.IdDoctor);
                    if (doctorExistente != null)
                    {
                        objeto.Archivo = doctorExistente.Archivo;
                    }
                }

                string respuesta = await _repositorio.Editar(objeto);
                return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = ex.Message });
            }
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        // NUEVO: Método para listar las especialidades de la tabla intermedia
        [HttpGet]
        public async Task<IActionResult> ListarEspecialidadesDoctor(int idDoctor)
        {
            // Este método lo llamará el nuevo DataTable dentro del modal de especialidades
            var lista = await _repositorio.ListarEspecialidadesPorDoctor(idDoctor);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpPost]
        public async Task<IActionResult> AsignarEspecialidad(int idDoctor, int idEspecialidad)
        {
            string respuesta = await _repositorio.AsignarEspecialidad(idDoctor, idEspecialidad);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpDelete]
        public async Task<IActionResult> EliminarEspecialidad(int id)
        {
            bool respuesta = await _repositorio.EliminarEspecialidadDoctor(id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            int respuesta = await _repositorio.Eliminar(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        [Authorize(Roles = "Administrador,Paciente,Doctor")]
        [HttpGet]
        public async Task<IActionResult> ListaCitasAsignadas(int IdEstadoCita)
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault()!;

            List<Cita> lista = await _repositorio.ListaCitasAsignadas(int.Parse(idUsuario),IdEstadoCita);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }
        [HttpPost]
        public async Task<JsonResult> ListaCitasAsignadasServerSide(int start, int length, string search, int idEstadoCita)
        {
            try
            {
                // IdUsuario (claim) -> IdDoctor (por NumeroDocumentoIdentidad)
                var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out int idUsuario))
                {
                    return Json(new { error = true, message = "No se pudo obtener el IdUsuario del claim." });
                }

                int idDoctor = 0;

                await using var cn = new NpgsqlConnection(con.CadenaSQL);
                await cn.OpenAsync();

                await using var cmd = new NpgsqlCommand(@"
    SELECT d.iddoctor
    FROM public.usuario u
    JOIN public.doctor d ON d.numerodocumentoidentidad = u.numerodocumentoidentidad
    WHERE u.idusuario = @IdUsuario
    ORDER BY d.iddoctor
    LIMIT 1;
", cn);

                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                var obj = await cmd.ExecuteScalarAsync();
                idDoctor = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);



                if (idDoctor == 0)
                {
                    string drawEmpty = Request.HasFormContentType ? (Request.Form["draw"].ToString() ?? "1") : "1";
                    return Json(new { draw = drawEmpty, recordsTotal = 0, recordsFiltered = 0, data = new List<Cita>() });
                }

                // search[value]
                if (Request.HasFormContentType)
                {
                    var formSearch = Request.Form["search[value]"];
                    if (!string.IsNullOrEmpty(formSearch))
                        search = formSearch;
                }

                // draw
                string draw = "1";
                if (Request.HasFormContentType)
                {
                    var formDraw = Request.Form["draw"];
                    if (!string.IsNullOrEmpty(formDraw))
                        draw = formDraw;
                }

                // idEstadoCita (desde form)
                if (Request.HasFormContentType)
                {
                    var formEstado = Request.Form["idEstadoCita"];
                    if (!string.IsNullOrEmpty(formEstado) && int.TryParse(formEstado, out var tmp))
                        idEstadoCita = tmp;
                }

                var (lista, total) = await _repositorio.ListaCitasAsignadasServerSide(
                    idDoctor,
                    idEstadoCita,
                    start,
                    length,
                    search ?? ""
                );

                return Json(new
                {
                    draw = draw,
                    recordsTotal = total,
                    recordsFiltered = total,
                    data = lista
                });
            }
            catch (Exception ex)
            {
                ErrorHandler.RegistrarError(
                    Conexion.CN,
                    "ListaCitasAsignadasServerSide",
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


        [HttpPost]
        public async Task<IActionResult> CambiarEstado([FromBody] Cita objeto)
        {
            string respuesta = await _repositorioCita.CambiarEstado(objeto.IdCita,objeto.EstadoCita.IdEstadoCita,objeto.Indicaciones);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        [HttpPost]
        public async Task<IActionResult> GuardarDocumentoDoctor([FromBody] Cita objeto)
        {
            string respuesta = await _repositorioCita.ActualizarDocIndicacionesDoctor(
                objeto.IdCita,
                objeto.DocIndicacionesDoctor,
                objeto.ContentTypeDoctor
            );

            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        [HttpGet]
        [Authorize(Roles = "Doctor,Administrador")]
        public async Task<IActionResult> GetCalPublicUrl(int idDoctor)
        {
            try
            {
                if (idDoctor <= 0)
                    return BadRequest(new { ok = false, msg = "IdDoctor inválido." });

                await using var cn = new NpgsqlConnection(con.CadenaSQL);
                await cn.OpenAsync();

                await using var cmd = new NpgsqlCommand(@"
                SELECT cal_public_url
                FROM public.doctoresapikeycalcom
                WHERE iddoctor = @idDoctor
                  AND is_active = true
                LIMIT 1;
            ", cn);

                cmd.Parameters.AddWithValue("@idDoctor", idDoctor);

                var obj = await cmd.ExecuteScalarAsync();
                var url = (obj == null || obj == DBNull.Value) ? null : obj.ToString();

                url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();

                return Ok(new { ok = true, cal_public_url = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, msg = ex.Message });
            }
        }

        [Authorize(Roles = "Doctor,Administrador")]
        [HttpGet]
        public async Task<IActionResult> ObtenerCalcom(int idDoctor)
        {
            var cfg = await _repositorio.ObtenerCalcom(idDoctor);
            return Ok(new { data = cfg });
        }

        [Authorize(Roles = "Doctor,Administrador")]
        [HttpPost]
        public async Task<IActionResult> GuardarCalcom([FromBody] DoctorApiKeyCalcom cfg)
        {
            if (cfg == null || cfg.IdDoctor <= 0) return BadRequest(new { data = "IdDoctor inválido" });
            if (string.IsNullOrWhiteSpace(cfg.ApiKey)) return BadRequest(new { data = "ApiKey requerida" });

            var resp = await _repositorio.GuardarCalcom(cfg);
            return Ok(new { data = resp });
        }
        [Authorize(Roles = "Doctor,Administrador")]
        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleCalcom(int idSlot)
        {
            var detalle = await _repositorioCita.ObtenerDetalleCalcom(idSlot);
            if (detalle == null) return Json(new { ok = false, msg = "No hay datos de Cal.com." });
            return Json(new { ok = true, data = detalle });
        }
        [Authorize(Roles = "Doctor,Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetDetallesApiCal(string apiKey, string apiBase)
        {
            try
            {
                string url = (string.IsNullOrEmpty(apiBase) ? "https://api.cal.eu/v2" : apiBase).TrimEnd('/');
                if (!url.EndsWith("/v2")) url += "/v2";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var res = await client.GetAsync($"{url}/event-types");

                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    return BadRequest(new { msg = "Cal.com respondió con error: " + res.StatusCode });
                }

                var json = await res.Content.ReadFromJsonAsync<JsonElement>();

                // EXTRAEMOS EL ARRAY REAL DE EVENTOS
                // Cal.com v2 devuelve: { data: { eventTypeGroups: [ { eventTypes: [...] } ] } }
                JsonElement eventTypes;
                try
                {
                    eventTypes = json.GetProperty("data").GetProperty("eventTypeGroups")[0].GetProperty("eventTypes");
                }
                catch
                {
                    // Fallback por si la estructura cambia o viene simplificada
                    eventTypes = json.GetProperty("data");
                }

                return Ok(new { data = eventTypes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { msg = "Error interno: " + ex.Message });
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarPublicoTelegram(string chat_id)
        {
            if (string.IsNullOrEmpty(chat_id))
                return Ok(new { error = true, MENSAJE = "Chat ID no proporcionado." });

            try
            {
                var resultado = await _repositorioCita.ObtenerCitasPendientesTelegram(chat_id);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Ok(new { error = true, MENSAJE = ex.Message });
            }
        }
        // desde aqui los controladores para prototipo de telegram ..
        // Endpoint para el Bot de Telegram

        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> ListarDoctoresPublico(int especialidad)
        {
            try
            {
                var lista = await _repositorio.ListarDoctoresPorEspecialidad(especialidad);
                return StatusCode(StatusCodes.Status200OK, new { data = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        // Añade estos dos métodos dentro de DoctorController.cs
        // No hay que tocar nada más: ni repositorio, ni BD, ni clases.

        // ── Añadir estos dos métodos en DoctorController.cs ──────────────────────────
        // No tocar nada más del controlador.

        /// <summary>
        /// Obtiene los bookingFields del event-type en Cal.com
        /// GET /Doctor/GetBookingFields?apiKey=...&eventTypeId=...
        /// </summary>
        [Authorize(Roles = "Doctor,Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetBookingFields(string apiKey, int eventTypeId, string? apiBase)
        {
            try
            {
                string url = (string.IsNullOrEmpty(apiBase) ? "https://api.cal.eu/v2" : apiBase).TrimEnd('/');

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("cal-api-version", "2024-06-14");

                var res = await client.GetAsync($"{url}/event-types/{eventTypeId}");
                if (!res.IsSuccessStatusCode)
                    return BadRequest(new { ok = false, msg = $"Cal.com respondió: {res.StatusCode}" });

                var json = await res.Content.ReadFromJsonAsync<JsonElement>();

                JsonElement bookingFields = default;
                try { bookingFields = json.GetProperty("data").GetProperty("bookingFields"); }
                catch { return Ok(new { ok = true, data = Array.Empty<object>() }); }

                return Ok(new { ok = true, data = bookingFields });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, msg = ex.Message });
            }
        }

        /// <summary>
        /// Crea en Cal.com los bookingFields custom que falten en el event-type
        /// POST /Doctor/CrearBookingFieldsFaltantes
        /// Body: { apiKey, apiBase?, eventTypeId, camposFaltantes: ["apellido","razoncitausr","telefonoMovil"] }
        /// </summary>
        [Authorize(Roles = "Doctor,Administrador")]
        [HttpPost]
        public async Task<IActionResult> CrearBookingFieldsFaltantes([FromBody] JsonElement body)
        {
            try
            {
                string apiKey = body.GetProperty("apiKey").GetString() ?? "";
                string apiBase = body.TryGetProperty("apiBase", out var ab)
                                      ? ab.GetString() ?? "https://api.cal.eu/v2"
                                      : "https://api.cal.eu/v2";
                int eventTypeId = body.GetProperty("eventTypeId").GetInt32();

                var camposFaltantes = new List<string>();
                if (body.TryGetProperty("camposFaltantes", out var cf))
                    foreach (var item in cf.EnumerateArray())
                        camposFaltantes.Add(item.GetString() ?? "");

                if (!camposFaltantes.Any())
                    return Ok(new { ok = true, msg = "No hay campos que crear." });

                // Solo campos CUSTOM (Cal.com exige "slug"; los nativos name/email los gestiona él)
                var definiciones = new Dictionary<string, object>
                {
                    ["apellido"] = new
                    {
                        slug = "apellido",
                        type = "text",
                        label = "Apellido",
                        required = true,
                        hidden = false
                    },
                    ["razoncitausr"] = new
                    {
                        slug = "razoncitausr",
                        type = "textarea",
                        label = "Motivo de la cita",
                        required = false,
                        hidden = false
                    },
                    ["telefonoMovil"] = new
                    {
                        slug = "telefonoMovil",
                        type = "phone",
                        label = "Telefono movil",
                        required = false,
                        hidden = false
                    }
                };

                var nuevosFields = camposFaltantes
                    .Where(c => definiciones.ContainsKey(c))
                    .Select(c => definiciones[c])
                    .ToList();

                if (!nuevosFields.Any())
                    return Ok(new { ok = false, msg = "Campos desconocidos." });

                string url = apiBase.TrimEnd('/');

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("cal-api-version", "2024-06-14");

                var patch = new { bookingFields = nuevosFields };
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(patch),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var res = await client.PatchAsync($"{url}/event-types/{eventTypeId}", content);
                var resBody = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                    return BadRequest(new
                    {
                        ok = false,
                        msg = $"Cal.com respondió: {res.StatusCode}",
                        detalle = resBody
                    });

                return Ok(new { ok = true, msg = $"Campos creados: {string.Join(", ", camposFaltantes)}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, msg = ex.Message });
            }
        }
    }
}
