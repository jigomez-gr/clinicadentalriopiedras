using ClinicaData.Configuracion;
using ClinicaData.Contrato;
using ClinicaEntidades;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace ClinicaData.Repositorio
{
    public class ZadarmaSmsRespuestaRepositorio : IZadarmaSmsRespuestaRepositorio
    {
        //private readonly Conexion con = new Conexion();
        private readonly ConnectionStrings con;

        public ZadarmaSmsRespuestaRepositorio (IOptions<ConnectionStrings> options)
        {
            con = options.Value;
        }
        public async Task<List<ZadarmaSmsRespuesta>> Lista()
        {
            var lista = new List<ZadarmaSmsRespuesta>();

            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            const string sql = @"
SELECT
    idrespuestasms,
    fecharegistro,
    httpstatuscode,
    status,
    messages,
    costtotal,
    currency,
    callerid,
    numerodestino,
    cost,
    costmin,
    costmax,
    mensaje,
    parts,
    deniednumbers,
    rawjsonrespuesta
FROM public.zadarmasmsrespuesta
ORDER BY idrespuestasms DESC;
";

            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.CommandType = CommandType.Text;

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var item = new ZadarmaSmsRespuesta
                {
                    IdRespuestaSms = dr["idrespuestasms"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idrespuestasms"]),
                    FechaRegistro = dr["fecharegistro"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["fecharegistro"]),

                    HttpStatusCode = dr["httpstatuscode"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["httpstatuscode"]),
                    Status = dr["status"] == DBNull.Value ? "" : dr["status"].ToString()!,
                    Messages = dr["messages"] == DBNull.Value ? 0 : Convert.ToInt32(dr["messages"]),
                    CostTotal = dr["costtotal"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["costtotal"]),
                    Currency = dr["currency"] == DBNull.Value ? "" : dr["currency"].ToString()!,

                    CallerId = dr["callerid"] == DBNull.Value ? null : dr["callerid"].ToString(),
                    NumeroDestino = dr["numerodestino"] == DBNull.Value ? "" : dr["numerodestino"].ToString()!,

                    Cost = dr["cost"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["cost"]),
                    CostMin = dr["costmin"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["costmin"]),
                    CostMax = dr["costmax"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["costmax"]),

                    Mensaje = dr["mensaje"] == DBNull.Value ? "" : dr["mensaje"].ToString()!,
                    Parts = dr["parts"] == DBNull.Value ? 0 : Convert.ToInt32(dr["parts"]),

                    DeniedNumbers = dr["deniednumbers"] == DBNull.Value ? null : dr["deniednumbers"].ToString(),
                    RawJsonRespuesta = dr["rawjsonrespuesta"] == DBNull.Value ? null : dr["rawjsonrespuesta"].ToString()
                };

                lista.Add(item);
            }

            return lista;
        }

        public async Task<(string? json, string filename)> ObtenerRawJson(int idRespuestaSms)
        {
            await using var conexion = new NpgsqlConnection(con.CadenaSQL);
            await conexion.OpenAsync();

            const string sql = @"
SELECT rawjsonrespuesta
FROM public.zadarmasmsrespuesta
WHERE idrespuestasms = @id;
";

            await using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@id", idRespuestaSms);

            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj == DBNull.Value)
                return (null, $"zadarma_sms_{idRespuestaSms}.json");

            return (obj.ToString(), $"zadarma_sms_{idRespuestaSms}.json");
        }

    }
}
