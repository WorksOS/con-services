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

                return header;
            }
        }
    }
}
