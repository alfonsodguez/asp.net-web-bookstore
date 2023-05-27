using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models.Interfaces
{
    public interface IControlSession
    {
        void ActualizarSession<T>(String clave, T valor);
        T RecuperarSession<T>(String clave);
    }
}
