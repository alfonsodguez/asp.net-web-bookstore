using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bookstore.Models.Interfaces;
using Microsoft.Extensions.Configuration;

//hashear/comporbar hash de passwords
using BCrypt.Net;

//acceso a datos contra sqlserver
using System.Data;
using System.Data.SqlClient;


namespace bookstore.Models
{
    public class SqlServerDBAccess : IDBAccess
    {
        private IConfiguration _accesoAppSettings;
        private String _cadenaConexionDB;
        
        public SqlServerDBAccess(IConfiguration accesoConfigInyect)
        {
            this._accesoAppSettings = accesoConfigInyect;
            this._cadenaConexionDB = this._accesoAppSettings.GetConnectionString("SqlServerCadenaConexion");  
        }


        #region //REGISTRO//
        public async Task<List<Municipio>> DevolverMunicipios(int codpro)
        {
            try
            {
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _selectMunis = new SqlCommand("SELECT * from dbo.Municipios WHERE CodPro = @codpro ORDER BY NombreMunicipio ASC ;", _conexionDB); //<--- PARAMETRIZAR 
                _selectMunis.Parameters.AddWithValue("@codpro", codpro);

                SqlDataReader _result = await _selectMunis.ExecuteReaderAsync();
                List<Municipio> _listaMunis = new List<Municipio>();

                while (_result.Read())
                {
                    _listaMunis.Add(new Municipio  
                    {
                        CodPro = codpro,
                        CodMun = System.Convert.ToInt16(_result["CodMun"]),
                        NombreMunicipio = _result["NombreMunicipio"].ToString()
                    });
                }
                return _listaMunis;

                #region --------- con LINQ ------------
                /*
                return _listaMunis.Cast<IDataRecord>()
                                  .Select((IDataRecord fila) => new Municipio
                                  {
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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _selectProv = new SqlCommand("SELECT * FROM dbo.Provincias ORDER BY NombreProvincia ASC;",_conexionDB);
                SqlDataReader _result = await _selectProv.ExecuteReaderAsync();

                return _result
                    .Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Provincia
                    {
                        CodPro = System.Convert.ToInt16(fila["CodPro"]),
                        NombreProvincia = fila["NombreProvincia"].ToString()
                    })
                    .ToList<Provincia>();
                
                #region ...query con aspnet puro...
                /*
                 List<Provincia> _listaProv = new List<Provincia>();
                while (_result.Read())
                {
                    Provincia _prov = new Provincia();
                    _prov.CodPro = System.Convert.ToInt16(_result["CodPro"]);
                    _prov.NombreProvincia = _result["NombreProvincia"].ToString();

                    _listaProv.Add(_prov);
                }
                return _listaProv;
                */
                #endregion
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<bool> RegistrarCliente(Cliente nuevoCliente)
        {
            try
            {
                SqlConnection _conexionDB = new SqlConnection();
                _conexionDB.ConnectionString = this._cadenaConexionDB;
                await _conexionDB.OpenAsync();

                SqlCommand _insertCliente = new SqlCommand();
                _insertCliente.Connection = _conexionDB;
                _insertCliente.CommandText = "INSERT INTO dbo.Clientes VALUES(@NIF,@Nom,@Ape,@Em,@Hp,@Nick,@IdDir,@tlfno,@Act,@Desc,@Avat);";
                _insertCliente.Parameters.AddWithValue("@NIF", nuevoCliente.NIF);
                _insertCliente.Parameters.AddWithValue("@Nom", nuevoCliente.Nombre);
                _insertCliente.Parameters.AddWithValue("@Ape", nuevoCliente.Apellidos);
                _insertCliente.Parameters.AddWithValue("@Em", nuevoCliente.CredencialesCliente.Email);

                //hasheamos la password 
                String _hash = BCrypt.Net.BCrypt.HashPassword(nuevoCliente.CredencialesCliente.Password, 10);
                _insertCliente.Parameters.AddWithValue("@Hp", _hash);

                _insertCliente.Parameters.AddWithValue("@Nick", nuevoCliente.CredencialesCliente.NickName);

                //cremaos un identificador unico para el idDireccion
                _insertCliente.Parameters.AddWithValue("@IdDir", "Principal-"+nuevoCliente.NIF); 

                _insertCliente.Parameters.AddWithValue("@tlfno", nuevoCliente.Telefono);
                _insertCliente.Parameters.AddWithValue("@Act", false); 
                _insertCliente.Parameters.AddWithValue("@Desc", DBNull.Value); 
                _insertCliente.Parameters.AddWithValue("@Avat", DBNull.Value);


                int _filasafectadas = await _insertCliente.ExecuteNonQueryAsync();  
                if(_filasafectadas == 1)
                {
                    SqlCommand _insertDir = new SqlCommand("INSERT INTO dbo.Direcciones VALUES(@idDir,@codpro,@codmun,@calle,@cp,@esprincipal);", _conexionDB);
                    _insertDir.Parameters.AddWithValue("@idDir", "Principal-" + nuevoCliente.NIF);

                    Direccion _direccionPpal = nuevoCliente.ListaDirecciones[0];
                    _insertDir.Parameters.AddWithValue("@codpro", _direccionPpal.Provincia.CodPro);
                    _insertDir.Parameters.AddWithValue("@codmun", _direccionPpal.Municipio.CodMun);
                    _insertDir.Parameters.AddWithValue("@calle", _direccionPpal.Calle);
                    _insertDir.Parameters.AddWithValue("@cp", _direccionPpal.CP);
                    _insertDir.Parameters.AddWithValue("@esprincipal", true);

                    int _filasinsertdirecc = await _insertDir.ExecuteNonQueryAsync();
                    if(_filasinsertdirecc == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _updateCuentaActiva = new SqlCommand("UPDATE dbo.Clientes SET CuentaActivada = 1 WHERE Email like @email;",_conexionDB);
                _updateCuentaActiva.Parameters.AddWithValue("@email", email);

                int _resultUpdate = await _updateCuentaActiva.ExecuteNonQueryAsync();
                if(_resultUpdate == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _selectCliente = new SqlCommand("SELECT * FROM dbo.Clientes WHERE Email = @email;",_conexionDB);
                _selectCliente.Parameters.AddWithValue("@email", email);

                SqlDataReader _resultCliente = await _selectCliente.ExecuteReaderAsync();

                Cliente _clienteLogin = null;
                if(_resultCliente.Read())
                {                    
                    if(BCrypt.Net.BCrypt.Verify(password, _resultCliente["HashPassword"].ToString()) )
                    {
                        _clienteLogin = new Cliente();
                        _clienteLogin.NIF = _resultCliente["NIF"].ToString();
                        _clienteLogin.Nombre = _resultCliente["Nombre"].ToString();
                        _clienteLogin.Apellidos = _resultCliente["Apellidos"].ToString();
                        _clienteLogin.Telefono = _resultCliente["Telefono"].ToString();
                        _clienteLogin.CuentaActivada = System.Convert.ToBoolean(_resultCliente["CuentaActivada"]);
                        _clienteLogin.Descripcion = _resultCliente["Descripcion"].ToString();
                        _clienteLogin.ImagenAvatar = _resultCliente["ImagenAvatar"].ToString();

                        _clienteLogin.CredencialesCliente.Email = _resultCliente["Email"].ToString();
                        _clienteLogin.CredencialesCliente.NickName = _resultCliente["NickName"].ToString();
                 
                        _clienteLogin.ListaDirecciones = new List<Direccion>(); 
                        _clienteLogin.HistoricoPedidos = new List<Pedido>(); 

                        _clienteLogin.PedidoActual = new Pedido { NIF = _clienteLogin.NIF };

                        return _clienteLogin;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }                
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
                SqlConnection _conexion = new SqlConnection(this._cadenaConexionDB);
                await _conexion.OpenAsync();

                SqlCommand _selectcli = new SqlCommand("Select * From dbo.Clientes WHERE Email=@em");
                _selectcli.Parameters.AddWithValue("@em", email);

                SqlDataReader _resultado = await _selectcli.ExecuteReaderAsync();
                if (_resultado.Read())
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _selectMaterias = new SqlCommand("SELECT * FROM dbo.Materias WHERE IdMateriaPadre= @id",_conexionDB);
                _selectMaterias.Parameters.AddWithValue("@id", idmateriapadre);

                SqlDataReader _resultadofilas = await _selectMaterias.ExecuteReaderAsync();

                List<Materia> listaMaterias = _resultadofilas.Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Materia 
                    {
                        IdMateria = System.Convert.ToInt32(fila["IdMateria"]),
                        IdMateriaPadre = System.Convert.ToInt32(fila["IdMateriaPadre"]),
                        NombreMateria = fila["NombreMateria"].ToString()
                    })
                    .ToList<Materia>();

                return listaMaterias;
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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                String _consulta = "SELECT * FROM dbo.Libros WHERE IdMateria = @idmat";
                SqlCommand _selectLibros;

                if (isbn != null)
                {
                    _selectLibros = new SqlCommand(_consulta + " AND ISBN = @isbn;", _conexionDB);
                    _selectLibros.Parameters.AddWithValue("@idmat", idmateria);
                    _selectLibros.Parameters.AddWithValue("@isbn", isbn);
                }
                else
                {
                    _selectLibros = new SqlCommand(_consulta, _conexionDB);
                    _selectLibros.Parameters.AddWithValue("@idmat", idmateria);
                }

                SqlDataReader _resultado = await _selectLibros.ExecuteReaderAsync();

                return _resultado.Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Libro  
                    {
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
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();
               
                SqlCommand _select = new SqlCommand();
                _select.CommandText = "SELECT * FROM dbo.Libros";

                if(filtro != null)
                {
                    switch (filtro)
                    {
                        case "Materia":
                            _select.CommandText += " WHERE IdMateria = @valor ";
                            break;
                        case "Titulo":
                            _select.CommandText += " WHERE Titulo LIKE %@valor%";
                            break;
                        case "ISBN":
                            _select.CommandText += " WHERE ISBN = @valor";
                            break;
                        case "Autor":
                            _select.CommandText += " WHERE Autor LIKE %@valor%";
                            break;
                        case "Editorial":
                            _select.CommandText += " WHERE Editorial = @valor";
                            break;
                        default:
                            throw new Exception();
                    }
                    _select.Parameters.AddWithValue("@valor", (filtro == "Materia") ? System.Convert.ToInt32(valor) : valor);
                }
                else
                {
                    _select.CommandText = ""; 
                }

                _select.Connection = _conexionDB;
                SqlDataReader _result = await _select.ExecuteReaderAsync();

                return _result.Cast<IDataRecord>()
                    .Select((IDataRecord fila) => new Libro  
                    {
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
                SqlConnection _conexionDB = new SqlConnection(_cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _insertPedido = new SqlCommand("INSERT INTO dbo.Pedidos VALUES (@idPedido, @nif, @fecha, @estado, @gastosEnv, @subtotal, @total);", _conexionDB);
                _insertPedido.Parameters.AddWithValue("@idPedido", pedidoActual.NIF + DateTime.Now.ToString());

                _insertPedido.Parameters.AddWithValue("@nif", pedidoActual.NIF);
                _insertPedido.Parameters.AddWithValue("@fecha", DateTime.Now);
                _insertPedido.Parameters.AddWithValue("@estado", pedidoActual.EstadoPedido);
                _insertPedido.Parameters.AddWithValue("@gastosEnv", pedidoActual.GastosDeEnvio);
                _insertPedido.Parameters.AddWithValue("@subtotal", pedidoActual.SubTotalPedido);
                _insertPedido.Parameters.AddWithValue("@total", pedidoActual.TotalPedido);

                int _filasInsert = await _insertPedido.ExecuteNonQueryAsync();
                if (_filasInsert == 1)
                {
                    pedidoActual.ListaPedido.ForEach((ItemPedido item) => {
                        SqlCommand _insertItem = new SqlCommand("dbo.AlmacenarItemsPedidoStoredProc", _conexionDB);
                        _insertItem.CommandType = CommandType.StoredProcedure;

                        _insertItem.Parameters.AddWithValue("@IdPedido", pedidoActual.IdPedido);
                        _insertItem.Parameters.AddWithValue("@ISBN", item.LibroPedido.ISBN);
                        _insertItem.Parameters.AddWithValue("@CantidadLibro", item.CantidadLibro);

                        if (_insertItem.ExecuteNonQuery() != 1) throw new Exception();
                        _insertItem = null;
                       }
                   );
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;                
            }
        }
        #endregion

        #region //PANEL DEL CLIENTE//
        public async Task<bool> ActualizarDatosCliente(Cliente clientemodif)
        {
            try
            {
                SqlConnection _conexionDB = new SqlConnection(this._cadenaConexionDB);
                await _conexionDB.OpenAsync();

                SqlCommand _updateCli = new SqlCommand(
                    @"UPDATE dbo.Clientes 
                      SET Nombre = @nom,
                          Apellidos = @ape,
                          Email = @em,
                          Telefono = @tlfno,
                          Descripcion = @desc,
                          NickName = @log
                      WHERE NIF = @nif"
                    ,_conexionDB);

                _updateCli.Parameters.AddWithValue("@nom", clientemodif.Nombre);
                _updateCli.Parameters.AddWithValue("@ape", clientemodif.Apellidos);
                _updateCli.Parameters.AddWithValue("@em", clientemodif.CredencialesCliente.Email);
                _updateCli.Parameters.AddWithValue("@tlfno", clientemodif.Telefono);
                _updateCli.Parameters.AddWithValue("@desc", clientemodif.Descripcion);
                _updateCli.Parameters.AddWithValue("@log", clientemodif.CredencialesCliente.NickName);
                _updateCli.Parameters.AddWithValue("@nif", clientemodif.NIF);

                int _result = await _updateCli.ExecuteNonQueryAsync();

                return _result == 1 ? true : false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int> AltaNuevaDireccionCliente(Direccion nuevaDirec)
        {
            try
            {
                SqlConnection _conexion = new SqlConnection(this._cadenaConexionDB);
                await _conexion.OpenAsync();

                SqlCommand _insertDir = new SqlCommand(
                    @"INSERT INTO dbo.Direcciones
                      VALUES (@id,@codpro,@codmun,@calle,@cp,@tipo,@esppal)",
                    _conexion);

                _insertDir.Parameters.AddWithValue("@id", nuevaDirec.IdDireccion);
                _insertDir.Parameters.AddWithValue("@codpro", nuevaDirec.Provincia.CodPro);
                _insertDir.Parameters.AddWithValue("@codmun", nuevaDirec.Municipio.CodMun);
                _insertDir.Parameters.AddWithValue("@calle", nuevaDirec.Calle);
                _insertDir.Parameters.AddWithValue("@cp", nuevaDirec.CP);
                _insertDir.Parameters.AddWithValue("@tipo", nuevaDirec.TipoDireccion);
                _insertDir.Parameters.AddWithValue("@esppal", nuevaDirec.EsPrincipal);

                return await _insertDir.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion
    }
}
