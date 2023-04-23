using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bookstore.Models;

namespace bookstore.Models
{
    public class Pedido
    {
        public List<ItemPedido> ListaPedido { get; set; }  
        public String IdPedido { get; set; }               
        public String NIF { get; set; }
        public String EstadoPedido { get; set; }
        public DateTime FechaPedido { get; set; }       
        public decimal GastosDeEnvio { get; set; }
        public decimal SubTotalPedido { get => CalculoSubTotal(); }
        public decimal TotalPedido { get => SubTotalPedido + GastosDeEnvio; }

        public Pedido()
        {
            this.ListaPedido = new List<ItemPedido>();
            this.IdPedido = System.Guid.NewGuid().ToString();  
            this.EstadoPedido = "en curso";
            this.FechaPedido = DateTime.Now;
            this.GastosDeEnvio = (decimal) 5.05; 
        }

        private decimal CalculoSubTotal()
        {
            decimal coste = 0;
            foreach (ItemPedido item in ListaPedido)
            {
                coste += item.CantidadLibro * item.LibroPedido.Precio;
            }
            return coste;
        }
    }
}
