
using ClinicaEntidades;
using ClinicaEntidades.DTO;

namespace ClinicaData.Contrato
{
    public interface ICitaRepositorio
    {
        Task<string> Guardar(Cita objeto);
        Task<string> Cancelar(int Id);
        Task<List<Cita>> ListaCitasPendiente(int IdUsuario);
        Task<List<Cita>> ListaHistorialCitas(int IdUsuario);
        Task<string> CambiarEstado(int IdCita,int IdEstado,string Indicaciones);
        Task<string> ActualizarDocIndicacionesDoctor(int IdCita, byte[]? doc, string? contentTypeDoctor);
        Task<string> GuardarDocumentoDoctor(int idCita, byte[]? documento, string? contentTypeDoctor);
        Task<string> ActualizarDatosPaciente(int IdCita, string razonCitaUsr, byte[]? documento, string? contentType);

        // 👉 NUEVOS MÉTODOS
        Task<string> GuardarMotivoPaciente(Cita objeto);
        Task<string> GuardarDocumentoPaciente(Cita objeto);

        // NUEVO: actualizar motivo y documento del paciente

        // 🔹 NUEVO: actualizar motivo + documento del paciente
        Task<List<Cita>> ListaCitasGestion();
        Task<string> AdminActualizarCita(Cita objeto);
        Task ActualizarCitaConfirmacionAdmin(int idCita, string? citaConfirmada);
        Task<string> ActualizarMotivoPaciente(
            int idCita,
            string razonCitaUsr,
            byte[]? documento,
            string? contentType
        );
        // ... lo que ya tengas ...

        Task<List<Cita>> ListaCitasAdmin(int idEstadoCita);
        

        // 🔹 NUEVOS PARA GESTIÓN ADMIN
        Task<List<Cita>> ListaCitasGestion(int IdEstadoCita);
      
        // Task<string> AdminActualizarCita(Cita objeto);
        // NUEVO: obtener detalle de cita (confirmación/peticiones) por IdCita
        Task<CitaDetalleDTO?> ObtenerDetalleCita(int idCita);
        // =====================
        // CONTRATO (ICitaRepositorio)
        // =====================
        // Añade esto en la interfaz (sin out)
        Task<(List<Cita> Lista, int TotalRegistros)> ListaCitasGestionServerSide(
            int idEstadoCita,
            int start,
            int length,
            string filtro

        );
        
        Task<List<DoctorHorarioDetalle>> ObtenerSlotsLibres(int idDoctor, string fecha);

        // El método de encolar ya lo tenemos, pero asegúrate de que acepte el idAccion
        Task<(bool ok, string msg, int idAccion)> EncolarReprogramacion(int idCitaVieja, string nuevaFecha, string nuevaHora, string motivo, string documentoEjecutor);
        /* comienzo cambio */
        Task<(bool ok, string msg, int idAccion)> EncolarCancelacion(int idCita,  string motivo, string documentoEjecutor);
        /* comienzo cambio */
        Task<int> ObtenerIdDoctorDeCita(int idCita);

        /* final cambio */
        // Método para ejecutar el cambio de estado (lanza el SP)
        Task<string> CambiarEstadoCitaBD(int idCita, int nuevoEstado);

        // Método para obtener el ID de la acción creada por fn_cacancel_insert
        // Esto es vital para que el Polling de JS sepa qué rastrear
        Task<int> ObtenerUltimaAccionCancelacion(int idCita);
      
        // Consulta el estado ('S', 'P', 'E') de una acción específica
        Task<string> ConsultarEstadoAccion(int idAccion);
        // Añade esto a tu interfaz ICitasRepository
        Task<(bool ok, int idAccion)> CancelarDesdeAgenda(int idCita, string motivo);
        /* comienzo cambio sincro */
        // Encola la petición de chequeo en accionescomplementarias
        Task<(bool ok, int idAccion)> EncolarChequeoCita(int idCita, string refresh);

        // Obtiene el estado de la acción para el polling (S, P, E)
        Task<string> ConsultarEstadoAccionSincro(int idAccion);

        // Obtiene los datos del último resultado en doctorhorariodetallecalcom
        Task<(bool ok, dynamic data)> ObtenerResultadoSincroDetalle(int idCita);
        /* fin cambio sincro */
        // Obtiene la metadata de Cal.com vinculada a un slot
        Task<DoctorHorarioDetalleCalcom> ObtenerDetalleCalcom(int idDoctorHorarioDetalle);

        // Guarda o actualiza los datos extendidos (Blindado)
        Task<bool> GuardarDetalleCalcom(DoctorHorarioDetalleCalcom modelo);
    }
}
