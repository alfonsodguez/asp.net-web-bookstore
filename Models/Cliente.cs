using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//importamos 
using System.ComponentModel.DataAnnotations;

namespace bookstore.Models
{
    public class Cliente
    {        
        public Credenciales CredencialesCliente { get; set; }

        [Required(ErrorMessage = "* Nombre obligatorio")]
        [MaxLength(50, ErrorMessage = "El nombre no puede exceder de 50 caracteres")]
        public String Nombre { get; set; }
        
        [Required(ErrorMessage = "* Apellidos obligatorios")]
        [MaxLength(100, ErrorMessage ="Los apellidos no pueden exceder de 100 caracteres")]
        public String Apellidos { get; set; }

        [Required(ErrorMessage = "* NIF obligatorio")]
        [RegularExpression("^[0-9]{7}-?[a-zA-Z]$", ErrorMessage = "Formato de NIF invalido: 1234567-A")]
        public String NIF { get; set; }

        [Required(ErrorMessage = "* Telefono obligatorio")]
        [RegularExpression("^[0-9]{3} [0-9]{2} [0-9]{2} [0-9]{2}$", ErrorMessage = "Formato de tlfno invalido: 000 11 22 33")]
        public String Telefono { get; set; }

        public bool? CuentaActivada { get; set; }
        #nullable enable
        public String? Descripcion { get; set; } 
        public String? ImagenAvatar { get; set; }
        #nullable disable

        public List<Direccion> ListaDirecciones { get; set; } 
        public List<Pedido> HistoricoPedidos { get; set; }       
        public Pedido PedidoActual { get; set; }              


        public Cliente()
        {
            this.CredencialesCliente = new Credenciales();
            this.ListaDirecciones = new List<Direccion>();
            this.HistoricoPedidos = new List<Pedido>();
            this.PedidoActual = new Pedido();
        }

        public class Credenciales
        {
            [Required(ErrorMessage = "* NickName obligatorio")]
            public String NickName { get; set; }

            [Required(ErrorMessage = "* Email obligatorio")]
            public String Email { get; set; }

            [Required(ErrorMessage = "* Password obligatoria")]
            [MinLength(4, ErrorMessage = "Se requieren al menos 4 caracteres MIN")]
            [MaxLength(20, ErrorMessage = "la Password no debe tener mas de 20 caracteres")]
            public String Password { get; set; }

            #nullable enable
            public String? HashPassword { get; set; }
            #nullable disable
        }
    }
}
