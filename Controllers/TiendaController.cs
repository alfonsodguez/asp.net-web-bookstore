using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bookstore.Models.Interfaces;
using bookstore.Models;

namespace bookstore.Controllers
{
    public class TiendaController : Controller
    {
        private IDBAccess _accesoDB;

        public TiendaController(IDBAccess servicioDBInyect)
        {
            this._accesoDB = servicioDBInyect;
        }

        [HttpGet]                                         
        public async Task<IActionResult> Libros(String id) 
        {
            Materia _materiaUrl = new Materia();   

            if (String.IsNullOrEmpty(id))  
            {
                _materiaUrl.IdMateria = 5;
                _materiaUrl.IdMateriaPadre = 0;
                _materiaUrl.NombreMateria = "Informatica;informatica";
            }
            else
            {
                _materiaUrl.IdMateria = System.Convert.ToInt32(id.Split("_")[0]);
                _materiaUrl.IdMateriaPadre = System.Convert.ToInt32(id.Split("_")[1]);
                _materiaUrl.NombreMateria = id.Split("_")[2].ToString();
            }     

            List<Libro> _listaLibros = await this._accesoDB.DevolverLibros(_materiaUrl.IdMateria, null);
            return View(_listaLibros);
        }

        [HttpGet]
        public async Task<IActionResult> MostrarLibro([FromQuery]String isbn, [FromQuery]String idmateria) 
        {
            List<Libro> libros = await this._accesoDB.DevolverLibros(System.Convert.ToInt32(idmateria), isbn);
            Libro _unlibro = libros.Single<Libro>();
            return View(_unlibro);  
        }

        [HttpPost]
        public async Task<IActionResult> BuscadorLibros(String cajaTexto, String optradio) 
        {
            List<Libro> _librosBuscados = await this._accesoDB.BuscarLibros(optradio, cajaTexto);
            return View("Libros", _librosBuscados); 
        }
    }
}
