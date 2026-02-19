using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using Npgsql;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
namespace ClinicaData.Implementacion
{
    public class EspecialidadRepositorio : IEspecialidadRepositorio
    {
        private readonly ConnectionStrings con;
        public EspecialidadRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

public async Task<string> Editar(Especialidad objeto)
    {
        try
        {
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT public.sp_editarespecialidad(@IdEspecialidad, @Nombre);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdEspecialidad", objeto.IdEspecialidad);
            cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "";
        }
        catch
        {
            return "Error al editar la especialidad";
        }
    }

        public async Task<int> Eliminar(int Id)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_eliminarespecialidad(@IdEspecialidad);",
                    conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdEspecialidad", Id);

                await cmd.ExecuteNonQueryAsync();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<string> Guardar(Especialidad objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardarespecialidad(@Nombre);",
                    conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al guardar la especialidad";
            }
        }

        public async Task<List<Especialidad>> Lista()
        {
            var lista = new List<Especialidad>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listaespecialidad();",
                conexion);

            cmd.CommandType = CommandType.Text;

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Especialidad
                {
                    IdEspecialidad = Convert.ToInt32(dr["idespecialidad"]),
                    Nombre = dr["nombre"]?.ToString() ?? "",
                    FechaCreacion = dr["fechacreacion"]?.ToString() ?? ""
                });
            }

            return lista;
        }

    }
}
