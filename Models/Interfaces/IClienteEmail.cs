using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models.Interfaces
{
    public interface IClienteEmail
    {
        #nullable enable
        void EnviarEmail(String ToEmailCliente, String Subject, String Body, String? nombreAdjunto);
        #nullable disable
    }
}
