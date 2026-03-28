using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEntidades
{
    public class ChequeoRequest
    {
        public int id_operacion { get; set; }
        public int numerooperacion { get; set; }
        public string valor_recibido { get; set; }
        public string workflowid { get; set; }
    }

}
