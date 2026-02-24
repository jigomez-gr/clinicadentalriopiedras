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
                    "SELECT public.sp_editardoctor(" +
                    "@IdDoctor, @NumeroDocumentoIdentidad, @Nombres, @Apellidos, @Genero, @IdEspecialidad);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@IdDoctor", objeto.IdDoctor);
                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombres", objeto.Nombres);
                cmd.Parameters.AddWithValue("@Apellidos", objeto.Apellidos);
                cmd.Parameters.AddWithValue("@Genero", objeto.Genero);
                cmd.Parameters.AddWithValue("@IdEspecialidad", objeto.Especialidad.IdEspecialidad);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                return "Error al editar el doctor";
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
                    "SELECT public.sp_guardardoctor(" +
                    "@NumeroDocumentoIdentidad, @Nombres, @Apellidos, @Genero, @IdEspecialidad);",
                    conexion);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@NumeroDocumentoIdentidad", objeto.NumeroDocumentoIdentidad);
                cmd.Parameters.AddWithValue("@Nombres", objeto.Nombres);
                cmd.Parameters.AddWithValue("@Apellidos", objeto.Apellidos);
                cmd.Parameters.AddWithValue("@Genero", objeto.Genero);
                cmd.Parameters.AddWithValue("@IdEspecialidad", objeto.Especialidad.IdEspecialidad);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch
            {
                // (tu mensaje decía "editar", lo dejo corregido a "guardar")
                return "Error al guardar el doctor";
            }
        }

        public async Task<List<Doctor>> Lista()
        {
            var lista = new List<Doctor>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listadoctor();",
                conexion);

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
                    Especialidad = new Especialidad
                    {
                        IdEspecialidad = Convert.ToInt32(dr["idespecialidad"]),
                        Nombre = dr["nombreespecialidad"]?.ToString() ?? "",
                    },
                    FechaCreacion = dr["fechacreacion"]?.ToString() ?? ""
                });
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
                    MetodoPeticion = dr["MetodoPeticion"] == DBNull.Value ? null : dr["MetodoPeticion"]?.ToString()
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
                await using var conexion = new NpgsqlConnection(con.CadenaSQL);
                await conexion.OpenAsync();

                int mes = fecha.Month;

                // 1) Resolver DoctorHorario del doctor + mes
                int idDoctorHorario;

                // ✅ CAMBIO MÍNIMO: las horas de Postgres (time) vienen como TimeOnly
                TimeOnly? horaInicioAM = null, horaFinAM = null, horaInicioPM = null, horaFinPM = null;

                await using (var cmdH = new NpgsqlCommand(@"
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

                    await using var dr = await cmdH.ExecuteReaderAsync();
                    if (!await dr.ReadAsync())
                        return $"No existe DoctorHorario para el doctor {idDoctor} en el mes {mes} (NumeroMes).";

                    idDoctorHorario = Convert.ToInt32(dr["iddoctorhorario"]);

                    // ✅ CAMBIO MÍNIMO: cast a TimeOnly
                    if (dr["horainicioam"] != DBNull.Value) horaInicioAM = (TimeOnly)dr["horainicioam"];
                    if (dr["horafinam"] != DBNull.Value) horaFinAM = (TimeOnly)dr["horafinam"];
                    if (dr["horainiciopm"] != DBNull.Value) horaInicioPM = (TimeOnly)dr["horainiciopm"];
                    if (dr["horafinpm"] != DBNull.Value) horaFinPM = (TimeOnly)dr["horafinpm"];
                }

                // 2) Inferir Turno AM/PM usando los rangos del DoctorHorario
                // (Tu llamada ya era así; solo cambia tipos)
                string turno = InferirTurno(turnoHora, horaInicioAM, horaFinAM, horaInicioPM, horaFinPM);

                // 3) Evitar duplicados (mismo doctorhorario + fecha + hora)
                await using (var cmdChk = new NpgsqlCommand(@"
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
                        return "Ese slot ya existe para esa fecha.";
                }

                // 4) Insert
                await using (var cmdIns = new NpgsqlCommand(@"
                INSERT INTO public.doctorhorariodetalle
                (iddoctorhorario, fecha, turno, turnohora, reservado, fechacreacion)
                VALUES
                (@idh, @fecha, @turno, @hora, false, NOW());
            ", conexion))
                {
                    cmdIns.Parameters.AddWithValue("@idh", idDoctorHorario);
                    cmdIns.Parameters.AddWithValue("@fecha", fecha.Date);
                    cmdIns.Parameters.AddWithValue("@turno", turno);
                    cmdIns.Parameters.AddWithValue("@hora", turnoHora);

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
       event_inbox, event_inbox_url, is_active, notes
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
                Notes = rd.IsDBNull(11) ? null : rd.GetString(11)
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
 event_inbox, event_inbox_url, is_active, notes, created_at, updated_at)
VALUES
(@iddoctor, @email, @telefono, @nombreyapellido, @apikey,
 @cal_username, @cal_event_type_id, @cal_public_url,
 @event_inbox, @event_inbox_url, @is_active, @notes, now(), now())
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

                await cmd.ExecuteNonQueryAsync();
                return "";
            }
            catch (Exception ex)
            {
                return "Error GuardarCalcom: " + ex.Message;
            }
        }
        /* Comienzo Cambio CORREGIDO */
        public async Task<(bool ok, string msg, int idAccion)> EncolarSincroCalcom(int idDoctor, string mes, string anio)
        {
            try
            {
                using (var conn = new NpgsqlConnection(con.CadenaSQL))
                {
                    // Insertamos la petición incluyendo conexiones y rutas para el orquestador
                    string sql = @"INSERT INTO public.accionescomplementarias 
                   (tipoaccion, iddoctor, mes, anio, repasado, reintentos, fecharegistro,
                    conexion, conexion_vps, pathexewin, pathexevps) 
                   VALUES 
                   ('sincroidcalcom', @idDoctor, @mes, @anio, 'N', 0, NOW(),
                    'Host=localhost;Port=5432;Database=dbclinica;Username=postgres;Password=W39xlpS9',
                    'Host=72.60.89.227;Port=5433;Database=dbclinica;Username=postgres;Password=W39xlpS9;Pooling=true;Maximum Pool Size=20;Minimum Pool Size=0;Timeout=30',
                    'C:\tmp\rios_rosas\c#\regresion\sincroidcalcom\bin\Release\net8.0\sincroidcalcom.exe',
                    '/us2/dbclinica/exe/sincroidcalcomvps'
                    ) 
                   RETURNING idaccionescomplementarias;";

                    var id = await conn.ExecuteScalarAsync<int>(sql, new { idDoctor, mes, anio });

                    return (true, "Sincronización de Cal.com encolada correctamente.", id);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error al encolar en BD: " + ex.Message, 0);
            }
        }
        /* Final Cambio CORREGIDO */
    }
}
