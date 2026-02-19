using System;

public static class ErrorHandler
{
    public static void RegistrarError(
        string conexionDB_Hotel1App,
        string tipoSolicitudID,
        string programa,
        string token,
        string aplicacion,
        Exception ex,
        string zmensajeerror,
        string uuid,
        string znumFas,
        string znumero,
        string zoperacion)
    {
        string mensajeErrorExcepcion = ex.Message;
        string mensajeDeDebug = ex.StackTrace ?? "Sin detalles de la pila de ejecución";

        DB_Hotel1DaErroresHandler errorHandler = new DB_Hotel1DaErroresHandler();

        errorHandler.Registraelerror(
            conexionDB_Hotel1App,
            tipoSolicitudID,
            Environment.MachineName,
            mensajeErrorExcepcion,
            mensajeDeDebug,
            programa,
            token,
            zmensajeerror,
            aplicacion,
            "KO",
            uuid,
            znumFas,
            znumero,
            zoperacion
        );
    }

    public static void ModificarError(
        string conexionDB_Hotel1App,
        string tipoSolicitudID,
        string programa,
        string token,
        string aplicacion,
        Exception ex,
        string zmensajeerror,
        string uuid,
        string znumFas,
        string znumero,
        string zoperacion)
    {
        string mensajeErrorExcepcion = ex.Message;
        string mensajeDeDebug = ex.StackTrace ?? "Sin detalles de la pila de ejecución";

        DB_Hotel1DaErroresHandler errorHandler = new DB_Hotel1DaErroresHandler();

        errorHandler.Registraelerror(
            conexionDB_Hotel1App,
            tipoSolicitudID,
            Environment.MachineName,
            mensajeErrorExcepcion,
            mensajeDeDebug,
            programa,
            token,
            zmensajeerror,
            aplicacion,
            "KO",
            uuid,
            znumFas,
            znumero,
            zoperacion
        );
    }
}
