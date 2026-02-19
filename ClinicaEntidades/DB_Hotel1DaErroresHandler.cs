using Npgsql;
using System;
using Npgsql; // <-- NuGet: Microsoft.Data.SqlClient

public class DB_Hotel1DaErroresHandler
{
    // Propiedades que reflejan la estructura de la tabla DB_Hotel1DaErrores
    public string? TipoSolicitud { get; set; }
    public DateTime FechaYHora { get; set; } = DateTime.Now;
    public string? Programa { get; set; }
    public string? Token { get; set; }
    public string? MensajeError { get; set; }
    public string? MensajeExcepcion { get; set; }
    public string? MensajeErrorExcepcion { get; set; }
    public int Identifi { get; set; }
    public string? Estado { get; set; }
    public int? Id { get; set; }
    public int? Numero { get; set; }
    public int? Fase { get; set; }
    public int? Operacion { get; set; }
    public string? Maquina { get; set; }
    public string? MensajeDeDebug { get; set; }
    public string? Status { get; set; }

    // Método para registrar el error en la base de datos
    public void Registraelerror(
        string conexionDB_Hotel1App,
        string tipoSolicitudID,
        string maquina,
        string mensajeErrorExcepcion,
        string mensajeDeDebug,
        string programa,
        string token,
        string zmensajeerror,
        string aplicacion,
        string estado,
        string uuid,
        string znumFas,
        string znumero,
        string zoperacion
    )
    {
        // Si quieres usar la máquina real, ignora el parámetro "maquina"
        maquina = Environment.MachineName.ToUpperInvariant().Trim();

        // Evitar NullReference si vienen nulos
        mensajeDeDebug = (mensajeDeDebug ?? string.Empty).Replace("'", "").Replace("\"", "");
        mensajeErrorExcepcion = (mensajeErrorExcepcion ?? string.Empty).Replace("'", "").Replace("\"", "");
        zmensajeerror = (zmensajeerror ?? string.Empty).Replace("'", "").Replace("\"", "");

        estado = "KO";

        const string xsql =
            "INSERT INTO DB_Hotel1DaErrores " +
            "(MENSAJEDEDEBUG, MAQUINA, tiposolicitud, programa, token, aplicacion, mensajeerror, mensajeerrorexcepcion, status, id, numero, fase, operacion) " +
            "VALUES (@MensajeDeDebug, @Maquina, @TipoSolicitudID, @Programa, @Token, @Aplicacion, @MensajeError, @MensajeErrorExcepcion, @Estado, @UUID, @ZNumero, @ZNumFas, @ZOperacion)";

        try
        {
            using var con = new NpgsqlConnection(conexionDB_Hotel1App);
            con.Open();

            using var cmd = new NpgsqlCommand(xsql, con);

            cmd.Parameters.AddWithValue("@MensajeDeDebug", mensajeDeDebug);
            cmd.Parameters.AddWithValue("@Maquina", maquina);
            cmd.Parameters.AddWithValue("@TipoSolicitudID", tipoSolicitudID ?? string.Empty);
            cmd.Parameters.AddWithValue("@Programa", programa ?? string.Empty);
            cmd.Parameters.AddWithValue("@Token", token ?? string.Empty);
            cmd.Parameters.AddWithValue("@Aplicacion", aplicacion ?? string.Empty);
            cmd.Parameters.AddWithValue("@MensajeError", zmensajeerror);
            cmd.Parameters.AddWithValue("@MensajeErrorExcepcion", mensajeErrorExcepcion);
            cmd.Parameters.AddWithValue("@Estado", estado);
            cmd.Parameters.AddWithValue("@UUID", uuid ?? string.Empty);

            // OJO: id/numero/fase/operacion en tu tabla parecen numéricos.
            // Aquí tus parámetros llegan como string. Si no puedes cambiar firma, al menos manda DBNull cuando esté vacío.
            cmd.Parameters.AddWithValue("@ZNumero", string.IsNullOrWhiteSpace(znumero) ? (object)DBNull.Value : znumero);
            cmd.Parameters.AddWithValue("@ZNumFas", string.IsNullOrWhiteSpace(znumFas) ? (object)DBNull.Value : znumFas);
            cmd.Parameters.AddWithValue("@ZOperacion", string.IsNullOrWhiteSpace(zoperacion) ? (object)DBNull.Value : zoperacion);

            cmd.ExecuteNonQuery();
        }
        catch (Exception)
        {
            // Aquí puedes loguear si quieres (pero no relances si no te interesa)
        }
    }
}
