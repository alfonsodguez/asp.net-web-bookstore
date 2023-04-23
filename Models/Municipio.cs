using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookstore.Models
{
    public class Municipio
    {
        public int CodPro { get; set; }
        public int CodMun { get; set; }
        #nullable enable
        public String? NombreMunicipio { get; set; }
        #nullable disable
    }
}
