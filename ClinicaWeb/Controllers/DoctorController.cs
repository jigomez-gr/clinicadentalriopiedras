using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Claims;

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
            List<Doctor> lista = await _repositorio.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] Doctor objeto)
        {
            string respuesta = await _repositorio.Guardar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] Doctor objeto)
        {
            string respuesta = await _repositorio.Editar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            int respuesta = await _repositorio.Eliminar(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

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

    }
}
