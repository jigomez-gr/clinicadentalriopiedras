using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using ClinicaEntidades.DTO;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace ClinicaData.Implementacion
{
    public class CitaRepositorio : ICitaRepositorio
    {
        private readonly ConnectionStrings con;

        public CitaRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }
    
        // ============================================================
        // Actualizar datos del PACIENTE (motivo + doc) vía SP genérico
        // ============================================================
     public async Task<string> ActualizarDatosPaciente(
        int IdCita,
        string razonCitaUsr,
        byte[]? documento,
        string? contentType)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // OJO: es una FUNCTION -> se llama con SELECT y devuelve un varchar
                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_actualizarcitapaciente(@IdCita, @RazonCitaUsr, @DocumentoCitaUsr, @ContentType);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", IdCita);

                cmd.Parameters.AddWithValue("@RazonCitaUsr",
                    string.IsNullOrWhiteSpace(razonCitaUsr) ? "" : razonCitaUsr);

                var pDoc = cmd.Parameters.Add("@DocumentoCitaUsr", NpgsqlDbType.Bytea);
                pDoc.Value = (documento != null && documento.Length > 0)
                    ? (object)documento
                    : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrWhiteSpace(contentType) ? (object)DBNull.Value : contentType);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch (Exception)
            {
                return "No se pudieron actualizar los datos del paciente.";
            }
        }

public async Task ActualizarCitaConfirmacionAdmin(int idCita, string? citaConfirmada)
    {
        await using var conexion = new NpgsqlConnection(con.CadenaSQL);
        await conexion.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT public.sp_actualizarcitaconfirmacionadmin(@IdCita, @CitaConfirmada);",
            conexion);

        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@IdCita", idCita);
        cmd.Parameters.AddWithValue("@CitaConfirmada",
            string.IsNullOrWhiteSpace(citaConfirmada) ? (object)DBNull.Value : citaConfirmada);

        var result = await cmd.ExecuteScalarAsync();
        var msg = result?.ToString() ?? "";

        // La función devuelve '' si OK. Si quieres ignorar errores, elimina este if.
        if (!string.IsNullOrWhiteSpace(msg))
            throw new Exception(msg);
    } 

        public async Task<string> CambiarEstado(int IdCita, int IdEstado, string Indicaciones)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_cambiarestadocita(@IdCita, @IdEstadoCita, @Indicaciones);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", IdCita);
                cmd.Parameters.AddWithValue("@IdEstadoCita", IdEstado);

                // En tu función, si viene NULL/'' mantiene el valor anterior:
                cmd.Parameters.AddWithValue("@Indicaciones",
                    string.IsNullOrWhiteSpace(Indicaciones) ? (object)DBNull.Value : Indicaciones);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al cambiar estado";
            }
        }

        public async Task<string> Cancelar(int Id)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_cancelarcita(@IdCita);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", Id);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al cancelar la cita";
            }
        }

        public async Task<string> GuardarDocumentoDoctor(int idCita, byte[]? documento, string? contentTypeDoctor)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardardocumentodoctor(@IdCita, @DocIndicacionesDoctor, @ContentType_Doctor);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", idCita);

                var pDoc = cmd.Parameters.Add("@DocIndicacionesDoctor", NpgsqlDbType.Bytea);
                pDoc.Value = (documento != null && documento.Length > 0)
                    ? (object)documento
                    : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType_Doctor",
                    string.IsNullOrWhiteSpace(contentTypeDoctor) ? (object)DBNull.Value : contentTypeDoctor);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "No se pudo guardar el documento del doctor";
            }
        }

        public async Task<string> Guardar(Cita objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardarcita(" +
                    "@IdUsuario, @IdDoctorHorarioDetalle, @IdEstadoCita, @FechaCita, " +
                    "@OrigenCita, @RazonCitaUsr, @DocumentoCitaUsr, @ContentType);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdUsuario", objeto.Usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@IdDoctorHorarioDetalle", objeto.DoctorHorarioDetalle.IdDoctorHorarioDetalle);
                cmd.Parameters.AddWithValue("@IdEstadoCita", objeto.EstadoCita.IdEstadoCita);

                // p_fechacita es character varying y la función hace to_date(..., 'DD/MM/YYYY')
                cmd.Parameters.AddWithValue("@FechaCita",
                    string.IsNullOrWhiteSpace(objeto.FechaCita) ? (object)DBNull.Value : objeto.FechaCita);

                cmd.Parameters.AddWithValue("@OrigenCita",
                    string.IsNullOrWhiteSpace(objeto.OrigenCita) ? "WEB" : objeto.OrigenCita);

                cmd.Parameters.AddWithValue("@RazonCitaUsr",
                    string.IsNullOrWhiteSpace(objeto.RazonCitaUsr) ? (object)DBNull.Value : objeto.RazonCitaUsr);

                var pDoc = cmd.Parameters.Add("@DocumentoCitaUsr", NpgsqlDbType.Bytea);
                pDoc.Value = (objeto.DocumentoCitaUsr != null && objeto.DocumentoCitaUsr.Length > 0)
                    ? (object)objeto.DocumentoCitaUsr
                    : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrWhiteSpace(objeto.ContentType) ? (object)DBNull.Value : objeto.ContentType);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al guardar la cita";
            }
        }


        public async Task<List<Cita>> ListaCitasPendiente(int IdUsuario)
        {
            var lista = new List<Cita>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listacitaspendiente(@IdUsuario);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var cita = new Cita
                {
                    IdCita = Convert.ToInt32(dr["IdCita"]),
                    FechaCita = dr["FechaCita"]?.ToString() ?? "",
                    FechaCitaOrden = dr["FechaCitaOrden"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaCitaOrden"]),
                    HoraCita = dr["HoraCita"]?.ToString() ?? "",

                    Especialidad = new Especialidad
                    {
                        Nombre = dr["NombreEspecialidad"]?.ToString() ?? "",
                    },
                    Doctor = new Doctor
                    {
                        Nombres = dr["Nombres"]?.ToString() ?? "",
                        Apellidos = dr["Apellidos"]?.ToString() ?? "",
                    },

                    // PACIENTE
                    OrigenCita = dr["OrigenCita"]?.ToString() ?? "",
                    RazonCitaUsr = dr["RazonCitaUsr"]?.ToString() ?? "",
                    DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],
                    ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"]?.ToString(),

                    // DOCTOR
                    Indicaciones = dr["Indicaciones"] == DBNull.Value ? "" : dr["Indicaciones"]?.ToString() ?? "",
                    DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                    ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"]?.ToString()
                };

                lista.Add(cita);
            }

            return lista;
        }


        public async Task<string> GuardarMotivoPaciente(Cita objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // FUNCTION -> SELECT y devuelve varchar con msg_error
                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_actualizarmotivopaciente(@IdCita, @RazonCitaUsr, @DocumentoCitaUsr, @ContentType);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", objeto.IdCita);

                // RazonCitaUsr (varchar/text). En tu SQL Server por defecto era NULL.
                cmd.Parameters.AddWithValue("@RazonCitaUsr",
                    string.IsNullOrWhiteSpace(objeto.RazonCitaUsr) ? (object)DBNull.Value : objeto.RazonCitaUsr);

                // DocumentoCitaUsr (bytea)
                var pDoc = cmd.Parameters.Add("@DocumentoCitaUsr", NpgsqlDbType.Bytea);
                pDoc.Value = (objeto.DocumentoCitaUsr != null && objeto.DocumentoCitaUsr.Length > 0)
                    ? (object)objeto.DocumentoCitaUsr
                    : DBNull.Value;

                // ContentType (varchar(50))
                cmd.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrWhiteSpace(objeto.ContentType) ? (object)DBNull.Value : objeto.ContentType);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al actualizar el motivo de la cita";
            }
        }

        public async Task<string> GuardarDocumentoPaciente(Cita objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_actualizardocumentopaciente(@IdCita, @DocumentoCitaUsr, @ContentType);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", objeto.IdCita);

                var pDoc = cmd.Parameters.Add("@DocumentoCitaUsr", NpgsqlDbType.Bytea);
                pDoc.Value = (objeto.DocumentoCitaUsr != null && objeto.DocumentoCitaUsr.Length > 0)
                    ? (object)objeto.DocumentoCitaUsr
                    : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrWhiteSpace(objeto.ContentType) ? (object)DBNull.Value : objeto.ContentType);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al actualizar el documento de la cita";
            }
        }

        public async Task<List<Cita>> ListaHistorialCitas(int IdUsuario)
        {
            var lista = new List<Cita>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listahistorialcitas(@IdUsuario);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var cita = new Cita
                {
                    IdCita = Convert.ToInt32(dr["IdCita"]),
                    FechaCita = dr["FechaCita"].ToString()!,
                    FechaCitaOrden = dr["FechaCitaOrden"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaCitaOrden"]),
                    HoraCita = dr["HoraCita"].ToString()!,

                    Especialidad = new Especialidad
                    {
                        Nombre = dr["NombreEspecialidad"].ToString()!,
                    },
                    Doctor = new Doctor
                    {
                        Nombres = dr["Nombres"].ToString()!,
                        Apellidos = dr["Apellidos"].ToString()!,
                    },

                    // PACIENTE
                    OrigenCita = dr["OrigenCita"].ToString()!,
                    RazonCitaUsr = dr["RazonCitaUsr"].ToString()!,
                    DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],
                    ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"].ToString(),

                    // DOCTOR
                    Indicaciones = dr["Indicaciones"] == DBNull.Value ? string.Empty : dr["Indicaciones"].ToString()!,
                    DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                    ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"].ToString()
                };

                lista.Add(cita);
            }

            return lista;
        }

        public async Task<string> ActualizarDocIndicacionesDoctor(int IdCita, byte[]? doc, string? contentTypeDoctor)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_actualizardocindicacionesdoctor(@IdCita, @DocIndicacionesDoctor, @ContentType_Doctor);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", IdCita);

                var pDoc = cmd.Parameters.Add("@DocIndicacionesDoctor", NpgsqlDbType.Bytea);
                pDoc.Value = (doc != null && doc.Length > 0) ? (object)doc : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType_Doctor",
                    string.IsNullOrWhiteSpace(contentTypeDoctor) ? (object)DBNull.Value : contentTypeDoctor);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                // Igual que tu SP de SQL Server, devolvemos mensaje genérico
                return "No se pudo actualizar el documento del doctor";
            }
        }

        public async Task<string> ActualizarMotivoPaciente(
            int idCita,
            string razonCitaUsr,
            byte[]? documento,
            string? contentType)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_actualizarmotivopaciente(@IdCita, @RazonCitaUsr, @DocumentoCitaUsr, @ContentType);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", idCita);

                cmd.Parameters.AddWithValue("@RazonCitaUsr",
                    string.IsNullOrWhiteSpace(razonCitaUsr) ? (object)DBNull.Value : razonCitaUsr);

                var pDoc = cmd.Parameters.Add("@DocumentoCitaUsr", NpgsqlDbType.Bytea);
                pDoc.Value = (documento != null && documento.Length > 0)
                    ? (object)documento
                    : DBNull.Value;

                cmd.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrWhiteSpace(contentType) ? (object)DBNull.Value : contentType);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "No se pudo actualizar el motivo del paciente";
            }
        }

        public async Task<List<Cita>> ListaCitasAdmin(int idEstadoCita)
        {
            var lista = new List<Cita>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listacitasadmin(@IdEstadoCita);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdEstadoCita", idEstadoCita);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var oCita = new Cita
                {
                    IdCita = Convert.ToInt32(dr["IdCita"]),
                    FechaCita = dr["FechaCita"]?.ToString() ?? "",
                    HoraCita = dr["HoraCita"]?.ToString() ?? "",

                    Usuario = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        Nombre = dr["NombrePaciente"]?.ToString() ?? "",
                        Apellido = dr["ApellidoPaciente"]?.ToString() ?? ""
                    },

                    Doctor = new Doctor
                    {
                        IdDoctor = Convert.ToInt32(dr["IdDoctor"]),
                        Nombres = dr["NombreDoctor"]?.ToString() ?? "",
                        Apellidos = dr["ApellidoDoctor"]?.ToString() ?? ""
                    },

                    Especialidad = new Especialidad
                    {
                        IdEspecialidad = Convert.ToInt32(dr["IdEspecialidad"]),
                        Nombre = dr["NombreEspecialidad"]?.ToString() ?? ""
                    },

                    EstadoCita = new EstadoCita
                    {
                        IdEstadoCita = Convert.ToInt32(dr["IdEstadoCita"]),
                        Nombre = dr["NombreEstadoCita"]?.ToString() ?? ""
                    },

                    FechaCitaOrden = dr["FechaCitaOrden"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaCitaOrden"]),

                    Indicaciones = dr["Indicaciones"]?.ToString() ?? "",
                    OrigenCita = dr["OrigenCita"]?.ToString() ?? "",
                    RazonCitaUsr = dr["RazonCitaUsr"]?.ToString() ?? "",

                    CitaConfirmada = dr["CitaConfirmada"] == DBNull.Value ? null : dr["CitaConfirmada"]?.ToString(),
                    FechaPeticion = dr["FechaPeticion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaPeticion"]),
                    FechaConfirmacion = dr["FechaConfirmacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaConfirmacion"]),

                    DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],
                    ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"]?.ToString(),

                    DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                    ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"]?.ToString()
                };

                lista.Add(oCita);
            }

            return lista;
        }

        // ==================================================
        // Server-side wrappers (DataTables)
        // ==================================================
        // ==================================================
        // Server-side wrapper (DataTables) -> devuelve tupla
        // idEstadoCita: 0 = todas
        // ==================================================
        public async Task<(List<Cita> Lista, int TotalRegistros)> ListaCitasGestionServerSide(
            int idEstadoCita,
            int start,
            int length,
            string filtro)
        {
            return await ListaCitasAdminServerSide(idEstadoCita, start, length, filtro);
        }


        

        private static DateTime? SafeDateTimeNullable(NpgsqlDataReader dr, string colName)
    {
        try
        {
            int ordinal = dr.GetOrdinal(colName);
            if (dr.IsDBNull(ordinal)) return null;
            return Convert.ToDateTime(dr.GetValue(ordinal));
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

        public async Task<(List<Cita> Lista, int TotalRegistros)> ListaCitasAdminServerSide(
            int idEstadoCita,
            int start,
            int length,
            string filtro)
        {
            var lista = new List<Cita>();
            int totalRegistros = 0;

            filtro ??= "";

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // 1) Página -> en Postgres es FUNCTION RETURNS TABLE
            await using (var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_obtener_citasgestion2(@IdEstadoCita, @ignorar_primeros, @cantidad_filas, @filtro);",
                conexion))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdEstadoCita", idEstadoCita);
                cmd.Parameters.AddWithValue("@ignorar_primeros", start);
                cmd.Parameters.AddWithValue("@cantidad_filas", length);
                cmd.Parameters.AddWithValue("@filtro", filtro);

                await using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    var oCita = new Cita
                    {
                        IdCita = Convert.ToInt32(dr["IdCita"]),
                        FechaCitaOrden = dr["FechaCitaOrden"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaCitaOrden"]),
                        FechaCita = dr["FechaCita"]?.ToString() ?? "",
                        HoraCita = dr["HoraCita"]?.ToString() ?? "",

                        Usuario = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Nombre = dr["NombrePaciente"]?.ToString() ?? "",
                            Apellido = dr["ApellidoPaciente"]?.ToString() ?? ""
                        },

                        Doctor = new Doctor
                        {
                            IdDoctor = Convert.ToInt32(dr["IdDoctor"]),
                            Nombres = dr["NombresDoctor"]?.ToString() ?? "",
                            Apellidos = dr["ApellidosDoctor"]?.ToString() ?? ""
                        },

                        Especialidad = new Especialidad
                        {
                            IdEspecialidad = Convert.ToInt32(dr["IdEspecialidad"]),
                            Nombre = dr["NombreEspecialidad"]?.ToString() ?? ""
                        },

                        EstadoCita = new EstadoCita
                        {
                            IdEstadoCita = Convert.ToInt32(dr["IdEstadoCita"]),
                            Nombre = dr["EstadoCita"]?.ToString() ?? ""
                        },

                        Indicaciones = dr["Indicaciones"]?.ToString() ?? "",
                        OrigenCita = dr["OrigenCita"]?.ToString() ?? "",
                        RazonCitaUsr = dr["RazonCitaUsr"]?.ToString() ?? "",

                        CitaConfirmada = dr["CitaConfirmada"] == DBNull.Value ? null : dr["CitaConfirmada"]?.ToString(),
                        FechaPeticion = dr["FechaPeticion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaPeticion"]),
                        FechaConfirmacion = dr["FechaConfirmacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaConfirmacion"]),
                        MetodoPeticion = dr["MetodoPeticion"] == DBNull.Value ? null : dr["MetodoPeticion"]?.ToString(),

                        DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],
                        ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"]?.ToString(),

                        DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                        ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"]?.ToString()
                    };

                    lista.Add(oCita);
                }
            }

            // 2) Total filtrado -> en Postgres es FUNCTION que devuelve integer
            await using (var cmdTotal = new NpgsqlCommand(
                "SELECT public.fn_obtenertotal_citasgestion2(@IdEstadoCita, @filtro);",
                conexion))
            {
                cmdTotal.CommandType = CommandType.Text;
                cmdTotal.Parameters.AddWithValue("@IdEstadoCita", idEstadoCita);
                cmdTotal.Parameters.AddWithValue("@filtro", filtro);

                var scalar = await cmdTotal.ExecuteScalarAsync();
                totalRegistros = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            }

            return (lista, totalRegistros);
        }

        public async Task<string> AdminActualizarCita(Cita cita)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_adminactualizarcita(" +
                    "@IdCita, @IdEstadoCita, @OrigenCita, @RazonCitaUsr, @Indicaciones, " +
                    "@DocPaciente, @ContentTypePaciente, @DocDoctor, @ContentTypeDoctor);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdCita", cita.IdCita);
                cmd.Parameters.AddWithValue("@IdEstadoCita", cita.EstadoCita.IdEstadoCita);

                cmd.Parameters.AddWithValue("@OrigenCita", cita.OrigenCita ?? "");
                cmd.Parameters.AddWithValue("@RazonCitaUsr", cita.RazonCitaUsr ?? "");
                cmd.Parameters.AddWithValue("@Indicaciones", cita.Indicaciones ?? "");

                // Doc paciente: si no hay doc -> NULL para que la función mantenga el valor anterior
                var pDocPaciente = cmd.Parameters.Add("@DocPaciente", NpgsqlDbType.Bytea);
                if (cita.DocumentoCitaUsr != null && cita.DocumentoCitaUsr.Length > 0)
                {
                    pDocPaciente.Value = cita.DocumentoCitaUsr;
                    cmd.Parameters.AddWithValue("@ContentTypePaciente", cita.ContentType ?? "");
                }
                else
                {
                    pDocPaciente.Value = DBNull.Value;
                    cmd.Parameters.AddWithValue("@ContentTypePaciente", DBNull.Value);
                }

                // Doc doctor: si no hay doc -> NULL para que la función mantenga el valor anterior
                var pDocDoctor = cmd.Parameters.Add("@DocDoctor", NpgsqlDbType.Bytea);
                if (cita.DocIndicacionesDoctor != null && cita.DocIndicacionesDoctor.Length > 0)
                {
                    pDocDoctor.Value = cita.DocIndicacionesDoctor;
                    cmd.Parameters.AddWithValue("@ContentTypeDoctor", cita.ContentTypeDoctor ?? "");
                }
                else
                {
                    pDocDoctor.Value = DBNull.Value;
                    cmd.Parameters.AddWithValue("@ContentTypeDoctor", DBNull.Value);
                }

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al actualizar la cita (admin).";
            }
        }
        /* cambio comienzo */
        public async Task<(bool ok, string msg, int idAccion)> EncolarReprogramacion(int idCitaVieja, string nuevaFecha, string nuevaHora, string motivo, string documentoEjecutor)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // 1. VALIDACIÓN DE FECHA Y CULTURA (Blindaje contra el error de mes)
                // Usamos es-ES para asegurar que dd/mm/yyyy se interprete correctamente
                IFormatProvider culture = new System.Globalization.CultureInfo("es-ES");
                DateTime fDate = DateTime.Parse(nuevaFecha, culture);

                // Combinamos fecha y hora para validar que no sea una fecha pasada
                DateTime fechaHoraNueva = fDate.Add(TimeSpan.Parse(nuevaHora));

                if (fechaHoraNueva <= DateTime.Now)
                    return (false, "No se puede reprogramar a una fecha u hora pasada.", 0);

                // 2. VALIDAR CITA ORIGINAL (Estado Pendiente y obtener Datos del Paciente)
                string sqlValidarCita = @"
            SELECT c.idestadocita, u.movil, dh.iddoctor, c.razoncitausr
            FROM public.cita c
            JOIN public.usuario u ON c.idusuario = u.idusuario
            JOIN public.doctorhorariodetalle dhd ON c.iddoctorhorariodetalle = dhd.iddoctorhorariodetalle
            JOIN public.doctorhorario dh ON dhd.iddoctorhorario = dh.iddoctorhorario
            WHERE c.idcita = @id";

                int idEstado = 0;
                string movilPaciente = "";
                int idDoctor = 0;
                string motivoOriginal = "";

                using (var cmd = new NpgsqlCommand(sqlValidarCita, conexion))
                {
                    cmd.Parameters.AddWithValue("id", idCitaVieja);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        idEstado = reader.GetInt32(0);
                        movilPaciente = reader.GetString(1);
                        idDoctor = reader.GetInt32(2);
                        motivoOriginal = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    }
                }

                if (idEstado == 0) return (false, "Cita original no encontrada.", 0);
                if (idEstado != 1) return (false, "Solo se pueden reprogramar citas en estado PENDIENTE.", 0);

                // 3. VALIDAR QUE EL SLOT NUEVO EXISTE Y ESTÁ LIBRE PARA EL MISMO DOCTOR
                string sqlValidarSlot = @"
            SELECT dhd.iddoctorhorariodetalle 
            FROM public.doctorhorariodetalle dhd
            JOIN public.doctorhorario dh ON dhd.iddoctorhorario = dh.iddoctorhorario
            WHERE dh.iddoctor = @idDoc 
              AND dhd.fecha = @f 
              AND dhd.turnohora = @h::time 
              AND dhd.reservado = false";

                bool slotLibre = false;
                using (var cmd = new NpgsqlCommand(sqlValidarSlot, conexion))
                {
                    cmd.Parameters.AddWithValue("idDoc", idDoctor);
                    cmd.Parameters.Add(new NpgsqlParameter("f", NpgsqlTypes.NpgsqlDbType.Date) { Value = fDate });
                    cmd.Parameters.AddWithValue("h", nuevaHora);
                    var res = await cmd.ExecuteScalarAsync();
                    if (res != null) slotLibre = true;
                }

                if (!slotLibre) return (false, "El slot horario no existe o ya ha sido reservado por otro paciente.", 0);

                // 4. SI TODO ESTÁ BIEN -> INSERTAR Y RETORNAR EL ID (RETURNING)
                string sqlInsert = @"
            INSERT INTO public.accionescomplementarias (
                tipoaccion, id_citavieja, movilejecutor, fecharepro, horarapro, motivo, 
                conexion, conexion_vps, pathexewin, pathexevps, repasado, reintentos
            ) VALUES (
                'calrepro', @idV, @movilP, @fStr, @h, @mot,
                'Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9',
                'Host=evolutionapi.n8njigretera.cloud;Port=5432;Database=evolution;Username=postgresql;Password=nnclxgjwswuh94ra',
                'C:\tmp\rios_rosas\c#\regresion\calrepro\publish\win\calrepro.exe',
                '/us2/dbclinica/exe/calreprovps', 'N', 0
            ) RETURNING idaccionescomplementarias";

                int idAccionGenerada = 0;
                using (var cmd = new NpgsqlCommand(sqlInsert, conexion))
                {
                    cmd.Parameters.AddWithValue("idV", idCitaVieja);
                    cmd.Parameters.AddWithValue("movilP", movilPaciente); // MÓVIL DEL PACIENTE para el Orquestador
                    cmd.Parameters.AddWithValue("fStr", fDate.ToString("yyyy-MM-dd")); // Formato ISO para evitar fallos de cultura en el .exe
                    cmd.Parameters.AddWithValue("h", nuevaHora);
                    cmd.Parameters.AddWithValue("mot", motivoOriginal + " - " + (motivo ?? "Reprogramación"));

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) idAccionGenerada = Convert.ToInt32(result);
                }

                return (true, "Procesando sincronización...", idAccionGenerada);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }
        /* cambio fin */
        // ==================================================
        // 3) Métodos que exige ICitaRepositorio: 
        //    ListaCitasGestion() y ListaCitasGestion(int)
        //    Implementados como wrappers de ListaCitasAdmin
        // ==================================================

        public async Task<List<Cita>> ListaCitasGestion()
        {
            // 0 = sin filtro de estado
            return await ListaCitasAdmin(0);
        }

        public async Task<List<Cita>> ListaCitasGestion(int idEstadoCita)
        {
            return await ListaCitasAdmin(idEstadoCita);
        }
        public async Task<CitaDetalleDTO?> ObtenerDetalleCita(int idCita)
        {
            const string SQL = "SELECT * FROM public.cita_obtenerdetalle(@IdCita);";

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(SQL, conexion);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdCita", idCita);

            await using var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await dr.ReadAsync())
                return null;

            DateTime? GetDate(string col) =>
                dr[col] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr[col]);

            string? GetStr(string col) =>
                dr[col] == DBNull.Value ? null : dr[col]?.ToString();

            return new CitaDetalleDTO
            {
                IdCita = Convert.ToInt32(dr["idcita"]), // <- ojo: en tu RETURNS TABLE va en minúsculas
                CitaConfirmada = GetStr("citaconfirmada"),
                FechaPeticion = GetDate("fechapeticion"),
                FechaConfirmacion = GetDate("fechaconfirmacion"),
                MetodoPeticion = GetStr("metodopeticion")
            };
        }

        /* comienzo cambio */
        public async Task<int> ObtenerIdDoctorDeCita(int idCita)
        {
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();
            string sql = @"SELECT dh.iddoctor 
                   FROM public.cita c
                   JOIN public.doctorhorariodetalle dhd ON c.iddoctorhorariodetalle = dhd.iddoctorhorariodetalle
                   JOIN public.doctorhorario dh ON dhd.iddoctorhorario = dh.iddoctorhorario
                   WHERE c.idcita = @id";
            using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("id", idCita);
            var res = await cmd.ExecuteScalarAsync();
            return res != null ? Convert.ToInt32(res) : 0;
        }
        public async Task<List<DoctorHorarioDetalle>> ObtenerSlotsLibres(int idDoctor, string fecha)
        {
            List<DoctorHorarioDetalle> lista = new();
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // SQL CORREGIDO: Se eliminó el error de sintaxis en el JOIN (dh = dhd... -> dh.iddoctorhorario = dhd.iddoctorhorario)
                string sql = @"SELECT dhd.iddoctorhorariodetalle, dhd.turnohora, dhd.turno, dhd.idhorariodetallecalcom
                       FROM public.doctorhorariodetalle dhd
                       JOIN public.doctorhorario dh ON dhd.iddoctorhorario = dh.iddoctorhorario
                       WHERE dh.iddoctor = @idD AND dhd.fecha = @f AND dhd.reservado = false
                       ORDER BY dhd.turnohora ASC";

                using var cmd = new NpgsqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("idD", idDoctor);

                // --- SOLUCIÓN AL FORMATO DE FECHA (MULTI-ENTORNO) ---
                DateTime fParsed;
                string[] formatosSoportados = {
            "dd-MM-yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "d-M-yyyy",
            "d/M/yyyy",
            "yyyy-M-d"
        };

                if (!DateTime.TryParseExact(fecha, formatosSoportados, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fParsed))
                {
                    fParsed = DateTime.Parse(fecha);
                }

                cmd.Parameters.Add(new NpgsqlParameter("f", NpgsqlTypes.NpgsqlDbType.Date) { Value = fParsed });

                using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    lista.Add(new DoctorHorarioDetalle
                    {
                        IdDoctorHorarioDetalle = dr.GetInt32(0),
                        TurnoHora = dr.GetTimeSpan(1).ToString(@"hh\:mm"),
                        Turno = dr.IsDBNull(2) ? "" : dr.GetString(2).Trim().ToUpper(),
                        IdDoctorHorarioDetalleCalcom = dr.IsDBNull(3) ? 0 : Convert.ToInt32(dr.GetValue(3))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error crítico en ObtenerSlotsLibres: " + ex.Message);
                throw;
            }
            return lista;
        }
        public async Task<(bool ok, string msg, int idAccion)> EncolarCancelacion(int idCita, string motivo, string documentoEjecutor)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // 1. VALIDAR CITA (Estado Pendiente y obtener Datos del Paciente)
                string sqlValidar = @"
            SELECT c.idestadocita, u.movil, c.razoncitausr
            FROM public.cita c
            JOIN public.usuario u ON c.idusuario = u.idusuario
            WHERE c.idcita = @id";

                int idEstado = 0;
                string movilPaciente = "";
                string motivoOriginal = "";

                using (var cmd = new NpgsqlCommand(sqlValidar, conexion))
                {
                    cmd.Parameters.AddWithValue("id", idCita);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        idEstado = reader.GetInt32(0);
                        movilPaciente = reader.GetString(1);
                        motivoOriginal = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    }
                }

                if (idEstado == 0) return (false, "Cita no encontrada.", 0);
                if (idEstado != 1) return (false, "Solo se pueden cancelar citas en estado PENDIENTE.", 0);

                // 2. INSERTAR EN ACCIONES COMPLEMENTARIAS (tipo 'cacancel')
                string sqlInsert = @"
            INSERT INTO public.accionescomplementarias (
                tipoaccion, id_citavieja, movilejecutor, motivo, 
                conexion, conexion_vps, pathexewin, pathexevps, repasado, reintentos
            ) VALUES (
                'cacancel', @idV, @movilP, @mot,
                'Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9',
                'Host=evolutionapi.n8njigretera.cloud;Port=5432;Database=evolution;Username=postgresql;Password=nnclxgjwswuh94ra',
                'C:\tmp\rios_rosas\c#\regresion\cacancel\publish\win\cacancel.exe',
                '/us2/dbclinica/exe/cacancelvps', 'N', 0
            ) RETURNING idaccionescomplementarias";

                int idAccionGenerada = 0;
                using (var cmd = new NpgsqlCommand(sqlInsert, conexion))
                {
                    cmd.Parameters.AddWithValue("idV", idCita);
                    cmd.Parameters.AddWithValue("movilP", movilPaciente);
                    cmd.Parameters.AddWithValue("mot", "CANCELADA POR PERSONAL: " + (motivo ?? "Sin motivo especificado"));

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) idAccionGenerada = Convert.ToInt32(result);
                }

                return (true, "Procesando cancelación...", idAccionGenerada);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }
        /* final cambio */
        public async Task<string> CambiarEstadoCitaBD(int idCita, int nuevoEstado)
        {
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Invocamos la función sp_cambiarestadocita(pidcita, pnewestado)
            string sql = "SELECT public.sp_cambiarestadocita(@id, @estado)";

            using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("id", idCita);
            cmd.Parameters.AddWithValue("estado", nuevoEstado);

            var res = await cmd.ExecuteScalarAsync();

            // Si res es null o vacío, la operación fue exitosa. Si tiene texto, es el error.
            return res != null ? res.ToString() : "";
        }
        public async Task<int> ObtenerUltimaAccionCancelacion(int idCita)
        {
            // Usamos Conexion.CN que es la que te funciona en el Controller
            await using var conexion = new Npgsql.NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Ajustado según tu SQL: 
            // Tabla: accionescomplementarias
            // ID: idaccionescomplementarias
            // Relación: id_cita (aquí sí lleva guion bajo según tu CREATE TABLE)
            string sql = @"SELECT idaccionescomplementarias 
                   FROM public.accionescomplementarias 
                   WHERE id_cita = @id 
                   ORDER BY idaccionescomplementarias DESC 
                   LIMIT 1";

            using var cmd = new Npgsql.NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("id", idCita);

            var res = await cmd.ExecuteScalarAsync();
            return res != null ? Convert.ToInt32(res) : 0;
        }

        public async Task<string> ConsultarEstadoAccion(int idAccion)
        {
            await using var conexion = new Npgsql.NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Columna según tu SQL: 'repasado' (es el que tiene 'N', 'S', etc.)
            string sql = "SELECT repasado FROM public.accionescomplementarias WHERE idaccionescomplementarias = @id";

            using var cmd = new Npgsql.NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("id", idAccion);

            var res = await cmd.ExecuteScalarAsync();
            return res?.ToString() ?? "";
        }
        public async Task<(bool ok, int idAccion)> CancelarDesdeAgenda(int idCita, string motivo)
        {
            try
            {
                await using var conexion = new Npgsql.NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // Ahora pasamos 3 parámetros: ID, Estado(3) y Motivo
                using var cmdSp = new Npgsql.NpgsqlCommand("SELECT public.sp_cambiarestadocita_zz(@id, 3, @motivo)", conexion);
                cmdSp.Parameters.AddWithValue("id", idCita);
                cmdSp.Parameters.AddWithValue("motivo", motivo ?? "");

                await cmdSp.ExecuteScalarAsync();

                // Buscamos el ID generado
                string sqlId = @"SELECT idaccionescomplementarias FROM public.accionescomplementarias 
                         WHERE id_cita = @id ORDER BY idaccionescomplementarias DESC LIMIT 1";
                using var cmdId = new Npgsql.NpgsqlCommand(sqlId, conexion);
                cmdId.Parameters.AddWithValue("id", idCita);
                var res = await cmdId.ExecuteScalarAsync();

                return (true, res != null ? Convert.ToInt32(res) : 0);
            }
            catch (Exception) { return (false, 0); }
        }
        /* comienzo cambio sincro CORREGIDO */
        public async Task<(bool ok, int idAccion)> EncolarChequeoCita(int idCita, string refresh)
        {
            try
            {
                using var conn = new NpgsqlConnection(con.CadenaSQL);

                // Definimos las conexiones y rutas manualmente para que el orquestador las tenga
                string sql = @"INSERT INTO public.accionescomplementarias 
              (tipoaccion, id_cita, refresh, repasado, reintentos, fecharegistro, 
               conexion, conexion_vps, pathexewin, pathexevps) 
              VALUES 
              ('chkcitacalcom', @idCita, @refresh, 'N', 0, NOW(),
               'Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9',
               'Host=evolutionapi.n8njigretera.cloud;Port=5432;Database=evolution;Username=postgresql;Password=nnclxgjwswuh94ra',
               'C:\tmp\rios_rosas\c#\regresion\chkcitacalcom\bin\Release\net8.0\chkcitacalcom.exe',
               '/us2/dbclinica/exe/chkcitacalcomvps'
                ) 
              RETURNING idaccionescomplementarias;";

                int id = await conn.ExecuteScalarAsync<int>(sql, new { idCita, refresh });
                return (true, id);
            }
            catch (Exception ex)
            {
                // Loguea el error si es necesario para depurar
                Console.WriteLine("Error al encolar: " + ex.Message);
                return (false, 0);
            }
        }

        public async Task<string> ConsultarEstadoAccionSincro(int idAccion)
        {
            using var conn = new NpgsqlConnection(con.CadenaSQL);
            return await conn.ExecuteScalarAsync<string>(
                "SELECT repasado FROM public.accionescomplementarias WHERE idaccionescomplementarias = @id",
                new { id = idAccion }) ?? "N";
        }

        public async Task<(bool ok, dynamic data)> ObtenerResultadoSincroDetalle(int idCita)
        {
            using var conn = new NpgsqlConnection(con.CadenaSQL);
            var data = await conn.QueryFirstOrDefaultAsync(@"
        SELECT resultsincro, fecha_sincro, cal_last_error, booking_uid, booking_status
        FROM public.doctorhorariodetallecalcom 
        WHERE idcita = @idCita", new { idCita });
            return (data != null, data);
        }
        /* fin cambio sincro */
        /* inicio implementacion cita calcom */
        public async Task<DoctorHorarioDetalleCalcom> ObtenerDetalleCalcom(int idDoctorHorarioDetalle)
        {
            DoctorHorarioDetalleCalcom detalle = null;
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                string query = @"SELECT idhorariodetallecalcom, iddoctorhorariodetalle, iddoctor, fecha_slot, 
                                turnohora, booking_uid, booking_status, paciente_nombre_completo, 
                                paciente_apellido, paciente_email, paciente_movil, razoncitausr, 
                                indicaciones, cal_json_full, idcita, resultsincro
                         FROM public.doctorhorariodetallecalcom 
                         WHERE iddoctorhorariodetalle = @idSlot";

                using (var cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@idSlot", idDoctorHorarioDetalle);
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            detalle = new DoctorHorarioDetalleCalcom
                            {
                                IdHorarioDetalleCalcom = (long)dr["idhorariodetallecalcom"],
                                IdDoctorHorarioDetalle = dr["iddoctorhorariodetalle"] as int?,
                                IdDoctor = dr["iddoctor"] as int?,
                                PacienteNombreCompleto = dr["paciente_nombre_completo"]?.ToString(),
                                PacienteApellido = dr["paciente_apellido"]?.ToString(),
                                PacienteEmail = dr["paciente_email"]?.ToString(),
                                PacienteMovil = dr["paciente_movil"]?.ToString(),
                                Razoncitausr = dr["razoncitausr"]?.ToString(),
                                CalJsonFull = dr["cal_json_full"]?.ToString(),
                                BookingUid = dr["booking_uid"]?.ToString(),
                                IdCita = dr["idcita"] as int?
                            };
                        }
                    }
                }
            }
            return detalle;
        }

        public async Task<bool> GuardarDetalleCalcom(DoctorHorarioDetalleCalcom m)
        {
            using (var conexion = new NpgsqlConnection(con.CadenaSQL))
            {
                await conexion.OpenAsync();
                // SQL Upsert (ON CONFLICT) para PostgreSQL
                string query = @"
            INSERT INTO public.doctorhorariodetallecalcom 
            (iddoctorhorariodetalle, iddoctor, fecha_slot, turnohora, booking_uid, booking_status, 
             paciente_nombre_completo, paciente_apellido, paciente_email, paciente_movil, 
             razoncitausr, cal_json_full, idcita, resultsincro, ultima_sincronizacion)
            VALUES 
            (@idSlot, @idDoc, @fecha, @hora, @uid, @status, @nom, @ape, @email, @movil, @razon, @json, @idCita, @res, NOW())
            ON CONFLICT (iddoctorhorariodetalle) 
            DO UPDATE SET 
                booking_status = EXCLUDED.booking_status,
                paciente_nombre_completo = EXCLUDED.paciente_nombre_completo,
                paciente_apellido = EXCLUDED.paciente_apellido,
                paciente_email = EXCLUDED.paciente_email,
                paciente_movil = EXCLUDED.paciente_movil,
                razoncitausr = EXCLUDED.razoncitausr,
                cal_json_full = EXCLUDED.cal_json_full,
                ultima_sincronizacion = NOW();";

                using (var cmd = new NpgsqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@idSlot", m.IdDoctorHorarioDetalle);
                    cmd.Parameters.AddWithValue("@idDoc", (object)m.IdDoctor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fecha", (object)m.FechaSlot ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@hora", (object)m.TurnoHora ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@uid", (object)m.BookingUid ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", (object)m.BookingStatus ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nom", (object)m.PacienteNombreCompleto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ape", (object)m.PacienteApellido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", (object)m.PacienteEmail ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@movil", (object)m.PacienteMovil ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@razon", (object)m.Razoncitausr ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@json", (object)m.CalJsonFull ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idCita", (object)m.IdCita ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@res", (object)m.ResultSincro ?? "S");

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
        /* fin implementacion cita calcom FUERZO RECOMPILACION */
    }
}
