using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using System.Data;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;
using NpgsqlTypes;

namespace ClinicaData.Implementacion
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ConnectionStrings con;
        public UsuarioRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> Editar(Usuario objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_editarusuario(" +
                    "@IdUsuario, @NumeroDocumentoIdentidad, @Nombre, @Apellido, @Clave, @IdRolUsuario, @Correo, @Movil);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdUsuario", objeto.IdUsuario);
                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);

                // En Postgres la firma va Clave antes que Correo/Movil
                cmd.Parameters.AddWithValue("@Clave", objeto.Clave);
                cmd.Parameters.AddWithValue("@IdRolUsuario", objeto.RolUsuario.IdRolUsuario);

                cmd.Parameters.AddWithValue("@Correo",
                    string.IsNullOrWhiteSpace(objeto.Correo) ? (object)DBNull.Value : objeto.Correo);

                cmd.Parameters.AddWithValue("@Movil",
                    string.IsNullOrWhiteSpace(objeto.Movil) ? (object)DBNull.Value : objeto.Movil);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al editar usuario";
            }
        }

        public async Task<string> Editar2(Usuario objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_editarusuario(" +
                    "@IdUsuario, @NumeroDocumentoIdentidad, @Nombre, @Apellido, @Clave, @IdRolUsuario, @Correo, @Movil);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdUsuario", objeto.IdUsuario);
                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);

                // En Postgres la firma va Clave antes que Correo/Movil
                cmd.Parameters.AddWithValue("@Clave", objeto.Clave);
                cmd.Parameters.AddWithValue("@IdRolUsuario", objeto.RolUsuario.IdRolUsuario);

                cmd.Parameters.AddWithValue("@Correo",
                    string.IsNullOrWhiteSpace(objeto.Correo) ? (object)DBNull.Value : objeto.Correo);

                cmd.Parameters.AddWithValue("@Movil",
                    string.IsNullOrWhiteSpace(objeto.Movil) ? (object)DBNull.Value : objeto.Movil);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al editar usuario";
            }
        }

        public async Task<int> Eliminar(int Id)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_eliminarusuario(@IdUsuario);",
                    conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdUsuario", Id);

                await cmd.ExecuteNonQueryAsync();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<string> Guardar(Usuario objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardarusuario(" +
                    "@NumeroDocumentoIdentidad, @Nombre, @Apellido, @Clave, @IdRolUsuario, @Correo, @Movil);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);

                // En Postgres: Clave antes que Correo/Movil
                cmd.Parameters.AddWithValue("@Clave", objeto.Clave);
                cmd.Parameters.AddWithValue("@IdRolUsuario", objeto.RolUsuario.IdRolUsuario);

                cmd.Parameters.AddWithValue("@Correo",
                    string.IsNullOrWhiteSpace(objeto.Correo) ? (object)DBNull.Value : objeto.Correo);

                cmd.Parameters.AddWithValue("@Movil",
                    string.IsNullOrWhiteSpace(objeto.Movil) ? (object)DBNull.Value : objeto.Movil);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al guardar usuario";
            }
        }

        public async Task<string> Guardar2(Usuario objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardarusuario(" +
                    "@NumeroDocumentoIdentidad, @Nombre, @Apellido, @Clave, @IdRolUsuario, @Correo, @Movil);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombre", objeto.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", objeto.Apellido);

                // En Postgres: Clave antes que Correo/Movil
                cmd.Parameters.AddWithValue("@Clave", objeto.Clave);
                cmd.Parameters.AddWithValue("@IdRolUsuario", 3);

                cmd.Parameters.AddWithValue("@Correo",
                    string.IsNullOrWhiteSpace(objeto.Correo) ? (object)DBNull.Value : objeto.Correo);

                cmd.Parameters.AddWithValue("@Movil",
                    string.IsNullOrWhiteSpace(objeto.Movil) ? (object)DBNull.Value : objeto.Movil);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al guardar usuario";
            }
        }
        public async Task<List<Usuario>> Lista(int IdRolUsuario = 0)
        {
            var lista = new List<Usuario>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listausuario(@IdRolUsuario);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdRolUsuario", IdRolUsuario);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                    NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                    Nombre = dr["Nombre"].ToString()!,
                    Apellido = dr["Apellido"].ToString()!,
                    Correo = dr["Correo"].ToString()!,
                    Clave = dr["Clave"].ToString()!,
                    Movil = dr["Movil"].ToString()!,
                    RolUsuario = new RolUsuario
                    {
                        IdRolUsuario = Convert.ToInt32(dr["IdRolUsuario"]),
                        Nombre = dr["NombreRol"].ToString()!,
                    },
                    FechaCreacion = dr["FechaCreacion"].ToString()!
                });
            }

            return lista;
        }
        public async Task<List<Usuario>> Lista2(int IdRolUsuario = 3)
        {
            var lista = new List<Usuario>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listausuario(@IdRolUsuario);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdRolUsuario", IdRolUsuario);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                    NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                    Nombre = dr["Nombre"].ToString()!,
                    Apellido = dr["Apellido"].ToString()!,
                    Correo = dr["Correo"].ToString()!,
                    Clave = dr["Clave"].ToString()!,
                    Movil = dr["Movil"].ToString()!,
                    RolUsuario = new RolUsuario
                    {
                        IdRolUsuario = Convert.ToInt32(dr["IdRolUsuario"]),
                        Nombre = dr["NombreRol"].ToString()!,
                    },
                    FechaCreacion = dr["FechaCreacion"].ToString()!
                });
            }

            return lista;
        }

        public async Task<Usuario> Login(string DocumentoIdentidad, string Clave)
        {
            Usuario objeto = null!;

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_loginusuario(@DocumentoIdentidad, @Clave);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@DocumentoIdentidad", DocumentoIdentidad);
            cmd.Parameters.AddWithValue("@Clave", Clave);

            await using var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await dr.ReadAsync())
            {
                objeto = new Usuario
                {
                    IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                    NumeroDocumentoIdentidad = dr["NumeroDocumentoIdentidad"].ToString()!,
                    Nombre = dr["Nombre"].ToString()!,
                    Apellido = dr["Apellido"].ToString()!,
                    Correo = dr["Correo"].ToString()!,
                    Movil = dr["Movil"].ToString()!,
                    RolUsuario = new RolUsuario
                    {
                       
                        IdRolUsuario = dr["IdRolUsuario"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdRolUsuario"]),
                        Nombre = dr["NombreRol"].ToString()!
                    }
                };
            }

            return objeto;
        }

    }
}
