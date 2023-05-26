using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookstore.Models;
using bookstore.Models.Interfaces;
// validaciones
using Microsoft.AspNetCore.Mvc.ModelBinding;
// variable sesion
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace bookstore.Controllers
{
    public class ClienteController : Controller
    {
        private IDBAccess _servicioSqlServer;
        private IClienteEmail _servicioClienteEmail;
        private IControlSession _servicioSession; 

        public ClienteController(IDBAccess servicioDBInyect, IClienteEmail servicioMailInyect, IControlSession servicioSessionInectado)
        {
            this._servicioSqlServer = servicioDBInyect;      //<-- servicio acceso a datos
            this._servicioClienteEmail = servicioMailInyect; //<-- servicio cliente de correo 
            this._servicioSession = servicioSessionInectado; //<-- servicio session 
        }


        private async Task<List<Provincia>> _ListaProvincias()
        {
            List<Provincia> lista = await this._servicioSqlServer.DevolverProvincias();
            return lista;
        }

        #region //REGISTRO//
        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            ViewData["ListaProvincias"] = await this._ListaProvincias();  
            return View(new Cliente());  
        }
        
        [HttpPost]
        public async Task<IActionResult> Registro(Cliente cliente) 
        {
            if (ModelState.IsValid)
            {
                bool registroOk = await _servicioSqlServer.RegistrarCliente(nuevoCliente)
                if (registroOk)
                {              
                    String destinatario = cliente.CredencialesCliente.Email
                    String asunto = "Bienvenido al portal de Agapea.com"
                    String adjunto = ""
                    String body = "<h3><strong>Se ha registrado correctamente en Agapea.com</strong></h3>" +
                                    $"<br>Pulsa " +
                                    $"<a href='https://localhost:44311/Cliente/ActivarCuenta/{cliente.CredencialesCliente.Email}'>" +
                                    $"aqui</a> para activar tu cuenta";

                    this._servicioClienteEmail.EnviarEmail(destinatario, asunto, body, adjunto);                                   
                    return RedirectToAction("RegistroOK","Cliente");
                }

                ModelState.AddModelError("", "Error interno, intentelo de nuevo mas tarde");
                ViewData["ListaProvincias"] = await this._ListaProvincias();
                return View(nuevoCliente); 
            }
            ModelState.AddModelError("", "Error en la validacion de los campos del formulario de registro");
            ViewData["ListaProvincias"] = await this._ListaProvincias();
            return View(nuevoCliente); 
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
            if (await this._servicioSqlServer.ActivarCuenta(id)) {
                return RedirectToAction("Login", "Cliente");
            }
            ModelState.AddModelError("", "Error en la activacion de la cuenta");
            return RedirectToAction("ActivarCuentaFail", "Cliente");
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
        public async Task<IActionResult> Login(Cliente.Credenciales credenciales)  
        {
            bool credencialesValidas = ModelState.GetValidationState("Email") == ModelValidationState.Valid && ModelState.GetValidationState("Password") == ModelValidationState.Valid
            if (credencialesValidas)
            {
                Cliente cliente = await this._servicioSqlServer.ComprobarCredenciales(credenciales.Email, credenciales.Password);
                if (cliente != null)
                {
                    HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize(cliente));                                                                                
                    return RedirectToAction("Libros", "Tienda");            
                }                
            }
            ModelState.AddModelError("", "Email o password invalidos");
            return View(credenciales);
        }
        #endregion

        #region //FORGOT PASSWORD//
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
                if (await this._servicioSqlServer.ExisteEmailCliente(Email))
                {
                    return RedirectToAction("Repassword", "Cliente");
                }
            }
            return View(Email);
        }
        #endregion

        #region //MIPERFIL//
        [HttpGet]
        public IActionResult MiPerfil()
        {
            Cliente cliente = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");   
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> MiPerfil(Cliente updateCliente)  
        {
            Cliente cliente = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");
            cliente.Nombre = updateCliente.Nombre;
            cliente.Apellidos = updateCliente.Apellidos;
            cliente.Telefono = updateCliente.Telefono;
            cliente.CredencialesCliente.Email = updateCliente.CredencialesCliente.Email;
            cliente.CredencialesCliente.NickName = updateCliente.CredencialesCliente.NickName;
            cliente.Descripcion = updateCliente.Descripcion;

            bool clienteActualizado await this._servicioSqlServer.ActualizarDatosCliente(updateCliente);
            if (clienteActualizado)
            {
                this._servicioSession.AddItemSession<Cliente>("datoscliente", cliente);
                return RedirectToAction("PanelInicio"); 
            }

            ViewData["mensajeError"] = "error interno del server al actualizar datos ";
            return View(updateCliente);
        }
        #endregion

        #region //PANELINICIO//
        [HttpGet]
        public IActionResult PanelInicio()
        {
            Cliente cliente = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente")
            return View(cliente); 
        }
        #endregion

        #region //MISDATOS//
        [HttpGet]
        public  async Task<IActionResult> MisDatosEnvio()
        {
            List<Direccion> direcciones = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente").ListaDirecciones
            List<Provincia> provincias = await this._ListaProvincias()

            ViewData["ListaProvincias"] = provincias
            return View(direcciones);
        }

        [HttpPost]
        public async Task<IActionResult> AltaDirecciones(String calle, String tipoDireccion, String codpro, String codmun, String cp)
        {                                                                   
            Cliente cliente = this._servicioSession.RecuperaItemSession<Cliente>("datoscliente");
            List<Provincia> provincias = await this._ListaProvincias()
            List<Municipio> municipios = await this._servicioSqlServer.DevolverMunicipios(System.Convert.ToInt32(codmun));

            Direccion direccion = new Direccion {
                Calle = calle,
                CP = System.Convert.ToInt32(cp),
                EsPrincipal = false,
                IdDireccion = System.Guid.NewGuid().ToString() + "-" + cliente.NIF,
                TipoDireccion = tipoDireccion,
                Provincia = provincias.Where((prov) => prov.CodPro == System.Convert.ToInt32(codpro)).Single<Provincia>(),
                Municipio = municipios.Where((municipio) => municipio.CodPro == System.Convert.ToInt32(codpro) && municipio.CodMun == System.Convert.ToInt32(codmun)).Single<Municipio>()
            };

            int altaDireccion = await this._servicioSqlServer.AltaNuevaDireccionCliente(direccion)
            if (altaDireccion == 1)
            {
                cliente.ListaDirecciones.Add(direccion);
                this._servicioSession.AddItemSession<Cliente>("datoscliente", cliente);
                return RedirectToAction("MisDatosEnvio");
            }

            ViewData["mensajeError"] = "error interno del servicio, intentelo de nuevo mas tarde";
            return RedirectToAction("MisDatosEnvio");
        }
        #endregion
    }
}
