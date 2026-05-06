using ClinicaEntidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicaData.Contrato
{
    public interface IMenuRepositorio
    {
        // Trae la lista de menús filtrada por el rol (ej: paciente = 3)
        Task<List<TgMenu>> Lista(int IdRolUsuario);
    }
}