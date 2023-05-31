using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models.Interfaces
{
    public interface IDBAccess  
    {
        Task<List<Municipio>> DevolverMunicipios(int codProvincia);
        Task<List<Provincia>> DevolverProvincias();
        Task<bool> RegistrarCliente(Cliente cliente);
        Task<bool> ActivarCuenta(String email);
        Task<Cliente> ComprobarCredenciales(String email, String password);
        Task<bool> ExisteEmailCliente(String email);
        Task<List<Materia>> DevolverMaterias(int? idmMateriaPadre);
        #nullable enable
        Task<List<Libro>?> DevolverLibros(int idMateria, string? isbn);
        Task<List<Libro>?> BuscarLibros(string? filtro, string? valor);
        #nullable disable
        Task<bool> GuardarPedido(Pedido pedido);
        Task<bool> ActualizarDatosCliente(Cliente cliente);
        Task<int> AltaNuevaDireccionCliente(Direccion direccion);
    }
}
