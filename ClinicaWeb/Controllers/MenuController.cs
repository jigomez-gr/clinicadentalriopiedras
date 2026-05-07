using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace SistemaClinica.Controllers
{
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
                text = $"{m.Icono} {m.Nombre}".Trim(),
                callback_data = m.IdMenu.ToString()
            }).ToList();

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
                text = $"{s.Icono} {s.Nombre}".Trim(),
                callback_data = s.Opcion
            }).ToList();

            var respuesta = new RespuestaWrapper { DATA = submenus };
            return StatusCode(StatusCodes.Status200OK, respuesta);
        }
    }
}