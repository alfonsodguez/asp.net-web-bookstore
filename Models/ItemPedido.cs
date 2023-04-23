using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models
{
    public class ItemPedido
    {
        public String IdPedido { get; set; }
        public Libro LibroPedido { get; set; } 
        public int CantidadLibro { get; set; }
    }
}
