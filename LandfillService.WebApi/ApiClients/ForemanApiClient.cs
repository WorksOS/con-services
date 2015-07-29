using LandfillService.Common;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LandfillService.WebApi.ApiClients
{
    /// <summary>
    /// This exception can be thrown when the Foreman API returns an unsuccessful response (which needs to be propagated to the client)
    /// </summary>
    public class ForemanApiException : ApplicationException
    {
        public HttpStatusCode code { get; set; }

        public ForemanApiException(HttpStatusCode c, string message) : base(message)
        {
            code = c;
        }
    }

    /// <summary>
    /// A wrapper around HttpClient to handle requests to the Foreman API
    /// </summary>
    public class ForemanApiClient
    {
        private HttpClient client;

        public ForemanApiClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ForemanApiUrl"] ?? "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Makes a JSON POST request to the Foreman API
        /// </summary>
        /// <param name="endpoint">URL path fragment for the request</param>
        /// <param name="parameters">JSON parameters</param>
        /// <returns>Response as a string; throws an exception if request is not successful</returns>
        private string Request<TParams>(string endpoint, TParams parameters)  
        {
            System.Diagnostics.Debug.WriteLine("In ForemanApiClient::Request");

            LoggerSvc.LogRequest(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, parameters);

            // Force syncronous processing by calling .Result
            var response = client.PostAsJsonAsync(endpoint, parameters).Result;

            System.Diagnostics.Debug.WriteLine(response.ToString());

            if (!response.IsSuccessStatusCode)
            {
                LoggerSvc.LogResponse(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, response);
                throw new ForemanApiException(response.StatusCode, response.ReasonPhrase);
            }

            System.Diagnostics.Debug.WriteLine("POST request succeeded");
            LoggerSvc.LogResponse(GetType().Name, MethodBase.GetCurrentMethod().Name, client.BaseAddress + endpoint, response);

            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var res = responseContent.ReadAsStringAsync().Result;
            return res;
        }

        /// <summary>
        /// Parses a JSON response from the Foreman API
        /// </summary>
        /// <param name="response">response string</param>
        /// <returns>Response string converted to the given type</returns>
        private T ParseResponse<T>(string response)
        {
            System.Diagnostics.Debug.WriteLine("In ForemanApiClient::ParseResponse");

            // chop out the "d" property wrapper which is added to every Foreman API response, and parse the remaining substring
            var resObj = JsonConvert.DeserializeObject<T>(response.Substring(5, response.Length - 6),
                 new JsonSerializerSettings
                 {
                     Error = delegate(object sender, ErrorEventArgs args)
                     {
                         System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error.Message);
                         args.ErrorContext.Handled = true;
                     }
                 });

            return resObj;
        }

        public string Login(Credentials credentials)
        {
            return ParseResponse<string>(Request("Login", new { username = credentials.userName, password = credentials.password }));
        }

        public string LoginWithKey(string key)
        {
            return ParseResponse<string>(Request("LoginWithKey", new { key = key }));
        }

        public void Logout(string sessionId)
        {
            Request("Logout", new { sessionID = sessionId });
        }

        public IEnumerable<Project> GetProjects(string sessionId)
        {
            var resp = ParseResponse<Project[]>(Request("GetProjects", new { sessionID = sessionId, landfillOnly = true }));
            
            return resp
                .Select(p => { p.timeZoneName = TimeZone.WindowsToIana(p.timeZoneName); return p; })
                .ToList()
                .OrderBy(p => p.name);
        }

      public UnitsTypeEnum GetUserUnits(string sessionId)
      {
        var resp = ParseResponse<ForemanUserPreferences>(Request("GetUserPreferences", new { sessionID = sessionId }));
        return (UnitsTypeEnum)resp.UnitsTypeID;
      }

    }


}