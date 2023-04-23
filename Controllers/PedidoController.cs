using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bookstore.Models.Interfaces;
using bookstore.Models;

//serializar y desarializar json
using System.Text.Json;
using Microsoft.AspNetCore.Http;

//generar pdf
using IronPdf;


namespace bookstore.Controllers
{
    public class PedidoController : Controller
    {
        private IDBAccess _conexionDB;
        private IClienteEmail _servicioEnvioEmail;

        public PedidoController(IDBAccess servicioDBInyect, IClienteEmail servicioEmailInyect)
        {
            this._conexionDB = servicioDBInyect;
            this._servicioEnvioEmail = servicioEmailInyect;
        }


        [HttpGet]
        public async Task<IActionResult> AddLibroPedido(String id) 
        {
            try
            {
                Cliente _clienteLogin = this.DevuelveClienteSession();

                int index = _clienteLogin.PedidoActual.ListaPedido.FindIndex(item => item.LibroPedido.ISBN == id);              
                if(index != -1)
                {
                    _clienteLogin.PedidoActual.ListaPedido[index].CantidadLibro += 1;
                }
                else
                {
                    Libro _libroPedidoAdd = (await this._conexionDB.BuscarLibros("ISBN", id)).Single<Libro>();

                    _clienteLogin.PedidoActual.ListaPedido.Add(new ItemPedido
                    {
                        IdPedido = _clienteLogin.NIF + DateTime.Now.ToString(),
                        LibroPedido = _libroPedidoAdd,
                        CantidadLibro = 1                        
                    });
                }

                this.ActualizaEstadoSession(_clienteLogin);

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Cliente");
            }  
        }

        [HttpGet]
        public IActionResult MostrarPedido()
        {
            try
            {
                TempData["ErrorServer"] = ""; 

                Cliente _cliente = this.DevuelveClienteSession();
                return View(_cliente.PedidoActual);  
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Cliente");
            }      
        }

        [HttpGet]
        public IActionResult EliminarLibroPedido(String id)  
        {
            Cliente _cliente = this.DevuelveClienteSession();

            int _index = _cliente.PedidoActual.ListaPedido.FindIndex(item => item.LibroPedido.ISBN == id);
            _cliente.PedidoActual.ListaPedido.RemoveAt(_index);
         
            this.ActualizaEstadoSession(_cliente);

            if (_cliente.PedidoActual.ListaPedido.Count() > 0) {
                return RedirectToAction("MostrarPedido");
            }
            else
            {
                return RedirectToAction("Libros", "Tienda");
            }                       
        }

        [HttpGet] 
        public IActionResult RestarCantidad(String isbn)
        {
            Cliente _cliente = this.DevuelveClienteSession();

            ItemPedido _itemLibro = _cliente
                .PedidoActual
                .ListaPedido
                .Find((ItemPedido item) => item.LibroPedido.ISBN == isbn);

            if (_itemLibro.CantidadLibro > 1)
            {
                _itemLibro.CantidadLibro -= 1;

                this.ActualizaEstadoSession(_cliente);
                return RedirectToAction("MostrarPedido");
            }
            else
            {
                _cliente.PedidoActual.ListaPedido.Remove(_itemLibro);

                this.ActualizaEstadoSession(_cliente);

                if(_cliente.PedidoActual.ListaPedido.Count() < 1)
                {
                    return RedirectToAction("Libros", "Tienda");
                }
                else
                {
                    return RedirectToAction("MostrarPedido", "Pedido");
                }
            } 
        }

        [HttpGet] 
        public IActionResult SumarCantidad(String isbn)
        {
            Cliente _cliente = this.DevuelveClienteSession();

            _cliente.PedidoActual.ListaPedido
                .Where<ItemPedido>(item => item.LibroPedido.ISBN == isbn)
                .Single<ItemPedido>().CantidadLibro += 1;

            this.ActualizaEstadoSession(_cliente);
            return RedirectToAction("MostrarPedido", "Pedido");
        }

        [HttpGet]
        public async Task<IActionResult> FinalizarPedido()
        {
            Cliente _cliente = this.DevuelveClienteSession();
            _cliente.PedidoActual.NIF = _cliente.NIF;
            _cliente.PedidoActual.FechaPedido = DateTime.Now;
            _cliente.PedidoActual.EstadoPedido = "pendiente";

            if (await this._conexionDB.GuardarPedido(_cliente.PedidoActual))
            {
                ChromePdfRenderer _renderPDF = new ChromePdfRenderer();

                String _itemspedidoHTML = "<tr>";

                _cliente.PedidoActual.ListaPedido
                    .ForEach((ItemPedido item)=> {
                        decimal _subtotal = item.LibroPedido.Precio * item.CantidadLibro;

                        _itemspedidoHTML += $@"
                            <td>{item.LibroPedido.Titulo}</td>
                            <td>{item.LibroPedido.Precio} €</td>
                            <td>{item.CantidadLibro.ToString()}</td>
                            <td>{_subtotal} €</td>
                            </tr>
                            <tr> 
                            ";
                    });

                _itemspedidoHTML += "</tr>";

                String _facturaHTML = $@"
                        <div>
                            <h3><strong>RESUMEN DE TU PEDIDO con ID: {_cliente.PedidoActual.IdPedido}</strong></h3>
                            <hr/>
                        </div>
                        <div>
                            <p>Gracias por comprar en nuestra tienda. A continuacion le pasamos un desglose de su pedido: </p>
                            <table>
                                <tr>
                                    <td>Titulo Libro</td>
                                    <td>Precio Libro</td>
                                    <td>Cantidad Libro</td>
                                    <td>Subtotal Libro</td>
                                </tr>
                                {_itemspedidoHTML}
                            </table>
                            <hr/>
                        </div>
                        <div>
                            <p><strong>SubTotal Pedido: {_cliente.PedidoActual.SubTotalPedido} €</strong></p>
                            <p> Gastos de Envio: { _cliente.PedidoActual.GastosDeEnvio} € </p>
                            <p><h3><strong>Total Pedido: {_cliente.PedidoActual.TotalPedido} €</strong></h3></p>
                        </div>            
                 ";

                _renderPDF.RenderHtmlAsPdf(_facturaHTML).SaveAs("factura-" + _cliente.PedidoActual.IdPedido + ".pdf");

                this._servicioEnvioEmail.EnviarEmail(_cliente.CredencialesCliente.Email, "Pedido realizado correctamente en Agapea.com", _facturaHTML, "factura-" + _cliente.PedidoActual.IdPedido + ".pdf");
                return View();
            }
            else
            {
                TempData["ErrorServer"] = "Error interno del server al procesar pedido, intentelo de nuevo mas tarde";
                return RedirectToAction("MostrarPedido");
            }
        }


        private Cliente DevuelveClienteSession()
        {
            try
            {
                Cliente _clienteLogin = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));
                return _clienteLogin;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void ActualizaEstadoSession(Cliente cliente)
        {
            HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize(cliente));
        }

    }
}
