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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace LandfillService.WebApi.ApiClients
{

    public class RaptorApiException : ApplicationException
    {
        public HttpStatusCode code { get; set; }

        public RaptorApiException(HttpStatusCode c, string message) : base(message)
        {
            code = c;
        }
    }

    public class RaptorApiClient : IDisposable
    {
        private HttpClient client;

        public RaptorApiClient()
        {
            //client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RaptorApiUrl"] ?? "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
        }

        public void Dispose()
        {
            client.Dispose();
        }

        //TODO: log requests and responses
        private async Task<string> Request<TParams>(string endpoint, TParams parameters)  
        {
            System.Diagnostics.Debug.WriteLine("In RaptorApiClient::Request to " + endpoint + " with " + parameters);

            // Force syncronous processing by calling .Result
            var response = await client.PostAsJsonAsync(endpoint, parameters);

            if (!response.IsSuccessStatusCode)
                throw new RaptorApiException(response.StatusCode, response.ReasonPhrase);

            System.Diagnostics.Debug.WriteLine("POST request succeeded");

            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var res = await responseContent.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("Response: " + res);

            return res;
        }

        private T ParseResponse<T>(string response)
        {
            System.Diagnostics.Debug.WriteLine("In RaptorApiClient::ParseResponse");

            var resObj = JsonConvert.DeserializeObject<T>(response,
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

        public async Task<SummaryVolumesResult> GetVolumesAsync(Project project, DateTime date)
        {
            // TODO: retrieve correct time zone from the Foreman API
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(LandfillDb.TimeZone.IanaToWindows(project.timeZone));
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(date, timeZone);

            System.Diagnostics.Debug.WriteLine("UTC time range in volume request: {0} - {1}", utcDateTime.ToString(), utcDateTime.AddDays(1).ToString());

            var volumeParams = new VolumeParams()
            {
                projectId = project.id,
                volumeCalcType = 4,
                baseFilter = new VolumeFilter() { startUTC = utcDateTime, endUTC = utcDateTime.AddDays(1), returnEarliest = true, gpsAccuracy = 1 },
                topFilter = new VolumeFilter() { startUTC = utcDateTime, endUTC = utcDateTime.AddDays(1), returnEarliest = false, gpsAccuracy = 1 }
            };
            return ParseResponse<SummaryVolumesResult>(await Request("volumes/summary", volumeParams));
        }

    }
}