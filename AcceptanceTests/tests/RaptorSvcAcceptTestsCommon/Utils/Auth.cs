using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Utils;
using Newtonsoft.Json;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    public static class Auth
    {
        public static string AuthProvider
        {
            get
            {
                return RaptorClientConfig.DLLConfig.AppSettings.Settings["AuthProvider"].Value;
            }
        }

        public static string GetVLSessionID()
        {
            string formanUri = "https://dev-mobile.vss-eng.com/foreman/Secure/ForemanSvc.svc/Login";
            string vlUsername = RaptorClientConfig.DLLConfig.AppSettings.Settings["VLUserName"].Value;
            string vlPassword = RaptorClientConfig.DLLConfig.AppSettings.Settings["VLPassword"].Value;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new { username = vlUsername, password = vlPassword };
                Logger.Info(formanUri, Logger.ContentType.URI);
                Logger.Info(string.IsNullOrEmpty(client.DefaultRequestHeaders.ToString()) ? client.DefaultRequestHeaders.ToString() :
                    client.DefaultRequestHeaders.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.RequestHeader);
                Logger.Info(JsonConvert.SerializeObject(request), Logger.ContentType.Request);
                HttpResponseMessage response = client.PostAsJsonAsync(formanUri, request).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    Logger.Info(response.StatusCode.ToString(), Logger.ContentType.HttpCode);
                    Logger.Info(string.IsNullOrEmpty(response.Headers.ToString()) ? response.Headers.ToString() :
                        response.Headers.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.ResponseHeader);
                    Logger.Info(responseContent, Logger.ContentType.Response);

                    var sessionIdDef = new { d = "" };
                    var sessionId = JsonConvert.DeserializeAnonymousType(responseContent, sessionIdDef);
                    return sessionId.d;
                }
            }

            throw new Exception("Unable to get VL session ID.");
        }
        public static string GetTCCTicket()
        {
            string formanUri = "https://dev-mobile.vss-eng.com/Foreman/Secure/ForemanSvc.svc/GetTCCDetails";
            string vlSessionId = GetVLSessionID();

            if (vlSessionId != null)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var request = new { sessionID = vlSessionId };
                    Logger.Info(formanUri, Logger.ContentType.URI);
                    Logger.Info(string.IsNullOrEmpty(client.DefaultRequestHeaders.ToString()) ? client.DefaultRequestHeaders.ToString() :
                        client.DefaultRequestHeaders.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.RequestHeader);
                    Logger.Info(JsonConvert.SerializeObject(request), Logger.ContentType.Request);
                    HttpResponseMessage response = client.PostAsJsonAsync(formanUri, request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().Result;

                        Logger.Info(response.StatusCode.ToString(), Logger.ContentType.HttpCode);
                        Logger.Info(string.IsNullOrEmpty(response.Headers.ToString()) ? response.Headers.ToString() :
                            response.Headers.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.ResponseHeader);
                        Logger.Info(responseContent, Logger.ContentType.Response);

                        var ticketDef = new { d = new { __type = "", FilespaceName = "", Organization = "", Ticket = "" } };
                        var ticket = JsonConvert.DeserializeAnonymousType(responseContent, ticketDef);
                        return ticket.d.Ticket;
                    }
                }
            }

            throw new Exception("Unable to get TCC Ticket.");
        }
        public static string GetTpaasToken()
        {
            try
            {
                return RaptorClientConfig.DLLConfig.AppSettings.Settings["BearerToken"].Value;
            }
            catch (Exception)
            {
                throw new Exception("Unable to get TPaaS Token.");
            }
        }

        public static WebHeaderCollection HeaderWithAuth
        {
            get
            {
                WebHeaderCollection header = new WebHeaderCollection();

                //if (RaptorClientConfig.TestEnvironment == "Dev")
                //{
                //    if (AuthProvider == "VL")
                //        header.Add(HttpRequestHeader.Authorization, "VL " + GetVLSessionID());
                //    else if (AuthProvider == "TCC")
                //        header.Add(HttpRequestHeader.Authorization, "TCC " + GetTCCTicket());
                //}
                //else if (RaptorClientConfig.TestEnvironment == "Tpaas")
                //{
                //    header.Add("X-API-Token", "VL " + Auth.GetVLSessionID());
                //    header.Add(HttpRequestHeader.Authorization, "Bearer " + GetTpaasToken());
                //}

                //Hardcode authentication for now
                header.Add("X-JWT-Assertion",
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=");
                header.Add("X-VisionLink-CustomerUid", CUSTOMER_UID);
                header.Add("X-VisionLink-ClearCache", "true");
                return header;
            }
        }

      public static readonly string CUSTOMER_UID = "87bdf851-44c5-e311-aa77-00505688274d";
    }
}
