using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models
{
    public class Materia
    {
        public int IdMateria { get; set; }
        public int IdMateriaPadre { get; set; }
        public String NombreMateria { get; set; }
    }
}
