using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Covid19TopCasesClassLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RestSharp;

namespace Covid19TopCasesAPI.Controllers
{
    /// <summary>
    /// Global controller of the RESTful 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalController : ControllerBase
    {
        public IConfiguration Configuration;

        public GlobalController(IConfiguration iConfig)
        {
            Configuration = iConfig;
        }

        /// <summary>
        /// Service that obtains COVID-19 top global statistics
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(Report))]
        public ActionResult Report([FromBody] RequestCovid19Stats requestCovid19Stats)
        {
            //Response object is initialized
            var response = new ServiceResponse()
            {
                Status = 0,
                Code = string.Empty,
                Message = string.Empty,
                ErrorMessage = string.Empty,
                StackTrace = string.Empty,
                TransactionDateTime = DateTime.Now,
                Data = new List<object>()
            };

            try
            {
                //Get the parameters from appsettings.json
                var globalReportEndpoint = Configuration.GetValue<string>("Covid-19APISettings:APIEndpoints:Reports"); //Global Report Endpoint
                var regionsCatalogEndpoint = Configuration.GetValue<string>("Covid-19APISettings:APIEndpoints:RegionsCatalog"); //Regions Catalog Endpoint
                var provincesCatalogEndpoint = Configuration.GetValue<string>("Covid-19APISettings:APIEndpoints:ProvincesCatalog"); //Provinces Catalog Endpoint
                var xRapidApiKey = Configuration.GetValue<string>("Covid-19APISettings:Headers:x-rapidapi-key"); //API key
                var xRapidApiHost = Configuration.GetValue<string>("Covid-19APISettings:Headers:x-rapidapi-host"); //API host

                //Parameters from appsettings.json are validated
                if (string.IsNullOrEmpty(globalReportEndpoint))
                {
                    throw new Exception("Global report endpoint is not configured");
                }
                if (string.IsNullOrEmpty(regionsCatalogEndpoint))
                {
                    throw new Exception("Regions catalog endpoint is not configured");
                }
                if (string.IsNullOrEmpty(provincesCatalogEndpoint))
                {
                    throw new Exception("Provinces catalog endpoint is not configured");
                }
                if (string.IsNullOrEmpty(xRapidApiKey))
                {
                    throw new Exception("API key is not configured");
                }
                if (string.IsNullOrEmpty(xRapidApiHost))
                {
                    throw new Exception("API host is not configured");
                }

                //API Key is validated
                Request.Headers.TryGetValue("x-rapidapi-key", out StringValues xApiKey);
                if (!xRapidApiKey.ToString().Equals(xApiKey))
                {
                    //Request is not authorized
                    response.Status = (int)HttpStatusCode.Unauthorized;
                    response.Code = "ERROR";
                    response.Message = "You are not unauthorized to do the request.";
                    response.ErrorMessage = "401 Unauthorized";
                    response.StackTrace = string.Empty;
                    return StatusCode((int)HttpStatusCode.Unauthorized, response);
                }

                //Request is validated
                if (requestCovid19Stats == null)
                {
                    //Bad Request
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Code = "ERROR";
                    response.Message = "We are sorry, the request sent is not valid.";
                    response.ErrorMessage = "400 Bad Request";
                    response.StackTrace = string.Empty;
                    return BadRequest(response);
                }

                //Get ISO from request
                var regionIso = requestCovid19Stats.RegionIso?.Trim();

                //RapidAPI Regions/Provinces Catalog Consumption
                var catalogEndpoint = string.IsNullOrEmpty(regionIso) ? regionsCatalogEndpoint : provincesCatalogEndpoint += "?iso=" + regionIso;
                IRestResponse responseCatalog = RapidAPIResponse(catalogEndpoint, Method.GET, xRapidApiKey, xRapidApiHost);
                if ((int)responseCatalog.StatusCode == (int)HttpStatusCode.OK) //SUCCESS response from the RapidAPI (Regions/Provinces Catalog)
                {
                    //Rapid API Global Report Service Consumption with RestSharp
                    var globalEndpoint = string.IsNullOrEmpty(regionIso) ? globalReportEndpoint : globalReportEndpoint += "?iso=" + regionIso;
                    IRestResponse responseReport = RapidAPIResponse(globalEndpoint, Method.GET, xRapidApiKey, xRapidApiHost);
                    if ((int)responseReport.StatusCode == (int)HttpStatusCode.OK) //SUCCESS response from the RapidAPI (Global Report)
                    {
                        //Deserialization of JSON data COVID-19 cases around the world
                        var globalReport = JsonConvert.DeserializeObject<GlobalReport>(responseReport.Content).Data.ToList();
                        //
                        // TOP 10 GLOBAL DATA / COVID-19
                        //
                        if (string.IsNullOrEmpty(regionIso))
                        {
                            //Definition of generic list to store COVID-19 data per region
                            var genericList = new List<TopRegionsStatistics>();
                            //Deserialization of Regions Data (JSON)
                            var regionCatalog = JsonConvert.DeserializeObject<RegionCatalog>(responseCatalog.Content).Data.ToList();
                            //Iteration of the region catalog to sum COVID-19 data per region
                            regionCatalog.ForEach(r =>
                            {
                                var regionStatistic = new TopRegionsStatistics()
                                {
                                    Region = r.Name,
                                    Cases = globalReport.Where(g => r.Iso.Equals(g.Region.Iso)).ToList().Sum(g => g.Confirmed),
                                    Deaths = globalReport.Where(g => r.Iso.Equals(g.Region.Iso)).ToList().Sum(g => g.Deaths)
                                };
                                genericList.Add(regionStatistic);
                            });
                            //Get the TOP 10 of regions with most COVID-19 cases
                            var topList = genericList.OrderByDescending(a => a.Cases).Take(10).ToList();
                            //Adding commas in thousands places 
                            topList.ForEach(a =>
                            {
                                a.CasesStr = string.Format("{0:n0}", a.Cases);
                                a.DeathsStr = string.Format("{0:n0}", a.Deaths);
                            });
                            response.Data = topList; //Top 10 list is added to response
                        }
                        //
                        // TOP 10 PROVINCES BY REGION / COVID-19
                        //
                        else
                        {
                            //Definition of generic list to store COVID-19 data per province
                            var genericList = new List<TopProvincesStatistics>();
                            //Deserialization of Provinces Data (JSON)
                            var provinceCatalog = JsonConvert.DeserializeObject<ProvinceCatalog>(responseCatalog.Content).Data.ToList();
                            //Iteration of the region catalog to sum COVID-19 data per region
                            provinceCatalog.ForEach(p =>
                            {
                                var provinceStatistic = new TopProvincesStatistics()
                                {
                                    Province = string.IsNullOrEmpty(p.Province) ? p.Name : p.Province,
                                    Cases = globalReport.Where(g => p.Province.Equals(g.Region.Province)).ToList().Sum(g => g.Confirmed),
                                    Deaths = globalReport.Where(g => p.Province.Equals(g.Region.Province)).ToList().Sum(g => g.Deaths)
                                };
                                genericList.Add(provinceStatistic);
                            });
                            //Get the TOP 10 of provinces with most COVID-19 cases
                            var topList = genericList.OrderByDescending(a => a.Cases).Take(10).ToList();
                            //Adding commas in thousands places 
                            topList.ForEach(a =>
                            {
                                a.CasesStr = string.Format("{0:n0}", a.Cases);
                                a.DeathsStr = string.Format("{0:n0}", a.Deaths);
                            });
                            response.Data = topList; //Top 10 list is added to response
                        }
                        //Preparing the RESTful API response
                        response.Status = (int)responseReport.StatusCode;
                        response.Code = "OK";
                        response.Message = "Data obtained successfully";
                        return Ok(response);
                    }
                    else //Another status code from the RapidAPI (Global Report)
                    {
                        response.Status = (int)responseReport.StatusCode;
                        response.Code = "ERROR";
                        response.Message = "An error occurred in the data query of the global report, please try again later.";
                        response.ErrorMessage = (int)responseReport.StatusCode + " " + responseReport.StatusDescription;
                        response.StackTrace = "JSON returned from the Rapid API => " + responseReport.Content;
                        return StatusCode((int)responseReport.StatusCode, response);
                    }
                }
                else //Another status code from the RapidAPI (Regions/Provinces Catalog)
                {
                    var catalogType = string.IsNullOrEmpty(regionIso) ? "regions" : "provinces";
                    response.Status = (int)responseCatalog.StatusCode;
                    response.Code = "ERROR";
                    response.Message = "An error occurred in the data query of the " + catalogType + " catalog, please try again later.";
                    response.ErrorMessage = (int)responseCatalog.StatusCode + " " + responseCatalog.StatusDescription;
                    response.StackTrace = "JSON returned from the Rapid API => " + responseCatalog.Content;
                    return StatusCode((int)responseCatalog.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Code = "ERROR";
                response.Message = "An internal server error has ocurred, please try again later.";
                response.ErrorMessage = "500 Internal Server Error | " + ex.Message;
                response.StackTrace = ex.StackTrace;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        /// <summary>
        /// Generic RestSharp function for consumption of RapidAPI's REST services
        /// </summary>
        /// <param name="endpoint">REST service endpoint</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="xRapidApiKey">RapidAPI key</param>
        /// <param name="xRapidApiHost">RapidAPI host</param>
        /// <returns></returns>
        private static IRestResponse RapidAPIResponse(string endpoint, Method method, string xRapidApiKey, string xRapidApiHost)
        {
            var client = new RestClient(endpoint);
            var restRequest = new RestRequest(method);
            restRequest.AddHeader("Accept-Encoding", "gzip, deflate");
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("x-rapidapi-key", xRapidApiKey);
            restRequest.AddHeader("x-rapidapi-host", xRapidApiHost);
            //Omission of SSL certificates for all requests made
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            IRestResponse restResponse = client.Execute(restRequest);
            return restResponse;
        }
    }
}
