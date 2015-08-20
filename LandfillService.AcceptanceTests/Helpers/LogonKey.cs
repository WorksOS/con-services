using System;
using LandfillService.AcceptanceTests.StepDefinitions;
using RestSharp;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
      //  protected HttpClient httpClient;
       // protected HttpResponseMessage response;
        protected string AccessToken;

        public string GetKeyToken()
        {
            AccessToken = CallProjectMonitoringAndGetTheAccessToken();
            return AccessToken;
        }

        private string CallProjectMonitoringAndGetTheAccessToken()
        {
            string tokenkey = "grant_type=password&username=" + ConfigurationManager.AppSettings["UserName"] + "&password=" + ConfigurationManager.AppSettings["Password"];
            var client = new RestClient(Config.pmServiceUrl + "/token");
            var request = new RestRequest(Method.POST);
            request.AddParameter("text/json", tokenkey, ParameterType.RequestBody);
            IRestResponse restResponse = client.Execute(request);
            Assert.IsFalse(restResponse.Content.Length == 0,
                "There is a problem requests a token from the project monitoring service. Error is:" + restResponse.ErrorMessage);
            Assert.IsTrue(restResponse.Content.Contains("access_token"), "There is a problem requests a token from the project monitoring service" + restResponse.Content);
            var atok = SimpleJson.DeserializeObject<AccessToken>(restResponse.Content);
            return SessionLogonWithToken(atok.access_token);
        }

        public string SessionLogonWithToken(string accesstoken)
        {
            var client = new RestClient(Config.pmServiceUrl + "/api/v1/session");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", "Bearer " + accesstoken);
            IRestResponse restResponse = client.Execute(request);
            var tempkey = SimpleJson.DeserializeObject<TemporaryLoginKey>(restResponse.Content);
            return tempkey.key;
        }
    }
}
