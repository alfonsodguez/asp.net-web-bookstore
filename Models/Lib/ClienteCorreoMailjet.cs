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
        private IOptions<EmailServerMAILJET> _configServerMAILJET;
      
        public ClienteCorreoMailjet(IOptions<EmailServerMAILJET> objConfigServerMAILJET)
        {
            this._configServerMAILJET = objConfigServerMAILJET;
        }


        #nullable enable
        public void EnviarEmail(string ToEmailCliente, string Subject, string Body, string? nombreAdjunto)
        {
            SmtpClient _clienteSMTP = new SmtpClient();
            _clienteSMTP.Host = this._configServerMAILJET.Value.ServerName;
            _clienteSMTP.Credentials = new NetworkCredential(this._configServerMAILJET.Value.APIKey, this._configServerMAILJET.Value.SecretKey);

            MailAddress remitente = new MailAddress("aabebop@mailfence.com");  

            MailAddress destinatario = new MailAddress(ToEmailCliente);

            MailMessage _mensajeAEnviar = new MailMessage(remitente, destinatario);
            _mensajeAEnviar.Body = Body;
            _mensajeAEnviar.Subject = Subject;
            _mensajeAEnviar.IsBodyHtml = true; 

            if (!String.IsNullOrEmpty(nombreAdjunto))
            {
                FileStream _filecontent = new FileStream(nombreAdjunto, FileMode.Open, FileAccess.Read);
                _mensajeAEnviar.Attachments.Add(new Attachment(_filecontent, nombreAdjunto, "application/pdf"));
            }
            _clienteSMTP.SendAsync(_mensajeAEnviar, null);
        }
        #nullable disable
    }
}
