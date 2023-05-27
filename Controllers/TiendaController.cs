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
            Materia materia = new Materia();   

            if (String.IsNullOrEmpty(id))  
            {
                materia.IdMateria = 5;
                materia.IdMateriaPadre = 0;
                materia.NombreMateria = "Informatica;informatica";
            }
            else
            {
                materia.IdMateria = System.Convert.ToInt32(id.Split("")[0]);
                materia.IdMateriaPadre = System.Convert.ToInt32(id.Split("")[1]);
                materia.NombreMateria = id.Split("")[2].ToString();
            }     
            List<Libro> libros = await this._accesoDB.DevolverLibros(materia.IdMateria, null);
            return View(libros);
        }

        [HttpGet]
        public async Task<IActionResult> MostrarLibro([FromQuery]String isbn, [FromQuery]String idmateria) 
        {
            List<Libro> libros = await this._accesoDB.DevolverLibros(System.Convert.ToInt32(idmateria), isbn);
            Libro libro = libros.Single<Libro>();
            return View(libro);  
        }

        [HttpPost]
        public async Task<IActionResult> BuscadorLibros(String busqueda, String opcion) 
        {
            List<Libro> libros = await this._accesoDB.BuscarLibros(opcion, busqueda);
            return View("Libros", libros); 
        }
    }
}
