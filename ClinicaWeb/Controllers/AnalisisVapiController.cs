using ClinicaData.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClinicaWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AnalisisVapiController : Controller
    {
        private readonly IAnalisisVapiRepositorio _repositorio;

        public AnalisisVapiController(IAnalisisVapiRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var lista = await _repositorio.Lista();
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAudio(int idLlamada)
        {
            var (bytes, filename) = await _repositorio.ObtenerAudio(idLlamada);

            if (bytes == null || bytes.Length == 0)
                return NotFound();

            // Detectar WAV/MP3/OGG por cabecera y dar content-type correcto
            string contentType = "application/octet-stream";
            string ext = "bin";

            if (bytes.Length >= 4 &&
                bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46) { contentType = "audio/wav"; ext = "wav"; }
            else if (bytes.Length >= 4 &&
                bytes[0] == 0x4F && bytes[1] == 0x67 && bytes[2] == 0x67 && bytes[3] == 0x53) { contentType = "audio/ogg"; ext = "ogg"; }
            else if (bytes.Length >= 3 &&
                ((bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33) || (bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0))) { contentType = "audio/mpeg"; ext = "mp3"; }

            if (string.IsNullOrWhiteSpace(filename)) filename = $"audio_llamada_{idLlamada}.{ext}";
            if (!filename.Contains(".")) filename = $"{filename}.{ext}";

            return File(bytes, contentType, filename);
        }


    }
}
