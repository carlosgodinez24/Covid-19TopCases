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
