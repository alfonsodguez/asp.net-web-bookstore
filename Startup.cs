using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using bookstore.Models.Lib;
using bookstore.Models.Config;
using bookstore.Models.Interfaces;
using bookstore.Models;

namespace bookstore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            #region ----- servicio acceso a datos -------
            services.AddScoped<IDBAccess, SqlServerDBAccess>();
            services.Configure<ConfigSqlServer>( (ConfigSqlServer opciones) => {
                opciones.SqlServerCadenaConexion = Configuration.GetConnectionString("SqlServerCadenaConexion");
            });
            #endregion

            #region ----- servicio cliente de correo -----
            services.AddScoped<IClienteEmail, ClienteCorreoMailjet>();
            services.Configure<ConfigMailject>(Configuration.GetSection("SMTPMailJet")); 
            #endregion

            #region ----- servicio estado de session --------
            services.AddSession((SessionOptions opciones)=> {
                opciones.Cookie.HttpOnly = true;
                opciones.Cookie.MaxAge = new TimeSpan(0, 30, 0);
            });
            services.AddHttpContextAccessor();
            services.AddScoped<IControlSession, ServicioGestionSession>();
            #endregion            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(); 
            app.UseRouting(); 
            app.UseSession(); 
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Tienda}/{action=Libros}/{id?}"); //<-- modificamos la ruta por defecto
            });
        }

    }
}
