using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookstore.Models.Interfaces;
using Microsoft.Extensions.Configuration;
// hashear/comporbar passwords
using BCrypt.Net;
// acceso a datos 
using System.Data;
using System.Data.SqlClient;


namespace bookstore.Models
{
    public class ServicioDBAccess : IDBAccess
    {
        private IConfiguration _accesoAppSettings;
        private String _cadenaConexionDB;
        
        public ServicioDBAccess(IConfiguration accesoConfigInyect)
        {
            this._accesoAppSettings = accesoConfigInyect;
            this._cadenaConexionDB = this._accesoAppSettings.GetConnectionString("SqlServerCadenaConexion");  
        }


        #region //REGISTRO//
        public async Task<List<Municipio>> DevolverMunicipios(int codpro)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand selectMunicipios = new SqlCommand("SELECT * from dbo.Municipios WHERE CodPro = @codpro ORDER BY NombreMunicipio ASC ;", conexionDB); //<--- PARAMETRIZAR 
                selectMunicipios.Parameters.AddWithValue("@codpro", codpro);

                SqlDataReader cursorMunicipios = await selectMunicipios.ExecuteReaderAsync();
                List<Municipio> municipios = new List<Municipio>();

                while (cursorMunicipios.Read())
                {
                    municipios.Add(new Municipio {
                        CodPro = codpro,
                        CodMun = System.Convert.ToInt16(cursorMunicipios["CodMun"]),
                        NombreMunicipio = cursorMunicipios["NombreMunicipio"].ToString()
                    });
                }
                return municipios;

                #region --------- con LINQ ------------
                /*
                return municipios
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Municipio {
                        CodPro = codpro,
                        CodMun = System.Convert.ToInt16(fila["CodMun"]),
                        NombreMunicpio = fila["NombreMunicipio"].ToString()
                    })
                    .ToList<Municipio>();
                */
                #endregion
            }
            catch (Exception ex)
            {
                return null;
            }
        
        }

        public async Task<List<Provincia>> DevolverProvincias()
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand selectProvincias = new SqlCommand("SELECT * FROM dbo.Provincias ORDER BY NombreProvincia ASC;", conexionDB);
                SqlDataReader cursorProvincias = await selectProvincias.ExecuteReaderAsync();

                return cursorProvincias
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Provincia {
                        CodPro = System.Convert.ToInt16(fila["CodPro"]),
                        NombreProvincia = fila["NombreProvincia"].ToString()
                    })
                    .ToList<Provincia>();
                
                #region ...query con aspnet puro...
                /*
                List<Provincia> listaProvicias = new List<Provincia>();
                while (cursorProvincias.Read())
                {
                    Provincia provicia = new Provincia();
                    prov.CodPro = System.Convert.ToInt16(cursorProvincias["CodPro"]);
                    prov.NombreProvincia = cursorProvincias["NombreProvincia"].ToString();
                    listaProvicias.Add(provincia);
                }
                return listaProvicias;
                */
                #endregion
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<bool> RegistrarCliente(Cliente cliente)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection();
                conexionDB.ConnectionString = this._cadenaConexionDB;
                await conexionDB.OpenAsync();

                SqlCommand insertCliente = new SqlCommand();
                insertCliente.Connection = conexionDB;
                insertCliente.CommandText = "INSERT INTO dbo.Clientes VALUES(@NIF, @Nom, @Ape, @Em, @Hp, @Nick, @IdDir, @tlfno, @Act, @Desc, @Avat);";
                insertCliente.Parameters.AddWithValue("@NIF", cliente.NIF);
                insertCliente.Parameters.AddWithValue("@Nom", cliente.Nombre);
                insertCliente.Parameters.AddWithValue("@Ape", cliente.Apellidos);
                insertCliente.Parameters.AddWithValue("@Em", cliente.CredencialesCliente.Email);
                insertCliente.Parameters.AddWithValue("@Nick", cliente.CredencialesCliente.NickName);

                //hasheamos la password 
                int salt = 10
                String hash = BCrypt.Net.BCrypt.HashPassword(cliente.CredencialesCliente.Password, salt);
                insertCliente.Parameters.AddWithValue("@Hp", hash);

                //cremaos un identificador unico para el idDireccion
                insertCliente.Parameters.AddWithValue("@IdDir", "Principal-" + cliente.NIF); 

                insertCliente.Parameters.AddWithValue("@tlfno", cliente.Telefono);
                insertCliente.Parameters.AddWithValue("@Act", false); 
                insertCliente.Parameters.AddWithValue("@Desc", DBNull.Value); 
                insertCliente.Parameters.AddWithValue("@Avat", DBNull.Value);

                int clientesInsertados = await insertCliente.ExecuteNonQueryAsync();  
                if (clientesInsertados == 1)
                {
                    SqlCommand insertDireccion = new SqlCommand("INSERT INTO dbo.Direcciones VALUES(@idDir, @codpro, @codmun, @calle, @cp, @esprincipal);", conexionDB);
                    insertDireccion.Parameters.AddWithValue("@idDir", "Principal-" + cliente.NIF);

                    Direccion direccionPpal = cliente.ListaDirecciones[0];
                    insertDireccion.Parameters.AddWithValue("@codpro", direccionPpal.Provincia.CodPro);
                    insertDireccion.Parameters.AddWithValue("@codmun", direccionPpal.Municipio.CodMun);
                    insertDireccion.Parameters.AddWithValue("@calle", direccionPpal.Calle);
                    insertDireccion.Parameters.AddWithValue("@cp", direccionPpal.CP);
                    insertDireccion.Parameters.AddWithValue("@esprincipal", true);

                    int direccionesInsertadas = await insertDir.ExecuteNonQueryAsync();
                    if (direccionesInsertadas == 1)
                    {
                        return true;
                    }
                }
                return false
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region //ACTIVAR CUENTA//
        public async Task<bool> ActivarCuenta(string email)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand updateCliente = new SqlCommand("UPDATE dbo.Clientes SET CuentaActivada = 1 WHERE Email like @email;", conexionDB);
                updateCliente.Parameters.AddWithValue("@email", email);

                int registrosModificados = await updateCuentaActiva.ExecuteNonQueryAsync();
                if (registrosModificados == 1)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region //LOGIN//
        public async Task<Cliente> ComprobarCredenciales(String email, String password)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand selectCliente = new SqlCommand("SELECT * FROM dbo.Clientes WHERE Email = @email;", conexionDB);
                selectCliente.Parameters.AddWithValue("@email", email);

                SqlDataReader cursorCliente = await selectCliente.ExecuteReaderAsync();

                if (cursorCliente.Read())
                {                    
                    bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, cursorCliente["HashPassword"].ToString())
                    if (isValidPassword)
                    {
                        Cliente cliente = new Cliente();
                        cliente.NIF = cursorCliente["NIF"].ToString();
                        cliente.Nombre = cursorCliente["Nombre"].ToString();
                        cliente.Apellidos = cursorCliente["Apellidos"].ToString();
                        cliente.Telefono = cursorCliente["Telefono"].ToString();
                        cliente.CuentaActivada = System.Convert.ToBoolean(cursorCliente["CuentaActivada"]);
                        cliente.Descripcion = cursorCliente["Descripcion"].ToString();
                        cliente.ImagenAvatar = cursorCliente["ImagenAvatar"].ToString();
                        cliente.CredencialesCliente.Email = cursorCliente["Email"].ToString();
                        cliente.CredencialesCliente.NickName = cursorCliente["NickName"].ToString();
                        cliente.ListaDirecciones = new List<Direccion>(); 
                        cliente.HistoricoPedidos = new List<Pedido>(); 
                        cliente.PedidoActual = new Pedido { NIF = cliente.NIF };

                        return clienteLogin;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }                                
        }
        #endregion

        #region //FORGOT PASSWORD//
        public async Task<bool> ExisteEmailCliente(string email)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(this._cadenaConexionDB);
                await conexion.OpenAsync();

                SqlCommand selectCliente = new SqlCommand("Select * From dbo.Clientes WHERE Email = @em");
                selectcli.Parameters.AddWithValue("@em", email);

                SqlDataReader cursorCliente = await selectcli.ExecuteReaderAsync();
                if (cursorCliente.Read())
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region //TIENDA//
        public async Task<List<Materia>> DevolverMaterias(int? idmateriapadre)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand selectMaterias = new SqlCommand("SELECT * FROM dbo.Materias WHERE IdMateriaPadre= @id",conexionDB);
                selectMaterias.Parameters.AddWithValue("@id", idmateriapadre);

                SqlDataReader cursorMaterias = await selectMaterias.ExecuteReaderAsync();

                List<Materia> materias = cursorMaterias
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Materia {
                        IdMateria = System.Convert.ToInt32(fila["IdMateria"]),
                        IdMateriaPadre = System.Convert.ToInt32(fila["IdMateriaPadre"]),
                        NombreMateria = fila["NombreMateria"].ToString()
                    })
                    .ToList<Materia>();

                return materias;
            }
            catch (Exception ex)
            {           
                return null;
            }
        }

#nullable enable
        public async Task<List<Libro>?> DevolverLibros(int idmateria, String? isbn)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                String query = "SELECT * FROM dbo.Libros WHERE IdMateria = @idmat";
                SqlCommand selectLibros;

                if (isbn != null)
                {
                    selectLibros = new SqlCommand(query + " AND ISBN = @isbn;", conexionDB);
                    selectLibros.Parameters.AddWithValue("@idmat", idmateria);
                    selectLibros.Parameters.AddWithValue("@isbn", isbn);
                }
                else
                {
                    selectLibros = new SqlCommand(query, conexionDB);
                    selectLibros.Parameters.AddWithValue("@idmat", idmateria);
                }

                SqlDataReader cursorLibros = await selectLibros.ExecuteReaderAsync();

                return cursorLibros
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Libro {
                        ISBN = fila["ISBN"].ToString(),
                        ISBN13 = fila["ISBN13"].ToString(),
                        Titulo = fila["Titulo"].ToString(),
                        Editorial = fila["Editorial"].ToString(),
                        Autores = fila["Autores"].ToString(),
                        NumeroPaginas = System.Convert.ToInt32(fila["NumeroPaginas"]),
                        Precio = System.Convert.ToDecimal(fila["Precio"]),
                        FicheroImagen = fila["FicheroImagen"].ToString(),
                        Descripcion = fila["Descripcion"].ToString(),
                        IdMateria = System.Convert.ToInt32(fila["IdMateria"])
                    })
                    .ToList<Libro>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Libro>?> BuscarLibros(string? filtro, string? valor)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();
               
                SqlCommand selectLibros = new SqlCommand();
                selectLibros.CommandText = "SELECT * FROM dbo.Libros";

                if (filtro != null)
                {
                    switch (filtro)
                    {
                        case "Materia":
                            select.CommandText += " WHERE IdMateria = @valor ";
                            break;
                        case "Titulo":
                            select.CommandText += " WHERE Titulo LIKE %@valor%";
                            break;
                        case "ISBN":
                            select.CommandText += " WHERE ISBN = @valor";
                            break;
                        case "Autor":
                            select.CommandText += " WHERE Autor LIKE %@valor%";
                            break;
                        case "Editorial":
                            select.CommandText += " WHERE Editorial = @valor";
                            break;
                        default:
                            throw new Exception();
                    }
                    select.Parameters.AddWithValue("@valor", (filtro == "Materia") ? System.Convert.ToInt32(valor) : valor);
                }
                else
                {
                    select.CommandText = ""; 
                }

                select.Connection = conexionDB;
                SqlDataReader cusorLibros = await selectLibros.ExecuteReaderAsync();

                return cusorLibros
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Libro {
                        Autores = fila["Autores"].ToString(),
                        ISBN = fila["ISBN"].ToString(),
                        ISBN13 = fila["ISBN13"].ToString(),
                        Titulo = fila["Titulo"].ToString(),
                        Editorial = fila["Editorial"].ToString(),
                        Descripcion = fila["Descripcion"].ToString(),
                        FicheroImagen = fila["FicheroImagen"].ToString(),
                        IdMateria = System.Convert.ToInt32(fila["IdMateria"]),
                        NumeroPaginas = System.Convert.ToInt32(fila["NumeroPaginas"]),
                        Precio = System.Convert.ToDecimal(fila["Precio"])
                    })
                    .ToList<Libro>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
#nullable disable
        #endregion

        #region //PEDIDO// 
        public async Task<bool> GuardarPedido(Pedido pedidoActual)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(_cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand insertPedido = new SqlCommand("INSERT INTO dbo.Pedidos VALUES (@idPedido, @nif, @fecha, @estado, @gastosEnv, @subtotal, @total);", conexionDB);
                insertPedido.Parameters.AddWithValue("@idPedido", pedidoActual.NIF + DateTime.Now.ToString());
                insertPedido.Parameters.AddWithValue("@nif", pedidoActual.NIF);
                insertPedido.Parameters.AddWithValue("@fecha", DateTime.Now);
                insertPedido.Parameters.AddWithValue("@estado", pedidoActual.EstadoPedido);
                insertPedido.Parameters.AddWithValue("@gastosEnv", pedidoActual.GastosDeEnvio);
                insertPedido.Parameters.AddWithValue("@subtotal", pedidoActual.SubTotalPedido);
                insertPedido.Parameters.AddWithValue("@total", pedidoActual.TotalPedido);

                int pedidosInsertados = await insertPedido.ExecuteNonQueryAsync();
                if (pedidosInsertados == 1)
                {
                    pedidoActual.ListaPedido.ForEach((ItemPedido item) => {
                        SqlCommand insertItem = new SqlCommand("dbo.ItemsPedido", conexionDB);
                        insertItem.CommandType = CommandType.StoredProcedure;

                        insertItem.Parameters.AddWithValue("@IdPedido", pedidoActual.IdPedido);
                        insertItem.Parameters.AddWithValue("@ISBN", item.LibroPedido.ISBN);
                        insertItem.Parameters.AddWithValue("@CantidadLibro", item.CantidadLibro);

                        if (insertItem.ExecuteNonQuery() != 1) {
                            return false
                        }

                    });
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;                
            }
        }
        #endregion

        #region //PANEL DEL CLIENTE//
        public async Task<bool> ActualizarDatosCliente(Cliente cliente)
        {
            try
            {
                SqlConnection conexionDB = new SqlConnection(this._cadenaConexionDB);
                await conexionDB.OpenAsync();

                SqlCommand updateCliente = new SqlCommand(
                    @"UPDATE dbo.Clientes 
                      SET Nombre = @nom,
                          Apellidos = @ape,
                          Email = @em,
                          Telefono = @tlfno,
                          Descripcion = @desc,
                          NickName = @log
                      WHERE NIF = @nif"
                    ,conexionDB);

                updateCliente.Parameters.AddWithValue("@nom", cliente.Nombre);
                updateCliente.Parameters.AddWithValue("@ape", cliente.Apellidos);
                updateCliente.Parameters.AddWithValue("@em", cliente.CredencialesCliente.Email);
                updateCliente.Parameters.AddWithValue("@tlfno", cliente.Telefono);
                updateCliente.Parameters.AddWithValue("@desc", cliente.Descripcion);
                updateCliente.Parameters.AddWithValue("@log", cliente.CredencialesCliente.NickName);
                updateCliente.Parameters.AddWithValue("@nif", cliente.NIF);

                int cursorCliente = await updateCli.ExecuteNonQueryAsync();

                return cursorCliente == 1 ? true : false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int> AltaNuevaDireccionCliente(Direccion direccion)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(this._cadenaConexionDB);
                await conexion.OpenAsync();

                SqlCommand insertDireccion = new SqlCommand(@"INSERT INTO dbo.Direcciones VALUES (@id ,@codpro, @codmun, @calle, @cp, @tipo, @esppal)", conexion);

                insertDireccion.Parameters.AddWithValue("@id", direccion.IdDireccion);
                insertDireccion.Parameters.AddWithValue("@codpro", direccion.Provincia.CodPro);
                insertDireccion.Parameters.AddWithValue("@codmun", direccion.Municipio.CodMun);
                insertDireccion.Parameters.AddWithValue("@calle", direccion.Calle);
                insertDireccion.Parameters.AddWithValue("@cp", direccion.CP);
                insertDireccion.Parameters.AddWithValue("@tipo", direccion.TipoDireccion);
                insertDireccion.Parameters.AddWithValue("@esppal", direccion.EsPrincipal);

                return await insertDireccion.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion
    }
}
