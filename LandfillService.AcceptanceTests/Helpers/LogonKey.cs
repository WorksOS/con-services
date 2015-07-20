using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LandfillService.WebApi.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.StepDefinitions;
using System.Net;
using System.IO;
using System.Net.Http.Formatting;
using RestSharp;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class TokenKey
    {
        public string grant_type;
        public string username;
        public string password;
    }

    public class AccessToken
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public int daysToAccountExpiry;
        public DateTime issued;
        public DateTime expires;
    }

    public class TemporaryLoginKey
    {
        public string key { get; set; }
    }

    public class LogonKey
    {
        protected HttpClient httpClient;
        protected HttpResponseMessage response;
        protected string AccessToken;

        public string GetKeyToken()
        {
            AccessToken = CallProjectMonitoringAndGetTheAccessToken();
            return AccessToken;
        }

        private string CallProjectMonitoringAndGetTheAccessToken()
        {
            string tokenkey = "grant_type=password&username=dglassenbury&password=Visionlink15_";
            var client = new RestClient(Config.PMServiceUrl + "/token");
            var request = new RestRequest(Method.POST);
            request.AddParameter("text/json", tokenkey, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var atok = SimpleJson.DeserializeObject<AccessToken>(response.Content);
            return SessionLogonWithToken(atok.access_token);
        }

        public string SessionLogonWithToken(string accesstoken)
        {
            var client = new RestClient(Config.PMServiceUrl + "/api/v1/session");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", "Bearer " + accesstoken);
            IRestResponse response = client.Execute(request);
            var tempkey = SimpleJson.DeserializeObject<TemporaryLoginKey>(response.Content);
            return tempkey.key;
        }
    }
}
