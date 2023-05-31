using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookstore.Models.Interfaces;
using bookstore.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace bookstore.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]  

    //----------- servicio RESTFULL ---------------
    public class RestController : ControllerBase 
    {
        private IDBAccess _accesoDB;
        private IControlSession _servicioSession;

        public RestController(IDBAccess servicioDBInyect, IControlSession servicioSessionInyect)
        {
            this._accesoDB = servicioDBInyect;
            this._servicioSession = servicioSessionInyect;
        }


        [HttpGet]
        public async Task<IActionResult> DevolverMunicipios([FromQuery] int codPro)
        {
            List<Municipio> municipios = await this._accesoDB.DevolverMunicipios(codPro);
            return new JsonResult(municipios);
        }

        [HttpPost]
        public IActionResult uploadImagen(IFormFile imagen)  
        {
            Cliente cliente = this._servicioSession.RecuperarSession<Cliente>("datoscliente")
            String nif = cliente.NIF
            String nombreFichero = imagen.FileName.Split('.')[0] + "-" + nif 
            String extensionFichero = "." + imagen.FileName.Split('.')[1];
            String fichero = nombreFichero + extensionFichero

            FileStream file = new FileStream(Path.Combine("Uploads", fichero), FileMode.Create);
            imagen.CopyTo(file);

            return Ok(new { codigo=0, mensaje = "imagen subida al server ok!!" });
        }
    }
}
