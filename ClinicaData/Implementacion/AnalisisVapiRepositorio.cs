using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaData.Implementacion;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using NpgsqlTypes;
namespace ClinicaData.Implementacion
{
    public class AnalisisVapiRepositorio : IAnalisisVapiRepositorio
    {
        private readonly ConnectionStrings con;

        public AnalisisVapiRepositorio(IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }


        public async Task<List<AnalisisVapi>> Lista()
        {
            var lista = new List<AnalisisVapi>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            // IMPORTANTE: en PostgreSQL se llama así a una FUNCTION
            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM public.sp_listaanalisisvapi();",
                conexion
            );
            cmd.CommandType = CommandType.Text;

            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new AnalisisVapi
                {
                    IdLlamada = dr.GetInt32(dr.GetOrdinal("IdLlamada")),

                    Title = dr.IsDBNull(dr.GetOrdinal("Title"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("Title")),

                    Url = dr.IsDBNull(dr.GetOrdinal("Url"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("Url")),

                    IdAsistente = dr.IsDBNull(dr.GetOrdinal("IdAsistente"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("IdAsistente")),

                    NombreAsistente = dr.IsDBNull(dr.GetOrdinal("NombreAsistente"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("NombreAsistente")),

                    Coste = dr.IsDBNull(dr.GetOrdinal("Coste"))
                        ? null
                        : dr.GetDecimal(dr.GetOrdinal("Coste")),

                    FechaInicio = dr.IsDBNull(dr.GetOrdinal("FechaInicio"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("FechaInicio")),

                    FechaFin = dr.IsDBNull(dr.GetOrdinal("FechaFin"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("FechaFin")),

                    DuracionMinutos = dr.IsDBNull(dr.GetOrdinal("DuracionMinutos"))
                        ? null
                        : dr.GetDecimal(dr.GetOrdinal("DuracionMinutos")),

                    Transcripcion = dr.IsDBNull(dr.GetOrdinal("Transcripcion"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("Transcripcion")),

                    SugerenciaMejora = dr.IsDBNull(dr.GetOrdinal("SugerenciaMejora"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("SugerenciaMejora")),

                    Validacion = dr.IsDBNull(dr.GetOrdinal("Validacion"))
                        ? null
                        : dr.GetInt32(dr.GetOrdinal("Validacion")),

                    Resumen = dr.IsDBNull(dr.GetOrdinal("Resumen"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("Resumen")),

                    Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("Telefono")),

                    TieneAudio =
                        !dr.IsDBNull(dr.GetOrdinal("TieneAudio")) &&
                        dr.GetInt32(dr.GetOrdinal("TieneAudio")) == 1,

                    EmailCompleto = dr.IsDBNull(dr.GetOrdinal("EmailCompleto"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("EmailCompleto")),

                    NombreCompleto = dr.IsDBNull(dr.GetOrdinal("NombreCompleto"))
                        ? null
                        : dr.GetString(dr.GetOrdinal("NombreCompleto")),
                });
            }

            return lista;
        }

        public async Task<(byte[]? bytes, string filename)> ObtenerAudio(int idLlamada)
        {
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
        SELECT audio
        FROM public.analisis_vapi
        WHERE id_llamada = @id
        LIMIT 1;
    ", conexion);

            cmd.Parameters.AddWithValue("@id", idLlamada);

            var obj = await cmd.ExecuteScalarAsync();

            var filename = $"audio_llamada_{idLlamada}.wav";

            if (obj == null || obj == DBNull.Value)
                return (null, filename);

            // bytea -> byte[]
            return ((byte[])obj, filename);
        }



    }
}
