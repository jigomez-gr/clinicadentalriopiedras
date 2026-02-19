using ClinicaEntidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClinicaData.Contrato
{
        public interface IZadarmaSmsRespuestaRepositorio
        {
            Task<List<ZadarmaSmsRespuesta>> Lista();
            Task<(string? json, string filename)> ObtenerRawJson(int idRespuestaSms);
        }
   
}
