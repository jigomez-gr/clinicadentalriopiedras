using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaEntidades;

namespace ClinicaData.Contrato
{
    public interface IAnalisisVapiRepositorio
    {
        Task<List<AnalisisVapi>> Lista();
        Task<(byte[]? bytes, string filename)> ObtenerAudio(int idLlamada);
    }
}

