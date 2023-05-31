using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookstore.Models.Interfaces;
using Microsoft.Extensions.Options;
// API mailjet
using Mailjet.Client;
using Mailjet.Client.Resources;
// email asp-net
using System.Net.Mail;
using System.Net;
using System.IO;

namespace bookstore.Models
{
    public class ClienteCorreoMailjet : IClienteEmail
    {
        private IOptions<EmailServerMAILJET> _configServerMailjet;
      
        public ClienteCorreoMailjet(IOptions<EmailServerMAILJET> configServerMailject)
        {
            this._configServerMailjet = configServerMailject;
        }


        #nullable enable
        public void EnviarEmail(string destinatario, string asunto, string cuerpo, string? nombreAdjunto)
        {
            String ServerName = this._configServerMailjet.Value.ServerName;
            String ApiKey     = this._configServerMailjet.Value.APIKey;
            String SecretKey  = this._configServerMailjet.Value.SecretKey

            SmtpClient clienteSMTP  = new SmtpClient();
            clienteSMTP.Host = ServerName
            clienteSMTP.Credentials = new NetworkCredential(ApiKey, SecretKey);

            MailAddress remitente = new MailAddress("admin@agapea.com");  
            MailAddress destinatario = new MailAddress(destinatario);
            MailMessage mensaje = new MailMessage(remitente, destinatario);
            mensaje.cuerpo = cuerpo;
            mensaje.asunto = asunto;
            mensaje.IscuerpoHtml = true; 

            if (!String.IsNullOrEmpty(nombreAdjunto))
            {
                FileStream fileContent = new FileStream(nombreAdjunto, FileMode.Open, FileAccess.Read);
                mensaje.Attachments.Add(new Attachment(fileContent, nombreAdjunto, "application/pdf"));
            }
            clienteSMTP.SendAsync(mensaje, null);
        }
        #nullable disable
    }
}
