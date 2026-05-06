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

        // 1. Lista las categorías principales (Operación 70)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarMenuPorRol(int idrol)
        {
            var todoElMenu = await _repositorio.Lista(idrol);
            // Formateamos para que n8n lea una lista simple de botones
            var categorias = todoElMenu.Select(m => new {
                text = $"{m.Icono} {m.Nombre}",
                callback_data = m.IdMenu.ToString()
            }).ToList();

            return StatusCode(StatusCodes.Status200OK, new { data = categorias });
        }

        // 2. Lista las opciones de un menú específico (Operación 80)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarSubmenu(int idmenu, int idrol = 3)
        {
            var todoElMenu = await _repositorio.Lista(idrol);
            var menuSeleccionado = todoElMenu.FirstOrDefault(m => m.IdMenu == idmenu);

            if (menuSeleccionado == null) return NotFound();

            var submenus = menuSeleccionado.Submenus.Select(s => new {
                text = $"{s.Icono} {s.Nombre}",
                callback_data = s.Opcion // Aquí va 'alta_cita', 'citas_pendientes', etc.
            }).ToList();

            return StatusCode(StatusCodes.Status200OK, new { data = submenus });
        }
    }
}