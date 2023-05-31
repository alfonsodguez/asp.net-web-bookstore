using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models.Interfaces
{
    public interface IClienteEmail
    {
        #nullable enable
        void EnviarEmail(String destinatario, String asunto, String cuerpo, String? nombreAdjunto);
        #nullable disable
    }
}
