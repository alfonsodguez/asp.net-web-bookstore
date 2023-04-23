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

    [Route("api/[controller]/[action]")]  
    [ApiController]

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
        public async Task<IActionResult> DevolverMunicipios([FromQuery] int codpro)
        {
            List<Municipio> _listaMunis = await this._accesoDB.DevolverMunicipios(codpro);
            return new JsonResult(_listaMunis);
        }

        [HttpPost]
        public IActionResult uploadImagen(IFormFile imagen)  
        {
            String _nif = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente").NIF;
            String _nombrefichero = imagen.FileName.Split('.')[0] + "-" + _nif + "." + imagen.FileName.Split('.')[1];

            FileStream _lectorEscritor = new FileStream(Path.Combine("Uploads", _nombrefichero),FileMode.Create);
            imagen.CopyTo(_lectorEscritor);

            return Ok(new { codigo=0, mensaje = "imagen subida al server ok!!" });
        }
    }
}
