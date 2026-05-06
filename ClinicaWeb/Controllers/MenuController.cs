using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization; // 👈 Fundamental para usar JsonPropertyName

namespace SistemaClinica.Controllers
{
    // 🏷️ Esta clase actúa como un "molde" que protege el nombre del campo
    public class RespuestaWrapper
    {
        [JsonPropertyName("DATA")]
        public object DATA { get; set; }
    }

    public class MenuController : Controller
    {
        private readonly IMenuRepositorio _repositorio;

        public MenuController(IMenuRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarMenuPorRol(int idrol)
        {
            var todoElMenu = await _repositorio.Lista(idrol);

            var categorias = todoElMenu.Select(m => new {
                text = $"{m.Icono}{m.Nombre}",
                callback_data = m.IdMenu.ToString()
            }).ToList();

            // 🚀 Usamos el Wrapper en lugar del objeto anónimo 'new { ... }'
            var respuesta = new RespuestaWrapper { DATA = categorias };
            return StatusCode(StatusCodes.Status200OK, respuesta);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarSubmenu(int idmenu, int idrol = 3)
        {
            var todoElMenu = await _repositorio.Lista(idrol);
            var menuSeleccionado = todoElMenu.FirstOrDefault(m => m.IdMenu == idmenu);

            if (menuSeleccionado == null) return NotFound();

            var submenus = menuSeleccionado.Submenus.Select(s => new {
                text = $"{s.Icono}{s.Nombre}",
                callback_data = s.Opcion
            }).ToList();

            // 🚀 Aquí también aplicamos el Wrapper para mantener la consistencia
            var respuesta = new RespuestaWrapper { DATA = submenus };
            return StatusCode(StatusCodes.Status200OK, respuesta);
        }
    }
}