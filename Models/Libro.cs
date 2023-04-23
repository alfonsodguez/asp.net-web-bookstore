using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models
{
    public class Libro
    {
        public String ISBN { get; set; }
        public String ISBN13 { get; set; }
        public String Titulo { get; set; }
        public String Editorial { get; set; }
        public String Autores { get; set; }
        public int NumeroPaginas { get; set; }
        public decimal Precio { get; set; }
        public String FicheroImagen { get; set; }
        public String Descripcion { get; set; }
        public int IdMateria { get; set; }

        public static implicit operator Libro(List<Libro> v)
        {
            throw new NotImplementedException();
        }
    }
}
