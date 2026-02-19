using ClinicaEntidades;
using ClinicaEntidades.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaData.Contrato
{
    public interface IDoctorRepositorio
    {
        Task<List<Doctor>> Lista();
        Task<string> Guardar(Doctor objeto);
        Task<string> Editar(Doctor objeto);
        Task<int> Eliminar(int Id);

        Task<string> RegistrarHorario(DoctorHorario objeto);
        Task<List<DoctorHorario>> ListaDoctorHorario();
        Task<string> EliminarHorario(int Id);

        // EXISTENTE (deja esto tal cual)
        Task<List<FechaAtencionDTO>> ListaDoctorHorarioDetalle(int Id);

        // NUEVO: incluye slots aunque tengan cita (usa sp_listaDoctorHorarioDetalleConCitas)
        Task<List<FechaAtencionDTO>> ListaDoctorHorarioDetalleConCitas(int Id);

        Task<List<Cita>> ListaCitasAsignadas(int Id, int IdEstadoCita);
        // Nuevo server-side (tupla):
        Task<(List<Cita> Lista, int TotalRegistros)> ListaCitasAsignadasServerSide(
            int idDoctor,
            int idEstadoCita,
            int start,
            int length,
            string filtro
        );
        Task<Doctor?> ObtenerPorNumeroDocumentoIdentidad(string numeroDocumentoIdentidad);
        Task<string> EliminarSlot(int idDoctorHorarioDetalle);
        // nuevo:
        Task<string> AgregarSlot(int idDoctor, DateTime fecha, TimeOnly turnoHora);
        Task<DoctorApiKeyCalcom?> ObtenerCalcom(int idDoctor);
        Task<string> GuardarCalcom(DoctorApiKeyCalcom cfg); // upsert
        Task<(bool ok, string msg, int idAccion)> EncolarSincroCalcom(int idDoctor, string mes, string anio);
     
    }
}
