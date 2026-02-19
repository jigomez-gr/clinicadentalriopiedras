using ClinicaEntidades;
using ClinicaData.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaClinica.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepositorio _repositorio;

        public UsuarioController(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        // Solo administrador puede entrar a la pantalla de usuarios
        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lista de usuarios.
        /// idRol = 0 -> todos los roles
        /// idRol = 1 -> solo Administrador
        /// idRol = 2 -> solo Doctor
        /// idRol = 3 -> solo Paciente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Lista(int idRol = 0)
        {
            List<Usuario> lista = await _repositorio.Lista(idRol);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] Usuario objeto)
        {
            // Validación mínima
            if (objeto == null ||
                string.IsNullOrWhiteSpace(objeto.NumeroDocumentoIdentidad) ||
                string.IsNullOrWhiteSpace(objeto.Nombre) ||
                string.IsNullOrWhiteSpace(objeto.Apellido) ||
                string.IsNullOrWhiteSpace(objeto.Correo) ||
                string.IsNullOrWhiteSpace(objeto.Clave) ||
                string.IsNullOrWhiteSpace(objeto.Movil) ||
                objeto.RolUsuario == null ||
                objeto.RolUsuario.IdRolUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Datos incompletos." });
            }

            string respuesta = await _repositorio.Guardar(objeto);

            // En este proyecto, si todo va bien devuelves cadena vacía
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] Usuario objeto)
        {
            if (objeto == null || objeto.IdUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Id de usuario no válido." });
            }

            if (objeto.RolUsuario == null || objeto.RolUsuario.IdRolUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Debe indicar el rol." });
            }

            string respuesta = await _repositorio.Editar(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            int respuesta = await _repositorio.Eliminar(id);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [Authorize(Roles = "Doctor")]
        public IActionResult Index2()
        {
            return View();
        }

        public async Task<IActionResult> Lista2(int idRol = 3)
        {
            List<Usuario> lista = await _repositorio.Lista2(idRol);
            return StatusCode(StatusCodes.Status200OK, new { data = lista });
        }

        [HttpPost]
        public async Task<IActionResult> Guardar2([FromBody] Usuario objeto)
        {
            // Validación mínima
            if (objeto == null ||
                string.IsNullOrWhiteSpace(objeto.NumeroDocumentoIdentidad) ||
                string.IsNullOrWhiteSpace(objeto.Nombre) ||
                string.IsNullOrWhiteSpace(objeto.Apellido) ||
                string.IsNullOrWhiteSpace(objeto.Correo) ||
                string.IsNullOrWhiteSpace(objeto.Clave) ||
                string.IsNullOrWhiteSpace(objeto.Movil) ||
                objeto.RolUsuario == null ||
                objeto.RolUsuario.IdRolUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Datos incompletos." });
            }

            string respuesta = await _repositorio.Guardar2(objeto);

            // En este proyecto, si todo va bien devuelves cadena vacía
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Editar2([FromBody] Usuario objeto)
        {
            if (objeto == null || objeto.IdUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Id de usuario no válido." });
            }

            if (objeto.RolUsuario == null || objeto.RolUsuario.IdRolUsuario == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { data = "Debe indicar el rol." });
            }

            string respuesta = await _repositorio.Editar2(objeto);
            return StatusCode(StatusCodes.Status200OK, new { data = respuesta });
        }
    }
}
