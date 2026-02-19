using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaWeb.Controllers
{
    public class DoctorHorarioController : Controller
    {
        private readonly IDoctorRepositorio _repositorio;
        public DoctorHorarioController(IDoctorRepositorio repositorio)
        {
            _repositorio = repositorio;
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<DoctorHorario> lista = await _repositorio.ListaDoctorHorario();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }


        public async Task<IActionResult> Guardar([FromBody] DoctorHorario objeto)
        {
            string respuesta = await _repositorio.RegistrarHorario(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int Id)
        {
            string respuesta = await _repositorio.EliminarHorario(Id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
        /* Comienzo Cambio */
        [HttpPost]
        public async Task<IActionResult> LanzarSincroCalcom(int idDoctor, string mes, string anio)
        {
            // Llamada al repositorio con la tupla
            var (ok, msg, idAccion) = await _repositorio.EncolarSincroCalcom(idDoctor, mes, anio);

            // Devolvemos el objeto que espera el JavaScript de la vista
            return Json(new { esExitoso = ok, mensaje = msg, id = idAccion });
        }
        /* Final Cambio */

    }
}
