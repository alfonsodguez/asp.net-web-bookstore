using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bookstore.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using bookstore.Models;


namespace bookstore.InfraEstructura.ViewComponents
{
    public class PanelListaMaterias : ViewComponent  
    {
        private IDBAccess _accesoDB;

        public PanelListaMaterias(IDBAccess servicioDBInyect)
        {
            this._accesoDB = servicioDBInyect;
        }
       

        [HttpGet]
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Materia> _listaMaterias = await this._accesoDB.DevolverMaterias(0);
            
            return View(_listaMaterias);
        }
    }
}
