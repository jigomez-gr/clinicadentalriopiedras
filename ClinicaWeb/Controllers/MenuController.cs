using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaClinica.Controllers
{
    // No usamos [Authorize] aquí para que el bot/n8n pueda consultar sin problemas de sesión
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

            // Formateamos los botones: icono y nombre pegados 👤
            var categorias = todoElMenu.Select(m => new {
                text = $"{m.Icono}{m.Nombre}", // Sin espacio intermedio
                callback_data = m.IdMenu.ToString()
            }).ToList();

            // Devolvemos DATA en mayúsculas para n8n ⬆️
            return StatusCode(StatusCodes.Status200OK, new { DATA = categorias });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarSubmenu(int idmenu, int idrol = 3)
        {
            var todoElMenu = await _repositorio.Lista(idrol);
            var menuSeleccionado = todoElMenu.FirstOrDefault(m => m.IdMenu == idmenu);

            if (menuSeleccionado == null) return NotFound();

            var submenus = menuSeleccionado.Submenus.Select(s => new {
                text = $"{s.Icono}{s.Nombre}", // Icono y nombre pegados
                callback_data = s.Opcion
            }).ToList();

            return StatusCode(StatusCodes.Status200OK, new { DATA = submenus });
        }
    }
}