using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models.Interfaces
{
    public interface IDBAccess  
    {
        #region //REGISTRO//
        Task<List<Municipio>> DevolverMunicipios(int codpro);
        Task<List<Provincia>> DevolverProvincias();
        Task<bool> RegistrarCliente(Cliente nuevoCliente);
        #endregion

        #region //ACTIVAR CUENTA//
        Task<bool> ActivarCuenta(String email);
        #endregion

        #region //LOGIN//
        Task<Cliente> ComprobarCredenciales(String email, String password);
        Task<bool> ExisteEmailCliente(String email);
        #endregion

        #region //TIENDA//
        Task<List<Materia>> DevolverMaterias(int? idmateriapadre);

#nullable enable
        Task<List<Libro>?> DevolverLibros(int idmateria, string? isbn);

        Task<List<Libro>?> BuscarLibros(string? filtro, string? valor);
#nullable disable
        #endregion

        #region //PEDIDO//
        Task<bool> GuardarPedido(Pedido pedidoActual);
        #endregion

        #region //PANEL DEL CLIENTE//
        Task<bool> ActualizarDatosCliente(Cliente clientemodif);
        Task<int> AltaNuevaDireccionCliente(Direccion nuevaDirec);
        #endregion
    }
}
