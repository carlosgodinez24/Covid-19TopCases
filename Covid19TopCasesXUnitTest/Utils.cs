using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Covid19TopCasesXUnitTest
{
    public static class Utils
    {
        /// <summary>
        /// Generic RestSharp function for consumption of RESTful API services
        /// </summary>
        /// <param name="endpoint">REST service endpoint</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="xApiKey">RapidAPI key</param>
        /// <param name="json">JSON request</param>
        /// <returns></returns>
        public static IRestResponse RestClientExec(string endpoint, Method method, string xApiKey, string json = null)
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

        public static string GetJSON(object obj)
        {
            //Get the JSON serialization from the object request
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
            return json;
        }
    }
}
