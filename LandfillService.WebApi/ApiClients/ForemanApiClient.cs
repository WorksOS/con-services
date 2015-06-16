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
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Request:");
            System.Diagnostics.Debug.WriteLine(request.ToString());
            if (request.Content != null)
            {
                System.Diagnostics.Debug.WriteLine(await request.Content.ReadAsStringAsync());
            }
            System.Diagnostics.Debug.WriteLine("\n");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            System.Diagnostics.Debug.WriteLine("Response:");
            System.Diagnostics.Debug.WriteLine(response.ToString());
            if (response.Content != null)
                System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());
            else
                System.Diagnostics.Debug.WriteLine("<No content in the response>");
            System.Diagnostics.Debug.WriteLine("\n");

            return response;
        }
    }

    public class ForemanApiException : ApplicationException
    {
        public HttpStatusCode code { get; set; }

        public ForemanApiException(HttpStatusCode c, string message) : base(message)
        {
            code = c;
        }
    }

    public class ForemanApiClient
    {
        private HttpClient client;

        public ForemanApiClient()
        {
            //client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ForemanApiUrl"] ?? "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
        }

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

            //var response = client.PostAsJsonAsync("GetProjects", new { sessionID = sessionId }).Result;

            //System.Diagnostics.Debug.WriteLine(response.ToString());

            //// TODO: It would be good to propagate "unauthorised" responses out to the client so it can redirect the user to the login page; 
            //// so the exception needs to include that information somehow.
            //if (!response.IsSuccessStatusCode)
            //    throw new Exception("Unable to retrieve projects from the Foreman API: " + response.ToString());

            //System.Diagnostics.Debug.WriteLine("POST request succeeded");

            //var responseContent = response.Content;

            //// by calling .Result you are synchronously reading the result
            //var res = responseContent.ReadAsStringAsync().Result;

            //var projects = JsonConvert.DeserializeObject<Project[]>(res,
            //     new JsonSerializerSettings
            //     {
            //         Error = delegate(object sender, ErrorEventArgs args)
            //         {
            //             System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error.Message);
            //             args.ErrorContext.Handled = true;
            //         }
            //     });

            //return projects;
        } 
    }
}