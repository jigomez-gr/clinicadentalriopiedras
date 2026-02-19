using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClinicaData.Contrato;

namespace SistemaClinica.Controllers
{
    public class ZadarmaSmsRespuestaController : Controller
    {
        private readonly IZadarmaSmsRespuestaRepositorio _repo;

        public ZadarmaSmsRespuestaController(IZadarmaSmsRespuestaRepositorio repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var lista = await _repo.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> DescargarJson(int idRespuestaSms)
        {
            var (json, filename) = await _repo.ObtenerRawJson(idRespuestaSms);

            if (string.IsNullOrWhiteSpace(json))
                return NotFound();

            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json; charset=utf-8", filename);
        }
    }
}
