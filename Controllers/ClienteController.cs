using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
using bookstore.Models;
using bookstore.Models.Interfaces;

//validaciones
using Microsoft.AspNetCore.Mvc.ModelBinding;

//VARIABLE SESSION
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace bookstore.Controllers
{
    public class ClienteController : Controller
    {
        private IDBAccess _servicioSQLServer;
        private IClienteEmail _servicioClienteEmail;
        private IControlSession _servicioSession; 


        public ClienteController(IDBAccess servicioDBInyect,IClienteEmail servicioMailInyect, IControlSession servicioSessionInectado)
        {
            this._servicioSQLServer = servicioDBInyect;      //<-- servicio acceso a datos
            this._servicioClienteEmail = servicioMailInyect; //<-- servicio cliente de correo 
            this._servicioSession = servicioSessionInectado; //<-- servicio session 
        }


        private async Task<List<Provincia>> ListaProvincias()
        {
            List<Provincia> lista = await this._servicioSQLServer.DevolverProvincias();
            return lista;
        }

        #region //REGISTRO//
        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            ViewData["ListaProvincias"] = await this.ListaProvincias();  
            return View(new Cliente());  
        }
        
        [HttpPost]
        public async Task<IActionResult> Registro(Cliente nuevoCliente) 
        {
            if (ModelState.IsValid)
            {
                if ( await _servicioSQLServer.RegistrarCliente(nuevoCliente))
                {              
                    String _body = "<h3><strong>Se ha registrado correctamente en Agapea.com</strong></h3>" +
                                    $"<br>Pulsa " +
                                    $"<a href='https://localhost:44311/Cliente/ActivarCuenta/{nuevoCliente.CredencialesCliente.Email}'>" +
                                    $"aqui</a> para activar tu cuenta";
                    
                    this._servicioClienteEmail.EnviarEmail(
                        nuevoCliente.CredencialesCliente.Email, //<--- DESTINATARIO
                        "Bienvenido al portal de Agapea.com",   //<--- ASUNTO 
                        _body,                                  //<--- CUERPO DEL MENSAJE
                        "");                                    //<--- nombre adjunto (opcional)
                    
                    return RedirectToAction("RegistroOK","Cliente");
                }
                else
                {
                    ModelState.AddModelError("", "Error de procesado de datos en servidor, intentelo de nuevo mas tarde");
                    ViewData["ListaProvincias"] = await this.ListaProvincias();
                    return View(nuevoCliente); 
                }
            }
            else
            {
                ModelState.AddModelError("", "Error en la validacion de los campos del formulario de registro");
                ViewData["ListaProvincias"] = await this.ListaProvincias();
                return View(nuevoCliente); 
            }

        }

        [HttpGet]
        public IActionResult RegistroOK()
        {
            return View();
        }
        #endregion

        #region //ACTIVAR CUENTA//
        [HttpGet]
        public async Task<IActionResult> ActivarCuenta(String id)  
        {
            if (await this._servicioSQLServer.ActivarCuenta(id)){
                return RedirectToAction("Login", "Cliente");
            }
            else
            {
                ModelState.AddModelError("", "Error en la activacion de la cuenta");

                return RedirectToAction("ActivarCuentaFail", "Cliente");
            }           
        }

        [HttpGet]
        public IActionResult ActivarCuentaFail() 
        {
            return View();
        }
        #endregion

        #region //LOGIN//
        [HttpGet]
        public IActionResult Login()
        {
            return View(new Cliente.Credenciales());   
        }

        [HttpPost]
        public async Task<IActionResult> Login(Cliente.Credenciales creds)  
        {
            if (ModelState.GetValidationState("Email") == ModelValidationState.Valid && ModelState.GetValidationState("Password") == ModelValidationState.Valid)
            {
                Cliente _clienteLogin = await this._servicioSQLServer.ComprobarCredenciales(creds.Email, creds.Password);
                if(_clienteLogin != null)
                {
                    HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize(_clienteLogin));  
                                                                                                             
                    return RedirectToAction("Libros", "Tienda");            
                }
                else
                {
                    ModelState.AddModelError("", "Email o password invalidos");
                    return View(creds);
                }
            }
            else
            {
                return View(creds);
            }        
        }
        #endregion

        #region //FORGOT PASSWORD
        [HttpGet]
        public IActionResult ForgottenPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgottenPassword([FromBody] String Email)
        {
            if (ModelState.IsValid)
            {
                if (await this._servicioSQLServer.ExisteEmailCliente(Email))
                {
                    return RedirectToAction("Repassword", "Cliente");
                }
                else
                {
                    return View(Email);
                }
            }
            else
            {
                return View(Email);
            }
        }
        #endregion

        #region //MIPERFIL//
        [HttpGet]
        public IActionResult MiPerfil()
        {
            Cliente _clienteLogin = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");   
            return View(_clienteLogin);
        }

        [HttpPost]
        public async Task<IActionResult> MiPerfil(Cliente clientemodif)  
        {
            Cliente _clientelogueado = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");

            _clientelogueado.Nombre = clientemodif.Nombre;
            _clientelogueado.Apellidos = clientemodif.Apellidos;
            _clientelogueado.Telefono = clientemodif.Telefono;
            _clientelogueado.CredencialesCliente.Email = clientemodif.CredencialesCliente.Email;
            _clientelogueado.CredencialesCliente.NickName = clientemodif.CredencialesCliente.NickName;
            _clientelogueado.Descripcion = clientemodif.Descripcion;

            bool _result = await this._servicioSQLServer.ActualizarDatosCliente(clientemodif);
            if (_result)
            {
                this._servicioSession.AddItemSession<Cliente>("datoscliente", _clientelogueado);

                return RedirectToAction("PanelInicio"); 
            }
            else
            {
                ViewData["mensajeError"] = "error interno del server al actualizar datos ";
                return View(clientemodif);
            }
        }
        #endregion

        #region //PANELINICIO//
        [HttpGet]
        public IActionResult PanelInicio()
        {
            return View(this._servicioSession.RecuperaItemSession<Cliente>("datoscliente")); 
        }
        #endregion

        #region //MISDATOS//
        [HttpGet]
        public IActionResult MisDatosEnvio()
        {
            ViewData["ListaProvincias"] = this._servicioSQLServer.DevolverProvincias();

            return View(this._servicioSession.RecuperaItemSession<Cliente>("datoscliente").ListaDirecciones);
        }

        [HttpPost]
        public async Task<IActionResult> AltaDirecciones(String direccion, String tipoDireccion, String codpro, String codmun, String cp)
        {                                                                   
            Cliente _clientelogueado = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");
            List<Provincia> _provincias = await this._servicioSQLServer.DevolverProvincias();
            List<Municipio> _municipios = await this._servicioSQLServer.DevolverMunicipios(System.Convert.ToInt32(codmun));

            Direccion _nuevaDirec = new Direccion 
            {
                Calle = direccion,
                CP = System.Convert.ToInt32(cp),
                EsPrincipal = false,
                IdDireccion = System.Guid.NewGuid().ToString() + "-" + _clientelogueado.NIF,
                TipoDireccion = tipoDireccion,
                Provincia = _provincias.Where((prov) => prov.CodPro == System.Convert.ToInt32(codpro)).Single<Provincia>(),
                Municipio = _municipios.Where((municipio) => municipio.CodPro == System.Convert.ToInt32(codpro) && municipio.CodMun == System.Convert.ToInt32(codmun)).Single<Municipio>()
            };

            if(await this._servicioSQLServer.AltaNuevaDireccionCliente(_nuevaDirec) == 1)
            {
                _clientelogueado.ListaDirecciones.Add(_nuevaDirec);

                this._servicioSession.AddItemSession<Cliente>("datoscliente", _clientelogueado);
                return RedirectToAction("MisDatosEnvio");
            }
            else
            {
                ViewData["mensajeError"] = "error interno del servicio, intentelo de nuevo mas tarde";
                return RedirectToAction("MisDatosEnvio");
            }
        }
        #endregion
    }
}
