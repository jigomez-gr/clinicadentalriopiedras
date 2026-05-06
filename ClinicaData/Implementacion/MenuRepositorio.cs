using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ClinicaData.Implementacion
{
    public class MenuRepositorio : IMenuRepositorio
    {
        private readonly ConnectionStrings con;

        public MenuRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }
        public async Task<List<TgMenu>> Lista(int IdRolUsuario)
        {
            var lista = new List<TgMenu>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Consulta única con LEFT JOIN para traer la jerarquía completa de una vez
            string query = @"
        SELECT m.idmenu, m.nombre as nombre_menu, m.icono as icono_menu, 
               s.idsubmenu, s.nombre as nombre_submenu, s.icono as icono_submenu, s.opcion
        FROM public.tg_menu m
        LEFT JOIN public.tg_submenu s ON m.idmenu = s.idmenu
        WHERE m.idrolusuario = @IdRolUsuario
        ORDER BY m.claveordenacion, s.claveordenacion;";

            await using var cmd = new NpgsqlCommand(query, conexion);
            cmd.Parameters.AddWithValue("@IdRolUsuario", IdRolUsuario);

            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                int idMenuActual = dr.GetInt32(dr.GetOrdinal("idmenu"));

                // Buscamos si ya hemos procesado la raíz de este menú
                var menu = lista.FirstOrDefault(m => m.IdMenu == idMenuActual);

                if (menu == null)
                {
                    menu = new TgMenu
                    {
                        IdMenu = idMenuActual,
                        // Usamos Trim() y Coalesce para evitar textos vacíos o con espacios fantasmas
                        Nombre = dr["nombre_menu"]?.ToString()?.Trim() ?? string.Empty,
                        Icono = dr["icono_menu"]?.ToString()?.Trim() ?? string.Empty,
                        Submenus = new List<TgSubmenu>()
                    };
                    lista.Add(menu);
                }

                // Si la fila actual contiene un submenú, lo añadimos a su padre
                if (dr["idsubmenu"] != DBNull.Value)
                {
                    menu.Submenus.Add(new TgSubmenu
                    {
                        IdSubmenu = Convert.ToInt32(dr["idsubmenu"]),
                        IdMenu = idMenuActual,
                        Nombre = dr["nombre_submenu"]?.ToString()?.Trim() ?? string.Empty,
                        Icono = dr["icono_submenu"]?.ToString()?.Trim() ?? string.Empty,
                        Opcion = dr["opcion"]?.ToString()?.Trim() ?? string.Empty
                    });
                }
            }
            return lista;
        }
    }
}