

using ClinicaData.Contrato;
using ClinicaEntidades;
using Npgsql;
using System.Data;
using ClinicaData.Configuracion;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
namespace ClinicaData.Implementacion
{
    public class RolUsuarioRepositorio : IRolUsuarioRepositorio
    {
        private readonly ConnectionStrings con;
        public RolUsuarioRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<List<RolUsuario>> Lista()
        {
            var lista = new List<RolUsuario>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listarolusuario();",
                conexion);

            cmd.CommandType = CommandType.Text;

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new RolUsuario
                {
                    IdRolUsuario = Convert.ToInt32(dr["idrolusuario"]),
                    Nombre = dr["nombre"]?.ToString() ?? "",
                    FechaCreacion = dr["fechacreacion"]?.ToString() ?? ""
                });
            }

            return lista;
        }

    }
}
