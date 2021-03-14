using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Covid19TopCases.Models;
using Microsoft.Extensions.Configuration;
using Covid19TopCasesClassLibrary;
using System.Collections.Generic;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Covid19TopCases.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration iConfig)
        {
            configuration = iConfig;
        }

        public IActionResult Index()
        {
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
        /// Service that provide the catalog of regions
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(Regions))]
        public ActionResult Regions()
        {
            try
            {
                //Get the parameters from appsettings.json
                var regionsCatalogEndpoint = configuration.GetValue<string>("RESTfulAPISettings:RegionsCatalogEndpoint");
                var xApiKey = configuration.GetValue<string>("RESTfulAPISettings:Headers:x-rapidapi-key");

                //Parameters from appsettings.json are validated
                if (string.IsNullOrEmpty(regionsCatalogEndpoint))
                {
                    throw new Exception("Regions catalog endpoint is not configured");
                }
                if (string.IsNullOrEmpty(xApiKey))
                {
                    throw new Exception("API key is not configured");
                }

                //RESTful API Regions Catalog Consumption and Deserialization
                IRestResponse responseAPI = RESTfulApiResponse(regionsCatalogEndpoint, Method.GET, xApiKey);
                var responseCatalog = JsonConvert.DeserializeObject<ServiceResponse>(responseAPI.Content);
                var response = new ServiceResponse()
                {
                    Status = (int)responseAPI.StatusCode,
                    Code = responseCatalog.Code,
                    Message = responseCatalog.Message,
                    ErrorMessage = responseCatalog.ErrorMessage,
                    StackTrace = responseCatalog.StackTrace,
                    TransactionDateTime = responseCatalog.TransactionDateTime,
                    Data = JsonConvert.DeserializeObject<List<RegionObject>>(responseCatalog.Data.ToString())
                };
                return StatusCode((int)responseAPI.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Code = "ERROR",
                    Message = "An internal error occurred while obtaining the regions catalog, please try again later.",
                    ErrorMessage = "500 Internal Server Error | " + ex.Message,
                    StackTrace = ex.StackTrace,
                    TransactionDateTime = DateTime.Now,
                    Data = new List<object>()
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        /// <summary>
        /// Service that obtains COVID-19 top global statistics
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(Report))]
        public ActionResult Report([FromBody] RequestCovid19Stats requestCovid19Stats)
        {
            try
            {
                //Get the parameters from appsettings.json
                var globalReportEndpoint = configuration.GetValue<string>("RESTfulAPISettings:GlobalReportEndpoint");
                var xApiKey = configuration.GetValue<string>("RESTfulAPISettings:Headers:x-rapidapi-key");

                //Parameters from appsettings.json are validated
                if (string.IsNullOrEmpty(globalReportEndpoint))
                {
                    throw new Exception("Global report endpoint is not configured");
                }
                if (string.IsNullOrEmpty(xApiKey))
                {
                    throw new Exception("API key is not configured");
                }

                //Request is validated
                if (requestCovid19Stats == null)
                {
                    //Bad Request
                    var response400 = new ServiceResponse
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Code = "ERROR",
                        Message = "We are sorry, the request sent is not valid.",
                        ErrorMessage = "400 Bad Request",
                        StackTrace = string.Empty
                    };
                    return BadRequest(response400);
                }

                //Get ISO from request
                var regionIso = requestCovid19Stats.RegionIso?.Trim();

                //Get the JSON serialization from the object request
                var contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };

                var json = JsonConvert.SerializeObject(requestCovid19Stats, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    NullValueHandling = NullValueHandling.Ignore
                });

                //RESTful API Global Report Consumption and Deserialization
                IRestResponse responseAPI = RESTfulApiResponse(globalReportEndpoint, Method.POST, xApiKey, json);
                var responseReport = JsonConvert.DeserializeObject<ServiceResponse>(responseAPI.Content);
                var response = new ServiceResponse()
                {
                    Status = (int)responseAPI.StatusCode,
                    Code = responseReport.Code,
                    Message = responseReport.Message,
                    ErrorMessage = responseReport.ErrorMessage,
                    StackTrace = responseReport.StackTrace,
                    TransactionDateTime = responseReport.TransactionDateTime,
                    Data = JsonConvert.DeserializeObject<List<TopStatistics>>(responseReport.Data.ToString())
                };
                return StatusCode((int)responseAPI.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Code = "ERROR",
                    Message = "An internal error occurred while obtaining the regions catalog, please try again later.",
                    ErrorMessage = "500 Internal Server Error | " + ex.Message,
                    StackTrace = ex.StackTrace,
                    TransactionDateTime = DateTime.Now,
                    Data = new List<object>()
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        /// <summary>
        /// Generic RestSharp function for consumption of RESTful API services
        /// </summary>
        /// <param name="endpoint">REST service endpoint</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="xApiKey">RapidAPI key</param>
        /// <param name="json">JSON request</param>
        /// <returns></returns>
        private static IRestResponse RESTfulApiResponse(string endpoint, Method method, string xApiKey, string json = null)
        {
            var client = new RestClient(endpoint);
            var restRequest = new RestRequest(method);
            restRequest.AddHeader("Accept-Encoding", "gzip, deflate");
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("x-rapidapi-key", xApiKey);
            restRequest.AddParameter("undefined", json, ParameterType.RequestBody);
            //Omission of SSL certificates for all requests made
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            IRestResponse restResponse = client.Execute(restRequest);
            return restResponse;
        }
    }
}
