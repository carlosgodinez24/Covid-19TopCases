using Covid19TopCasesClassLibrary;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Covid19TopCasesXUnitTest
{
    public class RESTfulAPITest
    {
        public const string _regionCatalogEndpoint = "https://localhost:44343/api/global/regions";
        public const string _globalReportEndpoint = "https://localhost:44343/api/global/report";
        public const string _xApiKey = "a2a6530641msh936511891b3799ep1dfa85jsn61d9a963a80b";

        [Theory]
        [InlineData(_regionCatalogEndpoint, _xApiKey, true)]
        public void GetRegionsCatalog(string endpoint, string apikey, bool expected)
        {
            //RESTful API Regions Catalog Consumption and Deserialization
            IRestResponse responseAPI = Utils.RestClientExec(endpoint, Method.GET, apikey);
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

            var jsonResponse = Utils.GetJSON(response);
            bool statusCode200 = (int)responseAPI.StatusCode == (int)HttpStatusCode.OK;

            Console.WriteLine("StatusCode => " + (int)responseAPI.StatusCode);
            Console.WriteLine("JSON response => " + jsonResponse);

            Assert.Equal(statusCode200, expected);
        }

        [Theory]
        [InlineData(_globalReportEndpoint, _xApiKey, true)]
        public void GetGlobalReport(string endpoint, string apikey, bool expected)
        {
            //RESTful API Global Report Consumption and Deserialization
            var request = new RequestCovid19Stats()
            {
                RegionIso = ""
            };
            var json = Utils.GetJSON(request);
            IRestResponse responseAPI = Utils.RestClientExec(endpoint, Method.POST, apikey, json);
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

            var jsonResponse = Utils.GetJSON(response);
            bool statusCode200 = (int)responseAPI.StatusCode == (int)HttpStatusCode.OK;

            Console.WriteLine("StatusCode => " + (int)responseAPI.StatusCode);
            Console.WriteLine("JSON response => " + jsonResponse);

            Assert.Equal(statusCode200, expected);
        }

        [Theory]
        [InlineData(_globalReportEndpoint, _xApiKey, true)]
        public void GetReportByRegion(string endpoint, string apikey, bool expected)
        {
            //RESTful API Report By Region Consumption and Deserialization
            var request = new RequestCovid19Stats()
            {
                RegionIso = "USA"
            };
            var json = Utils.GetJSON(request);
            endpoint += "?iso=" + request.RegionIso;
            IRestResponse responseAPI = Utils.RestClientExec(endpoint, Method.POST, apikey, json);
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

            var jsonResponse = Utils.GetJSON(response);
            bool statusCode200 = (int)responseAPI.StatusCode == (int)HttpStatusCode.OK;

            Console.WriteLine("StatusCode => " + (int)responseAPI.StatusCode);
            Console.WriteLine("JSON response => " + jsonResponse);

            Assert.Equal(statusCode200, expected);
        }
    }
}
