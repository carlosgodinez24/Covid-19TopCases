using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using ClassLibraryNetChallenge;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetChallenge.Models;

namespace NetChallenge.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        public IConfiguration Configuration;

        public HomeController(IConfiguration iConfig)
        {
            Configuration = iConfig;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Servicio de inserción de Afiliado
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(nameof(InsertAfiliado))]
        public ActionResult InsertAfiliado([FromBody] Afiliado request)
        {
            //Se inicializa objeto de respuesta del servicio
            long idAfiliado = 0;
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                FechaHoraTransaccion = DateTime.Now,
                Data = new List<Afiliado>()
            };

            try
            {
                //Se obtienen parámetros de appsettings.json
                var connectionString = Configuration.GetValue<string>("DataBaseSettings:ConnectionString");
                var schema = Configuration.GetValue<string>("DataBaseSettings:Schema");

                //Validación de cadena de conexion y esquema de base de datos
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se ha especificado cadena de conexion hacia la base de datos");
                }
                if (string.IsNullOrEmpty(schema))
                {
                    throw new Exception("No se ha especificado esquema de la base de datos");
                }

                //Definición de insert con los parámetros
                StringBuilder insertSQL = new StringBuilder();
                insertSQL.Append($@"INSERT INTO {schema}.AFILIADOS ");
                insertSQL.Append($@"(Nombre, DUI, NUP, Direccion, TelefonoResidencia, TelefonoTrabajo, TelefonoCelular, EstadoPensionado, FechaNacimiento) ");
                insertSQL.Append($@"values (@Nombre, @DUI, @NUP, @Direccion, @TelefonoResidencia, @TelefonoTrabajo, @TelefonoCelular, @EstadoPensionado, @FechaNacimiento); ");
                insertSQL.Append($@"SELECT CAST(SCOPE_IDENTITY() as bigint) ");

                //Inserción a la tabla de afiliados
                using (var connection = new SqlConnection(connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add(name: "Nombre", value: request.Nombre, direction: ParameterDirection.Input);
                    param.Add(name: "DUI", value: request.DUI, direction: ParameterDirection.Input);
                    param.Add(name: "NUP", value: request.NUP, direction: ParameterDirection.Input);
                    param.Add(name: "Direccion", value: request.Direccion, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoResidencia", value: request.TelefonoResidencia, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoTrabajo", value: request.TelefonoTrabajo, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoCelular", value: request.TelefonoCelular, direction: ParameterDirection.Input);
                    param.Add(name: "EstadoPensionado", value: request.EstadoPensionado, direction: ParameterDirection.Input);
                    param.Add(name: "FechaNacimiento", value: request.FechaNacimiento, direction: ParameterDirection.Input);

                    idAfiliado = connection.Query<long>(insertSQL.ToString(), param).Single();
                }

                response.Status = (int)HttpStatusCode.OK;
                response.Code = "OK";
                response.Message = "Afiliado registrado con éxito con id = " + idAfiliado;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "Se ha producido un error interno al insertar nuevo afiliado.";
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Servicio de actualización de datos de Afiliado
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(nameof(UpdateAfiliado))]
        public ActionResult UpdateAfiliado([FromBody] Afiliado request)
        {
            //Se inicializa objeto de respuesta del servicio
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                FechaHoraTransaccion = DateTime.Now,
                Data = new List<Afiliado>()
            };

            try
            {
                //Se obtienen parámetros de appsettings.json
                var connectionString = Configuration.GetValue<string>("DataBaseSettings:ConnectionString");
                var schema = Configuration.GetValue<string>("DataBaseSettings:Schema");

                //Validación de cadena de conexion y esquema de base de datos
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se ha especificado cadena de conexion hacia la base de datos");
                }
                if (string.IsNullOrEmpty(schema))
                {
                    throw new Exception("No se ha especificado esquema de la base de datos");
                }

                //Definición de update con los parámetros
                StringBuilder updateSQL = new StringBuilder();
                updateSQL.Append($@"UPDATE {schema}.AFILIADOS SET Nombre = @Nombre, DUI = @DUI, NUP = @NUP, Direccion = @Direccion, TelefonoResidencia = @TelefonoResidencia, ");
                updateSQL.Append($@"TelefonoTrabajo = @TelefonoTrabajo, TelefonoCelular = @TelefonoCelular, EstadoPensionado = @EstadoPensionado, FechaNacimiento = @FechaNacimiento ");
                updateSQL.Append($@"WHERE Id = @Id");
                
                //Actualización a la tabla de afiliados
                using (var connection = new SqlConnection(connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add(name: "Nombre", value: request.Nombre, direction: ParameterDirection.Input);
                    param.Add(name: "DUI", value: request.DUI, direction: ParameterDirection.Input);
                    param.Add(name: "NUP", value: request.NUP, direction: ParameterDirection.Input);
                    param.Add(name: "Direccion", value: request.Direccion, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoResidencia", value: request.TelefonoResidencia, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoTrabajo", value: request.TelefonoTrabajo, direction: ParameterDirection.Input);
                    param.Add(name: "TelefonoCelular", value: request.TelefonoCelular, direction: ParameterDirection.Input);
                    param.Add(name: "EstadoPensionado", value: request.EstadoPensionado, direction: ParameterDirection.Input);
                    param.Add(name: "FechaNacimiento", value: request.FechaNacimiento, direction: ParameterDirection.Input);
                    param.Add(name: "Id", value: request.Id, direction: ParameterDirection.Input);

                    connection.Execute(updateSQL.ToString(), param);
                }

                response.Status = (int)HttpStatusCode.OK;
                response.Code = "OK";
                response.Message = "Datos de afiliado con id = " + request.Id + " actualizados con éxito";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "Se ha producido un error interno al actualizar datos de afiliado.";
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Servicio de eliminación de Afiliado
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(nameof(DeleteAfiliado))]
        public ActionResult DeleteAfiliado([FromBody] Afiliado request)
        {
            //Se inicializa objeto de respuesta del servicio
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                FechaHoraTransaccion = DateTime.Now,
                Data = new List<Afiliado>()
            };

            try
            {
                //Se obtienen parámetros de appsettings.json
                var connectionString = Configuration.GetValue<string>("DataBaseSettings:ConnectionString");
                var schema = Configuration.GetValue<string>("DataBaseSettings:Schema");

                //Validación de cadena de conexion y esquema de base de datos
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se ha especificado cadena de conexion hacia la base de datos");
                }
                if (string.IsNullOrEmpty(schema))
                {
                    throw new Exception("No se ha especificado esquema de la base de datos");
                }

                //Definición de delete con los parámetros
                string deleteSQL = $@"DELETE FROM {schema}.AFILIADOS WHERE Id = @Id";

                //Eliminación a la tabla de afiliados
                using (var connection = new SqlConnection(connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add(name: "Id", value: request.Id, direction: ParameterDirection.Input);

                    connection.Execute(deleteSQL.ToString(), param);
                }

                response.Status = (int)HttpStatusCode.OK;
                response.Code = "OK";
                response.Message = "Datos de afiliado con id = " + request.Id + " eliminados con éxito";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "Se ha producido un error interno al eliminar datos de afiliado.";
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Servicio de obtención de datos de un Afiliado por id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetAfiliadoById))]
        public ActionResult GetAfiliadoById([FromBody] Afiliado request)
        {
            //Se inicializa objeto de respuesta del servicio
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                FechaHoraTransaccion = DateTime.Now,
                Data = new List<Afiliado>()
            };

            try
            {
                //Se obtienen parámetros de appsettings.json
                var connectionString = Configuration.GetValue<string>("DataBaseSettings:ConnectionString");
                var schema = Configuration.GetValue<string>("DataBaseSettings:Schema");

                //Validación de cadena de conexion y esquema de base de datos
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se ha especificado cadena de conexion hacia la base de datos");
                }
                if (string.IsNullOrEmpty(schema))
                {
                    throw new Exception("No se ha especificado esquema de la base de datos");
                }

                //Definición de select con los parámetros
                string query = $@"SELECT * FROM {schema}.AFILIADOS WHERE Id = @Id";

                //Consulta a la tabla de afiliados
                using (var connection = new SqlConnection(connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add(name: "Id", value: request.Id, direction: ParameterDirection.Input);

                    response.Data = connection.Query<Afiliado>(query.ToString(), param).ToList();
                }

                if (response.Data.Count() > 0)
                {
                    response.Status = (int)HttpStatusCode.OK;
                    response.Code = "OK";
                    response.Message = "Datos de afiliado con id = " + request.Id + " obtenidos con éxito";
                    return Ok(response);
                }
                else
                {
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Code = "OK";
                    response.Message = "Datos de afiliado con id = " + request.Id + " no existen en la base de datos";
                    return BadRequest(response);
                }
                
            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "Se ha producido un error interno al obtener datos de afiliado.";
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Servicio que devuelve toda la lista de afiliados registrados
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetAfiliados))]
        public ActionResult GetAfiliados()
        {
            //Se inicializa objeto de respuesta del servicio
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                FechaHoraTransaccion = DateTime.Now,
                Data = new List<Afiliado>()
            };

            try
            {
                //Se obtienen parámetros de appsettings.json
                var connectionString = Configuration.GetValue<string>("DataBaseSettings:ConnectionString");
                var schema = Configuration.GetValue<string>("DataBaseSettings:Schema");

                //Validación de cadena de conexion y esquema de base de datos
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No se ha especificado cadena de conexion hacia la base de datos");
                }
                if (string.IsNullOrEmpty(schema))
                {
                    throw new Exception("No se ha especificado esquema de la base de datos");
                }

                //Definición de select con los parámetros
                string query = $@"SELECT * FROM {schema}.AFILIADOS";

                //Consulta a la tabla de afiliados
                using (var connection = new SqlConnection(connectionString))
                {
                    response.Data = connection.Query<Afiliado>(query.ToString()).ToList();
                }

                if (response.Data.Count() > 0)
                {
                    response.Status = (int)HttpStatusCode.OK;
                    response.Code = "OK";
                    response.Message = "Datos de afiliados obtenidos con éxito";
                    return Ok(response);
                }
                else
                {
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Code = "OK";
                    response.Message = "No existen afiliados registrados en la base de datos";
                    return BadRequest(response);
                }

            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "Se ha producido un error interno al obtener datos de afiliado.";
                response.ErrorMessage = ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode(500, response);
            }
        }
    }
}
