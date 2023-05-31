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
        private IControlSession _servicioSession;

        public PedidoController(IDBAccess servicioDBInyect, IClienteEmail servicioEmailInyect, IControlSession servicioSessionInyect)
        {
            this._conexionDB = servicioDBInyect;
            this._servicioEnvioEmail = servicioEmailInyect;
            this._servicioSession = servicioSessionInyect;
        }


        [HttpGet]
        public async Task<IActionResult> AddLibroPedido(String isbn) 
        {
            try
            {
                Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")

                int index = cliente.PedidoActual.ListaPedido.FindIndex(itemPedido => itemPedido.LibroPedido.ISBN == isbn);              
                if(index != -1)
                {
                    cliente.PedidoActual.ListaPedido[index].CantidadLibro += 1;
                }
                else
                {
                    Libro libro = (await this._conexionDB.BuscarLibros("ISBN", isbn)).Single<Libro>();

                    cliente.PedidoActual.ListaPedido.Add(new ItemPedido {
                        IdPedido = cliente.NIF + DateTime.Now.ToString(),
                        LibroPedido = libro,
                        CantidadLibro = 1                        
                    });
                }
                this._servicioSession.ActualizarSession<Cliente>("datoscliente", cliente);
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
                Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
                return View(cliente.PedidoActual);  
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Cliente");
            }      
        }

        [HttpGet]
        public IActionResult EliminarLibroPedido(String isbn)  
        {
            Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
            int index = cliente.PedidoActual.ListaPedido.FindIndex(itemPedido => itemPedido.LibroPedido.ISBN == isbn);
            cliente.PedidoActual.ListaPedido.RemoveAt(index);
         
            this._servicioSession.ActualizarSession<Cliente>("datoscliente", cliente);

            int elementosEnPedido = cliente.PedidoActual.ListaPedido.Count() 
            if (elementosEnPedido > 0) {
                return RedirectToAction("MostrarPedido");
            }
            return RedirectToAction("Libros", "Tienda");                  
        }

        [HttpGet] 
        public IActionResult RestarCantidad(String isbn)
        {
            Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
            ItemPedido itemLibro = cliente.PedidoActual.ListaPedido.Find((ItemPedido itemPedido) => itemPedido.LibroPedido.ISBN == isbn);

            if (itemLibro.CantidadLibro > 1)
            {
                itemLibro.CantidadLibro -= 1;

                this._servicioSession.ActualizarSession<Cliente>("datoscliente", cliente);
                return RedirectToAction("MostrarPedido");
            }
            cliente.PedidoActual.ListaPedido.Remove(itemLibro);
            this._servicioSession.ActualizarSession<Cliente>("datoscliente", cliente);

            int elementosEnPedido = cliente.PedidoActual.ListaPedido.Count() 
            if(elementosEnPedido < 1)
            {
                return RedirectToAction("Libros", "Tienda");
            }
            return RedirectToAction("MostrarPedido", "Pedido");
        }

        [HttpGet] 
        public IActionResult SumarCantidad(String isbn)
        {
            Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
            cliente
                .PedidoActual
                .ListaPedido
                .Where<ItemPedido>(itemPedido => itemPedido.LibroPedido.ISBN == isbn)
                .Single<ItemPedido>().CantidadLibro += 1;

            this._servicioSession.ActualizarSession<Cliente>("datoscliente", cliente);
            return RedirectToAction("MostrarPedido", "Pedido");
        }

        [HttpGet]
        public async Task<IActionResult> FinalizarPedido()
        {
            Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
            cliente.PedidoActual.NIF = cliente.NIF;
            cliente.PedidoActual.FechaPedido = DateTime.Now;
            cliente.PedidoActual.EstadoPedido = "pendiente";

            bool pedidoGuardadoOk = await this._conexionDB.GuardarPedido(cliente.PedidoActual)
            if (pedidoGuardadoOk)
            {
                int idFactura = cliente.PedidoActual.IdPedido
                String factura = _GenerarFactura(cliente, idFactura)
                _EnviarFacturaPorEmail(cliente, factura, idFactura)

                return View();
            }
            TempData["ErrorServer"] = "Error interno del server al procesar pedido, intentelo de nuevo mas tarde";
            return RedirectToAction("MostrarPedido");
        }


        private String _GenerarFactura(Cliente cliente, int idfactura)
        {
            ChromePdfRenderer renderPDF = new ChromePdfRenderer();
            String tagsItemsPedido = "<tr>";

            cliente
                .PedidoActual
                .ListaPedido
                .ForEach((ItemPedido item) => {
                    decimal subtotal = item.LibroPedido.Precio * item.CantidadLibro;
                    tagsItemsPedido += $@"
                        <td>{item.LibroPedido.Titulo}</td>
                        <td>{item.LibroPedido.Precio} €</td>
                        <td>{item.CantidadLibro.ToString()}</td>
                        <td>{subtotal} €</td>
                        </tr>
                        <tr>";
                });
            tagsItemsPedido += "</tr>";

            String facturaHTML = $@"
                <div>
                    <h3><strong>RESUMEN DE TU PEDIDO con ID: {cliente.PedidoActual.IdPedido}</strong></h3>
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
                        {tagsItemsPedido}
                    </table>
                    <hr/>
                </div>
                <div>
                    <p><strong>SubTotal Pedido: {cliente.PedidoActual.SubTotalPedido} €</strong></p>
                    <p> Gastos de Envio: { cliente.PedidoActual.GastosDeEnvio} € </p>
                    <p><h3><strong>Total Pedido: {cliente.PedidoActual.TotalPedido} €</strong></h3></p>
                </div>";

            renderPDF.RenderHtmlAsPdf(facturaHTML).SaveAs("factura-" + idfactura + ".pdf");
            return facturaHTML
        }

        private void _EnviarFacturaPorEmail(Cliente cliente, String factura, int idFactura)
        {
            String destinatario = cliente.CredencialesCliente.Email
            String asunto = "Pedido realizado correctamente en Agapea.com"
            String adjunto = "factura-" + idFactura + ".pdf"
            String body = factura
            this._servicioEnvioEmail.EnviarEmail(destinatario, asunto, body, adjunto);
        }
    }
}
