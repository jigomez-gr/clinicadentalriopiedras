using ClinicaData.Contrato;
using ClinicaEntidades;
using Npgsql;
using Dapper; /* IMPORTANTE: Sin esto falla el ExecuteScalarAsync */
using System.Data;
using ClinicaData.Configuracion;
using Microsoft.Extensions.Options;
using ClinicaEntidades.DTO;
using System.Xml.Linq;
using NpgsqlTypes;
namespace ClinicaData.Implementacion
{
    public class DoctorRepositorio : IDoctorRepositorio
    {
        private readonly ConnectionStrings con;
        public DoctorRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }

        public async Task<string> Editar(Doctor objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_editardoctor(@Id, @NumDoc, @Nombres, @Apellidos, @Genero, @Archivo, @Bio, @Val, @ValAi, @Resumen);",
                    conexion);

                cmd.Parameters.AddWithValue("@Id", objeto.IdDoctor);
                cmd.Parameters.AddWithValue("@NumDoc", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombres", objeto.Nombres);
                cmd.Parameters.AddWithValue("@Apellidos", objeto.Apellidos);
                cmd.Parameters.AddWithValue("@Genero", objeto.Genero);
                cmd.Parameters.AddWithValue("@Archivo", (object?)objeto.Archivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bio", (object?)objeto.Biografia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Val", objeto.Valoracion);
                cmd.Parameters.AddWithValue("@ValAi", objeto.ValoracionAi);
                cmd.Parameters.AddWithValue("@Resumen", (object?)objeto.ResumenValoracion ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                return "Error al editar el doctor: " + ex.Message;
            }
        }



        public async Task<int> Eliminar(int Id)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_eliminardoctor(@IdDoctor);",
                    conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdDoctor", Id);

                var result = await cmd.ExecuteScalarAsync();
                var msg = result?.ToString() ?? "";

                // si la función devuelve '' -> OK
                return string.IsNullOrEmpty(msg) ? 1 : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<string> EliminarHorario(int Id)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_eliminardoctorhorario(@IdDoctorHorario);",
                    conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdDoctorHorario", Id);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al eliminar el horario";
            }
        }


        public async Task<string> Guardar(Doctor objeto)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "SELECT public.sp_guardardoctor(@NumDoc, @Nombres, @Apellidos, @Genero, @Archivo, @Bio, @Val, @ValAi, @Resumen);",
                    conexion);

                cmd.Parameters.AddWithValue("@NumDoc", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombres", objeto.Nombres);
                cmd.Parameters.AddWithValue("@Apellidos", objeto.Apellidos);
                cmd.Parameters.AddWithValue("@Genero", objeto.Genero);
                cmd.Parameters.AddWithValue("@Archivo", (object?)objeto.Archivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bio", (object?)objeto.Biografia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Val", objeto.Valoracion);
                cmd.Parameters.AddWithValue("@ValAi", objeto.ValoracionAi);
                cmd.Parameters.AddWithValue("@Resumen", (object?)objeto.ResumenValoracion ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                return "Error al guardar el doctor: " + ex.Message;
            }
        }
        public async Task<List<Doctor>> Lista()
        {
            var lista = new List<Doctor>();

            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // 1. Cargamos los doctores desde tu SP (nombres, apellidos, foto, IA...)
                await using var cmd = new NpgsqlCommand("SELECT * FROM public.sp_listadoctor();", conexion);
                cmd.CommandType = CommandType.Text;

                await using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    lista.Add(new Doctor
                    {
                        IdDoctor = Convert.ToInt32(dr["iddoctor"]),
                        NumeroDocumentoIdentidad = dr["numerodocumentoidentidad"]?.ToString() ?? "",
                        Nombres = dr["nombres"]?.ToString() ?? "",
                        Apellidos = dr["apellidos"]?.ToString() ?? "",
                        Genero = dr["genero"]?.ToString() ?? "",
                        FechaCreacion = dr["fechacreacion"]?.ToString() ?? "",
                        Archivo = dr["archivo"] != DBNull.Value ? (byte[])dr["archivo"] : null,
                        Biografia = dr["biografia"] != DBNull.Value ? dr["biografia"].ToString() : "",
                        Valoracion = dr["valoracion"] != DBNull.Value ? Convert.ToDecimal(dr["valoracion"]) : 3,
                        ValoracionAi = dr["valoracionai"] != DBNull.Value ? Convert.ToDecimal(dr["valoracionai"]) : 3,
                        ResumenValoracion = dr["resumenvaloracion"] != DBNull.Value ? dr["resumenvaloracion"].ToString() : "",

                        // Inicializamos la lista de tu clase Doctor.cs
                        EspecialidadesDoc = new List<EspecialidadesDoctor>(),
                        // Parche de compatibilidad (le ponemos un nombre cualquiera)
                        Especialidad = new Especialidad { Nombre = "Varias" }
                    });
                }
                await dr.CloseAsync();

                // 2. Cargamos las especialidades desde la tabla public.especialidadesdoctor
                foreach (var doc in lista)
                {
                    // Usamos el nombre exacto de tu tabla: public.especialidadesdoctor
                    const string sqlEsp = @"
                SELECT ed.idespecialidad, e.nombre 
                FROM public.especialidadesdoctor ed
                INNER JOIN public.especialidad e ON e.idespecialidad = ed.idespecialidad
                WHERE ed.iddoctor = @id";

                    await using var cmdEsp = new NpgsqlCommand(sqlEsp, conexion);
                    cmdEsp.Parameters.AddWithValue("id", doc.IdDoctor);

                    await using var drEsp = await cmdEsp.ExecuteReaderAsync();
                    while (await drEsp.ReadAsync())
                    {
                        doc.EspecialidadesDoc.Add(new EspecialidadesDoctor
                        {
                            IdDoctor = doc.IdDoctor,
                            IdEspecialidad = Convert.ToInt32(drEsp["idespecialidad"]),
                            NombreEspecialidad = drEsp["nombre"].ToString()
                        });
                    }
                    await drEsp.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                // Aquí podrías poner un punto de interrupción para ver si hay algún otro error
            }

            return lista;
        }
        public async Task<List<Cita>> ListaCitasAsignadas(int Id, int IdEstadoCita)
        {
            var lista = new List<Cita>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listacitasasignadas(@IdDoctor, @IdEstadoCita);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdDoctor", Id);               // en PG la function filtra por doctor
            cmd.Parameters.AddWithValue("@IdEstadoCita", IdEstadoCita);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var cita = new Cita
                {
                    IdCita = Convert.ToInt32(dr["IdCita"]),
                    FechaCita = dr["FechaCita"].ToString()!,
                    HoraCita = dr["HoraCita"].ToString()!,
                    Usuario = new Usuario
                    {
                        Nombre = dr["Nombre"].ToString()!,
                        Apellido = dr["Apellido"].ToString()!,
                    },
                    EstadoCita = new EstadoCita
                    {
                        Nombre = dr["EstadoCita"].ToString()!
                    },
                    Indicaciones = dr["Indicaciones"]?.ToString() ?? string.Empty,

                    OrigenCita = dr["OrigenCita"] == DBNull.Value ? string.Empty : dr["OrigenCita"].ToString()!,
                    RazonCitaUsr = dr["RazonCitaUsr"] == DBNull.Value ? string.Empty : dr["RazonCitaUsr"].ToString()!,
                    ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"].ToString(),
                    DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],

                    DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                    ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"].ToString()
                };

                lista.Add(cita);
            }

            return lista;
        }


public async Task<(List<Cita> Lista, int TotalRegistros)> ListaCitasAsignadasServerSide(
    int idDoctor,
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

        // 1) Página (FUNCTION RETURNS TABLE -> SELECT * FROM ...)
        await using (var cmd = new NpgsqlCommand(
            "SELECT * FROM public.sp_obtener_citasasignadas2(@IdDoctor, @IdEstadoCita, @ignorar_primeros, @cantidad_filas, @filtro);",
            conexion))
        {
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@IdDoctor", idDoctor);
            cmd.Parameters.AddWithValue("@IdEstadoCita", idEstadoCita);
            cmd.Parameters.AddWithValue("@ignorar_primeros", start);
            cmd.Parameters.AddWithValue("@cantidad_filas", length);
            cmd.Parameters.AddWithValue("@filtro", filtro);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var cita = new Cita
                {
                    IdCita = Convert.ToInt32(dr["IdCita"]),
                    FechaCitaOrden = dr["FechaCitaOrden"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaCitaOrden"]),
                    FechaCita = dr["FechaCita"]?.ToString() ?? "",
                    HoraCita = dr["HoraCita"]?.ToString() ?? "",

                    Usuario = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        Nombre = dr["Nombre"]?.ToString() ?? "",
                        Apellido = dr["Apellido"]?.ToString() ?? ""
                    },

                    Doctor = new Doctor
                    {
                        IdDoctor = Convert.ToInt32(dr["IdDoctor"])
                    },

                    EstadoCita = new EstadoCita
                    {
                        IdEstadoCita = Convert.ToInt32(dr["IdEstadoCita"]),
                        Nombre = dr["EstadoCita"]?.ToString() ?? ""
                    },

                    Indicaciones = dr["Indicaciones"]?.ToString() ?? string.Empty,
                    OrigenCita = dr["OrigenCita"] == DBNull.Value ? string.Empty : dr["OrigenCita"]?.ToString() ?? "",
                    RazonCitaUsr = dr["RazonCitaUsr"] == DBNull.Value ? string.Empty : dr["RazonCitaUsr"]?.ToString() ?? "",

                    ContentType = dr["ContentType"] == DBNull.Value ? null : dr["ContentType"]?.ToString(),
                    DocumentoCitaUsr = dr["DocumentoCitaUsr"] == DBNull.Value ? null : (byte[])dr["DocumentoCitaUsr"],

                    DocIndicacionesDoctor = dr["DocIndicacionesDoctor"] == DBNull.Value ? null : (byte[])dr["DocIndicacionesDoctor"],
                    ContentTypeDoctor = dr["ContentType_Doctor"] == DBNull.Value ? null : dr["ContentType_Doctor"]?.ToString(),

                    CitaConfirmada = dr["CitaConfirmada"] == DBNull.Value ? null : dr["CitaConfirmada"]?.ToString(),
                    FechaPeticion = dr["FechaPeticion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaPeticion"]),
                    FechaConfirmacion = dr["FechaConfirmacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaConfirmacion"]),
                    MetodoPeticion = dr["MetodoPeticion"] == DBNull.Value ? null : dr["MetodoPeticion"]?.ToString(),
                
                    // NUEVOS CAMPOS DE VALORACIÓN
                    ValDoctorCita = dr["ValDoctorCita"] == DBNull.Value ? 3 : Convert.ToInt32(dr["ValDoctorCita"]),
                    OpinionDoctorYClinica = dr["OpinionDoctorYClinica"] == DBNull.Value ? null : dr["OpinionDoctorYClinica"]?.ToString()
                };

                lista.Add(cita);
            }
        }

        // 2) Total filtrado (FUNCTION returns int -> SELECT fn(...))
        await using (var cmdTotal = new NpgsqlCommand(
            "SELECT public.fn_obtenertotal_citasasignadas2(@IdDoctor, @IdEstadoCita, @filtro);",
            conexion))
        {
            cmdTotal.CommandType = CommandType.Text;
            cmdTotal.Parameters.AddWithValue("@IdDoctor", idDoctor);
            cmdTotal.Parameters.AddWithValue("@IdEstadoCita", idEstadoCita);
            cmdTotal.Parameters.AddWithValue("@filtro", filtro);

            var scalar = await cmdTotal.ExecuteScalarAsync();
            totalRegistros = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        return (lista, totalRegistros);
    }

        public async Task<List<DoctorHorario>> ListaDoctorHorario()
        {
            var lista = new List<DoctorHorario>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Postgres: FUNCTION RETURNS TABLE -> SELECT * FROM ...
            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listadoctorhorario();",
                conexion);

            cmd.CommandType = CommandType.Text;

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new DoctorHorario
                {
                    IdDoctorHorario = Convert.ToInt32(dr["iddoctorhorario"]), // o "IdDoctorHorario" si viene con alias
                    Doctor = new Doctor
                    {
                        NumeroDocumentoIdentidad = dr["numerodocumentoidentidad"].ToString()!,
                        Nombres = dr["nombres"].ToString()!,
                        Apellidos = dr["apellidos"].ToString()!,
                    },
                    NumeroMes = Convert.ToInt32(dr["numeromes"]),
                    HoraInicioAM = dr["horainicioam"].ToString()!,
                    HoraFinAM = dr["horafinam"].ToString()!,
                    HoraInicioPM = dr["horainiciopm"].ToString()!,
                    HoraFinPM = dr["horafinpm"].ToString()!,
                    FechaCreacion = dr["fechacreacion"].ToString()!
                });
            }

            return lista;
        }

        public async Task<List<FechaAtencionDTO>> ListaDoctorHorarioDetalle(int idDoctor)
        {
            var lista = new List<FechaAtencionDTO>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Postgres: FUNCTION RETURNS xml (scalar) -> SELECT function(...)
            await using var cmd = new NpgsqlCommand(
                "SELECT public.sp_listadoctorhorariodetalle(@IdDoctor);",
                conexion);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@IdDoctor", idDoctor);

            object? scalar = await cmd.ExecuteScalarAsync();
            if (scalar == null || scalar == DBNull.Value)
                return lista;

            // Npgsql suele devolver xml como string
            string xml = Convert.ToString(scalar) ?? "";
            if (string.IsNullOrWhiteSpace(xml))
                return lista;

            var doc = XDocument.Parse(xml);

            var root = doc.Element("HorarioDoctor");
            if (root == null)
                return lista;

            lista = root.Elements("FechaAtencion")
                .Select(fechaAtencion => new FechaAtencionDTO
                {
                    Fecha = (string?)fechaAtencion.Element("Fecha") ?? "",
                    HorarioDTO = (fechaAtencion.Element("Horarios")?.Elements("Hora") ?? Enumerable.Empty<XElement>())
                        .Select(hora => new HorarioDTO
                        {
                            IdDoctorHorarioDetalle = (int?)hora.Element("IdDoctorHorarioDetalle") ?? 0,
                            Turno = (string?)hora.Element("Turno") ?? "",
                            TurnoHora = (string?)hora.Element("TurnoHora") ?? ""
                        })
                        .ToList()
                })
                .ToList();

            return lista;
        }

        /* INICIO REPOSITORIO CORREGIDO */
        public async Task<List<FechaAtencionDTO>> ListaDoctorHorarioDetalleConCitas(int idDoctor)
        {
            var lista = new List<FechaAtencionDTO>();
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT public.sp_listadoctorhorariodetalleconcitas(@IdDoctor);", conexion);
                cmd.Parameters.AddWithValue("@IdDoctor", idDoctor);

                object? scalar = await cmd.ExecuteScalarAsync();
                if (scalar == null || scalar == DBNull.Value) return lista;

                string xml = Convert.ToString(scalar) ?? "";
                if (string.IsNullOrWhiteSpace(xml)) return lista;

                var doc = XDocument.Parse(xml);
                var root = doc.Element("HorarioDoctor");
                if (root == null) return lista;

                lista = root.Elements("FechaAtencion")
                    .Select(fa => new FechaAtencionDTO
                    {
                        Fecha = (string?)fa.Element("Fecha") ?? "",
                        HorarioDTO = (fa.Element("Horarios")?.Elements("Hora") ?? Enumerable.Empty<XElement>())
                            .Select(h => new HorarioDTO
                            {
                                IdDoctorHorarioDetalle = (int?)h.Element("IdDoctorHorarioDetalle") ?? 0,
                                // Mapeo exacto al nombre del DTO y alias del XML
                                IdDoctorHorarioDetalleCalcom = (int?)h.Element("IdDoctorHorarioDetalleCalcom") ?? 0,
                                Turno = (string?)h.Element("Turno") ?? "",
                                TurnoHora = (string?)h.Element("TurnoHora") ?? ""
                            })
                            .ToList()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ListaDoctorHorarioDetalleConCitas: " + ex.Message);
            }
            return lista;
        }
        /* FINAL REPOSITORIO CORREGIDO */
        public async Task<string> RegistrarHorario(DoctorHorario objeto)
        {
            string respuesta = "";

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Postgres: FUNCTION que retorna varchar => SELECT ... y ExecuteScalar
            await using var cmd = new NpgsqlCommand(@"
        SELECT public.sp_registrardoctorhorario(
            @IdDoctor,
            @NumeroMes,
            @HoraInicioAM,
            @HoraFinAM,
            @HoraInicioPM,
            @HoraFinPM,
            @Fechas
        );", conexion);

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@IdDoctor", objeto.Doctor.IdDoctor);
            cmd.Parameters.AddWithValue("@NumeroMes", objeto.NumeroMes);

            // OJO: en tu función conviertes varchar->time (p_horainicioam::time), así que aquí manda "HH:mm"
            cmd.Parameters.AddWithValue("@HoraInicioAM", objeto.HoraInicioAM ?? "");
            cmd.Parameters.AddWithValue("@HoraFinAM", objeto.HoraFinAM ?? "");
            cmd.Parameters.AddWithValue("@HoraInicioPM", objeto.HoraInicioPM ?? "");
            cmd.Parameters.AddWithValue("@HoraFinPM", objeto.HoraFinPM ?? "");

            // string con fechas "DD/MM/YYYY,DD/MM/YYYY,..."
            cmd.Parameters.AddWithValue("@Fechas", objeto.DoctorHorarioDetalle.Fecha ?? "");

            try
            {
                var scalar = await cmd.ExecuteScalarAsync();
                respuesta = (scalar == null || scalar == DBNull.Value) ? "" : Convert.ToString(scalar) ?? "";
            }
            catch
            {
                respuesta = "Error al registrar el horario";
            }

            return respuesta;
        }

        public async Task<Doctor?> ObtenerPorNumeroDocumentoIdentidad(string numeroDocumentoIdentidad)
        {
            var num = (numeroDocumentoIdentidad ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(num)) return null;

            // No existe SP específico en este proyecto; reutilizamos Lista() y filtramos.
            var doctores = await Lista();
            return doctores.FirstOrDefault(d =>
                (d.NumeroDocumentoIdentidad ?? string.Empty).Trim()
                    .Equals(num, StringComparison.OrdinalIgnoreCase));
        }
        // --- NUEVO: eliminar un slot (DoctorHorarioDetalle) ---

        public async Task<string> EliminarSlot(int idDoctorHorarioDetalle)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                // Postgres: FUNCTION => SELECT y ExecuteScalar
                await using var cmd = new NpgsqlCommand(@"
            SELECT public.sp_eliminardoctorhorariodetalle(@IdDoctorHorarioDetalle);
        ", conexion);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@IdDoctorHorarioDetalle", idDoctorHorarioDetalle);

                var scalar = await cmd.ExecuteScalarAsync();
                var msg = (scalar == null || scalar == DBNull.Value) ? "" : Convert.ToString(scalar) ?? "";

                return msg; // "" si OK, o el mensaje si no
            }
            catch (Exception ex)
            {
                // error real de conexión/SQL, etc.
                return ex.Message;
            }
        }

        // ✅ CAMBIO MÍNIMO: turnoHora como TimeOnly
        public async Task<string> AgregarSlot(int idDoctor, DateTime fecha, TimeOnly turnoHora)
        {
            try
            {
                using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                int mes = fecha.Month;

                int idDoctorHorario;
                TimeOnly? horaInicioAM = null, horaFinAM = null, horaInicioPM = null, horaFinPM = null;

                // 1) Resolver DoctorHorario del doctor + mes
                using (var cmdH = new NpgsqlCommand(@"
            SELECT
                iddoctorhorario,
                horainicioam, horafinam,
                horainiciopm, horafinpm
            FROM public.doctorhorario
            WHERE iddoctor = @idDoctor AND numeromes = @mes
            ORDER BY iddoctorhorario DESC
            LIMIT 1;
        ", conexion))
                {
                    cmdH.Parameters.AddWithValue("@idDoctor", idDoctor);
                    cmdH.Parameters.AddWithValue("@mes", mes);

                    using var dr = await cmdH.ExecuteReaderAsync();
                    if (!await dr.ReadAsync())
                        return $"No existe DoctorHorario para el doctor {idDoctor} en el mes {mes} (NumeroMes).";

                    idDoctorHorario = Convert.ToInt32(dr["iddoctorhorario"]);

                    if (dr["horainicioam"] != DBNull.Value) horaInicioAM = (TimeOnly)dr["horainicioam"];
                    if (dr["horafinam"] != DBNull.Value) horaFinAM = (TimeOnly)dr["horafinam"];
                    if (dr["horainiciopm"] != DBNull.Value) horaInicioPM = (TimeOnly)dr["horainiciopm"];
                    if (dr["horafinpm"] != DBNull.Value) horaFinPM = (TimeOnly)dr["horafinpm"];
                }

                // 2) Inferir turno
                string turno = InferirTurno(turnoHora, horaInicioAM, horaFinAM, horaInicioPM, horaFinPM);

                // 3) Buscar slot en Cal.com
                long? idHorarioDetalleCalcom = null;
                bool existeSlotCalcom = false;

                using (var cmdCal = new NpgsqlCommand(@"
            SELECT idhorariodetallecalcom
            FROM public.doctorhorariodetallecalcom
            WHERE iddoctor = @idDoctor
              AND fecha_slot = @fecha
              AND turnohora = @hora
            ORDER BY idhorariodetallecalcom DESC
            LIMIT 1;
        ", conexion))
                {
                    cmdCal.Parameters.AddWithValue("@idDoctor", idDoctor);
                    cmdCal.Parameters.AddWithValue("@fecha", fecha.Date);
                    cmdCal.Parameters.AddWithValue("@hora", turnoHora);

                    var obj = await cmdCal.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                    {
                        idHorarioDetalleCalcom = Convert.ToInt64(obj);
                        existeSlotCalcom = true;
                    }
                }

                // 4) Ver si ya existe el slot local
                using (var cmdChk = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM public.doctorhorariodetalle
            WHERE iddoctorhorario = @idh
              AND fecha = @fecha
              AND turnohora = @hora;
        ", conexion))
                {
                    cmdChk.Parameters.AddWithValue("@idh", idDoctorHorario);
                    cmdChk.Parameters.AddWithValue("@fecha", fecha.Date);
                    cmdChk.Parameters.AddWithValue("@hora", turnoHora);

                    var n = Convert.ToInt32(await cmdChk.ExecuteScalarAsync());

                    if (n > 0)
                    {
                        using var cmdUpd = new NpgsqlCommand(@"
                    UPDATE public.doctorhorariodetalle
                    SET existe_slot_calcom = @existeSlotCalcom,
                        idhorariodetallecalcom = @idhorariodetallecalcom
                    WHERE iddoctorhorario = @idh
                      AND fecha = @fecha
                      AND turnohora = @hora;
                ", conexion);

                        cmdUpd.Parameters.AddWithValue("@existeSlotCalcom", existeSlotCalcom);
                        cmdUpd.Parameters.AddWithValue("@idh", idDoctorHorario);
                        cmdUpd.Parameters.AddWithValue("@fecha", fecha.Date);
                        cmdUpd.Parameters.AddWithValue("@hora", turnoHora);

                        if (idHorarioDetalleCalcom.HasValue)
                            cmdUpd.Parameters.AddWithValue("@idhorariodetallecalcom", idHorarioDetalleCalcom.Value);
                        else
                            cmdUpd.Parameters.AddWithValue("@idhorariodetallecalcom", DBNull.Value);

                        await cmdUpd.ExecuteNonQueryAsync();

                        return "Ese slot ya existe para esa fecha. Se actualizó la referencia a Cal.com si correspondía.";
                    }
                }

                // 5) Insertar
                using (var cmdIns = new NpgsqlCommand(@"
            INSERT INTO public.doctorhorariodetalle
            (iddoctorhorario, fecha, turno, turnohora, reservado, fechacreacion, existe_slot_calcom, idhorariodetallecalcom)
            VALUES
            (@idh, @fecha, @turno, @hora, false, NOW(), @existeSlotCalcom, @idhorariodetallecalcom);
        ", conexion))
                {
                    cmdIns.Parameters.AddWithValue("@idh", idDoctorHorario);
                    cmdIns.Parameters.AddWithValue("@fecha", fecha.Date);
                    cmdIns.Parameters.AddWithValue("@turno", turno);
                    cmdIns.Parameters.AddWithValue("@hora", turnoHora);
                    cmdIns.Parameters.AddWithValue("@existeSlotCalcom", existeSlotCalcom);

                    if (idHorarioDetalleCalcom.HasValue)
                        cmdIns.Parameters.AddWithValue("@idhorariodetallecalcom", idHorarioDetalleCalcom.Value);
                    else
                        cmdIns.Parameters.AddWithValue("@idhorariodetallecalcom", DBNull.Value);

                    await cmdIns.ExecuteNonQueryAsync();
                }

                return "";
            }
            catch (Exception ex)
            {
                return "Error al añadir slot: " + ex.Message;
            }
        }
        // ✅ Si ya la tienes, sustituye SOLO la firma y tipos:
        private static string InferirTurno(TimeOnly turnoHora, TimeOnly? horaInicioAM, TimeOnly? horaFinAM, TimeOnly? horaInicioPM, TimeOnly? horaFinPM)
        {
            if (horaInicioAM.HasValue && horaFinAM.HasValue)
            {
                if (turnoHora >= horaInicioAM.Value && turnoHora <= horaFinAM.Value) return "AM";
            }

            if (horaInicioPM.HasValue && horaFinPM.HasValue)
            {
                if (turnoHora >= horaInicioPM.Value && turnoHora <= horaFinPM.Value) return "PM";
            }

            // Fallback
            return turnoHora.Hour < 12 ? "AM" : "PM";
        }
        public async Task<DoctorApiKeyCalcom?> ObtenerCalcom(int idDoctor)
        {
            await using var cn = new NpgsqlConnection(con.CadenaSQL);
            await cn.OpenAsync();

            const string sql = @"
SELECT iddoctor, email, telefono, nombreyapellido, apikey,
       cal_username, cal_event_type_id, cal_public_url,
       event_inbox, event_inbox_url, is_active, notes,
        cal_event_name , cal_slot_interval 
FROM public.doctoresapikeycalcom
WHERE iddoctor = @iddoctor
LIMIT 1;
";
            await using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@iddoctor", idDoctor);

            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new DoctorApiKeyCalcom
            {
                IdDoctor = rd.GetInt32(0),
                Email = rd.IsDBNull(1) ? null : rd.GetString(1),
                Telefono = rd.IsDBNull(2) ? null : rd.GetString(2),
                NombreYApellido = rd.IsDBNull(3) ? null : rd.GetString(3),
                ApiKey = rd.IsDBNull(4) ? "" : rd.GetString(4),
                CalUsername = rd.IsDBNull(5) ? null : rd.GetString(5),
                CalEventTypeId = rd.IsDBNull(6) ? null : rd.GetInt32(6),
                CalPublicUrl = rd.IsDBNull(7) ? null : rd.GetString(7),
                EventInbox = rd.IsDBNull(8) ? "DEFAULT" : rd.GetString(8),
                EventInboxUrl = rd.IsDBNull(9) ? null : rd.GetString(9),
                IsActive = !rd.IsDBNull(10) && rd.GetBoolean(10),
                Notes = rd.IsDBNull(11) ? null : rd.GetString(11),
                CalEventName = rd.IsDBNull(12) ? null : rd.GetString(12),
                CalSlotInterval = rd.IsDBNull(13) ? null : rd.GetInt32(13)


            };
        }

        public async Task<string> GuardarCalcom(DoctorApiKeyCalcom cfg)
        {
            try
            {
                await using var cn = new NpgsqlConnection(con.CadenaSQL);
                await cn.OpenAsync();

                const string sql = @"
INSERT INTO public.doctoresapikeycalcom
(iddoctor, email, telefono, nombreyapellido, apikey,
 cal_username, cal_event_type_id, cal_public_url,
 event_inbox, event_inbox_url, is_active, notes, cal_event_name , cal_slot_interval , created_at, updated_at)
VALUES
(@iddoctor, @email, @telefono, @nombreyapellido, @apikey,
 @cal_username, @cal_event_type_id, @cal_public_url,
 @event_inbox, @event_inbox_url, @is_active, @notes, @cal_event_name ,@cal_slot_interval , now(), now())
ON CONFLICT (iddoctor)
DO UPDATE SET
 email = EXCLUDED.email,
 telefono = EXCLUDED.telefono,
 nombreyapellido = EXCLUDED.nombreyapellido,
 apikey = EXCLUDED.apikey,
 cal_username = EXCLUDED.cal_username,
 cal_event_type_id = EXCLUDED.cal_event_type_id,
 cal_public_url = EXCLUDED.cal_public_url,
 event_inbox = EXCLUDED.event_inbox,
 event_inbox_url = EXCLUDED.event_inbox_url,
 is_active = EXCLUDED.is_active,
 notes = EXCLUDED.notes,
cal_event_name = EXCLUDED.cal_event_name,
cal_slot_interval = EXCLUDED.cal_slot_interval,

 updated_at = now();
";
                await using var cmd = new NpgsqlCommand(sql, cn);

                cmd.Parameters.AddWithValue("@iddoctor", cfg.IdDoctor);
                cmd.Parameters.AddWithValue("@email", (object?)cfg.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@telefono", (object?)cfg.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombreyapellido", (object?)cfg.NombreYApellido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@apikey", cfg.ApiKey ?? "");
                cmd.Parameters.AddWithValue("@cal_username", (object?)cfg.CalUsername ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cal_event_type_id", (object?)cfg.CalEventTypeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cal_public_url", (object?)cfg.CalPublicUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@event_inbox", cfg.EventInbox ?? "DEFAULT");
                cmd.Parameters.AddWithValue("@event_inbox_url", (object?)cfg.EventInboxUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@is_active", cfg.IsActive);
                cmd.Parameters.AddWithValue("@notes", (object?)cfg.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cal_event_name", (object?)cfg.CalEventName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cal_slot_interval", (object?)cfg.CalSlotInterval ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                return "";
            }
            catch (Exception ex)
            {
                return "Error GuardarCalcom: " + ex.Message;
            }
        }
        public async Task<(bool ok, string msg, int idAccion)> EncolarSincroCalcom(int idDoctor, string mes, string anio)
        {
            try
            {
                int mesInicio = int.Parse(mes);
                int anioActual = int.Parse(anio);
                int totalInsertados = 0;
                int primerIdGenerado = 0;

                var registros = new List<(int mes, int anio)>();

                // Meses del año en curso desde el mes recibido hasta diciembre
                for (int m = mesInicio; m <= 12; m++)
                {
                    registros.Add((m, anioActual));
                }

                // Si el mes es 11 o 12, agregar los 12 meses del año siguiente
                if (mesInicio >= 11)
                {
                    int anioSiguiente = anioActual + 1;
                    for (int m = 1; m <= 12; m++)
                    {
                        registros.Add((m, anioSiguiente));
                    }
                }

                using (var conn = new NpgsqlConnection(con.CadenaSQL))
                {
                    await conn.OpenAsync();

                    string sql = @"INSERT INTO public.accionescomplementarias 
                (tipoaccion, iddoctor, mes, anio, repasado, reintentos, fecharegistro,
                 conexion, conexion_vps, pathexewin, pathexevps) 
                VALUES 
                ('sincroidcalcom', @idDoctor, @mes, @anio, 'N', 0, NOW(),
                 'Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9',
                 'Host=72.60.89.227;Port=5433;Database=dbclinica;Username=postgres;Password=W39xlpS9;Pooling=true;Maximum Pool Size=20;Minimum Pool Size=0;Timeout=30',
                 'C:\tmp\rios_rosas\c#\regresion\sincroidcalcom\bin\Release\net8.0\sincroidcalcom.exe',
                 '/us2/dbclinica/exe/sincroidcalcom'
                ) 
                RETURNING idaccionescomplementarias;";

                    foreach (var reg in registros)
                    {
                        var id = await conn.ExecuteScalarAsync<int>(sql, new
                        {
                            idDoctor,
                            mes = reg.mes.ToString(),
                            anio = reg.anio.ToString()
                        });

                        if (primerIdGenerado == 0)
                            primerIdGenerado = id;

                        totalInsertados++;
                    }
                }

                string mensaje = $"Sincronización encolada. Se crearon {totalInsertados} registros.";
                return (true, mensaje, primerIdGenerado);
            }
            catch (Exception ex)
            {
                return (false, "Error al encolar en BD: " + ex.Message, 0);
            }
        }
        public async Task<List<EspecialidadesDoctor>> ListarEspecialidadesPorDoctor(int idDoctor)
        {
            var lista = new List<EspecialidadesDoctor>();
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Consulta directa a la tabla intermedia con JOIN para traer el nombre
            string sql = @"SELECT ed.id_especialidad_doctor, ed.idespecialidad, e.nombre 
                   FROM public.especialidadesdoctor ed
                   JOIN public.especialidad e ON ed.idespecialidad = e.idespecialidad
                   WHERE ed.iddoctor = @id";

            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@id", idDoctor);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new EspecialidadesDoctor
                {
                    IdEspecialidadDoctor = Convert.ToInt32(dr["id_especialidad_doctor"]),
                    IdEspecialidad = Convert.ToInt32(dr["idespecialidad"]),
                    NombreEspecialidad = dr["nombre"].ToString() ?? ""
                });
            }
            return lista;
        }
        public async Task<string> AsignarEspecialidad(int idDoctor, int idEspecialidad)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();
                string sql = @"INSERT INTO public.especialidadesdoctor (iddoctor, idespecialidad) 
                       VALUES (@idD, @idE) ON CONFLICT DO NOTHING";
                await using var cmd = new NpgsqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@idD", idDoctor);
                cmd.Parameters.AddWithValue("@idE", idEspecialidad);
                await cmd.ExecuteNonQueryAsync();
                return "";
            }
            catch (Exception ex) { return ex.Message; }
        }

        public async Task<bool> EliminarEspecialidadDoctor(int idEspecialidadDoctor)
        {
            try
            {
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();
                string sql = "DELETE FROM public.especialidadesdoctor WHERE id_especialidad_doctor = @id";
                await using var cmd = new NpgsqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@id", idEspecialidadDoctor);
                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
            catch { return false; }
        }
        public async Task<List<Doctor>> ListarDoctoresPorEspecialidad(int idEspecialidad)
        {
            var lista = new List<Doctor>();
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // Traemos los campos que tu clase necesita para calcular el nombre
            string sql = @"SELECT d.iddoctor, d.nombres, d.apellidos
                  FROM public.doctor d
                  JOIN public.especialidadesdoctor ed ON d.iddoctor = ed.iddoctor
                  WHERE ed.idespecialidad = @id";

            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@id", idEspecialidad);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                // Al asignar Nombres y Apellidos, tu propiedad calculada NombreCompleto 
                // funcionará sola sin dar error de solo lectura
                lista.Add(new Doctor
                {
                    IdDoctor = Convert.ToInt32(dr["iddoctor"]),
                    Nombres = dr["nombres"].ToString() ?? "",
                    Apellidos = dr["apellidos"].ToString() ?? ""
                });
            }
            return lista;
        }
    }
}
