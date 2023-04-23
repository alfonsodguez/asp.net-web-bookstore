using System;

namespace bookstore.Models
{
    public class Direccion
    {
        public String IdDireccion { get; set; } 
        public string Calle { get; set; }
        public Provincia Provincia { get; set; }
        public Municipio Municipio { get; set; }
        public int CP { get; set; }
        public bool? EsPrincipal { get; set; } = false; 
        public String TipoDireccion { get; set; } 
    }
}