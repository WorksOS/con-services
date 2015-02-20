using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using LandfillService.WebApi.Models;
using LandfillService.WebApi.ApiClients;
using System.Collections;
using System.Web.Hosting;

namespace LandfillService.WebApi.Controllers
{
    [RoutePrefix("api/v1/projects")]
    public class ProjectsController : ApiController
    {
        private ForemanApiClient foremanApiClient = new ForemanApiClient();
        //private RaptorApiClient raptorApiClient = new RaptorApiClient();

        private IHttpActionResult ForemanRequest(Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                return Content(e.code, e.Message);
            }
        }

        // Get a list of available projects
        [Route("")]
        public IHttpActionResult Get()
        {
            return ForemanRequest(() => Ok(foremanApiClient.GetProjects(Request.Headers.GetValues("SessionId").First())));

            //return new Project[] { new Project() {id = 543, name = "Dump"}, new Project() {id = 544, name = "Dumpling"} };
        }

        // Get project data for a given project
        [Route("{id}")]
        public IEnumerable<DayEntry> Get(int id)
        {
            var totalDays = 730;
            var startDate = DateTime.Today.AddDays(-totalDays);

            var entries = new List<DayEntry>();
            var rnd = new Random();


            foreach (int i in Enumerable.Range(0, totalDays))
            {
                bool skip = (i < 728 && rnd.Next(5) % 6 == 0);

                entries.Add(new DayEntry
                {
                    date = DateTime.Today.AddDays(-totalDays + i),
                    entryPresent = !skip,
                    density = skip ? 0 : rnd.Next(1200, 1600),
                    weight = skip ? 0 : rnd.Next(500, 800)
                });
            }
            return entries.ToArray(); 

            //var client = new HttpClient();
            //client.BaseAddress = new Uri("https://dev-mobile.vss-eng.com/foreman/Secure/ForemanSvc.svc/");
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ////System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };

            
            //System.Diagnostics.Debug.WriteLine("Making POST request");

            //// by calling .Result you are performing a synchronous call
            //var response = client.PostAsJsonAsync("GetProjects", new {sessionID = "177c3b4c0b854c26b017deb53debef2f"}).Result;

            //System.Diagnostics.Debug.WriteLine(response.ToString());

            //if (!response.IsSuccessStatusCode)
            //    return "<error>";

            //System.Diagnostics.Debug.WriteLine("POST request succeeded");

            //var responseContent = response.Content;

            //// by calling .Result you are synchronously reading the result
            //var res = responseContent.ReadAsStringAsync().Result;

            //System.Diagnostics.Debug.WriteLine(res);
            
            //return res;
        }

        // POST api/projects
        //public void Post([FromBody]string value)
        //{
        //}

        private void GetVolumeInBackground(WeightEntry entry)
        {
            HostingEnvironment.QueueBackgroundWorkItem(async (CancellationToken cancel) =>
                {
                    try
                    {
                        //TODO: test with a single client instance - shouldn't need one per request
                        using (var raptorApiClient = new RaptorApiClient())
                        {
                            var res = await raptorApiClient.GetVolumesAsync(entry.date);
                            System.Diagnostics.Debug.WriteLine("Volume res:" + res);
                            System.Diagnostics.Debug.WriteLine("Volume: " + (res.Fill - res.Cut));
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Write("Exception while retrieving volumes: " + e);
                    }
                });
        }

        private void GetMissingVolumes()
        {
            //TODO: request any volumes which the service hasn't been able to obtain previously
            // In order to avoid duplicate requests, I need to mark any dates where the volume was requested successfully but isn't available from Raptor
        }

        // Submit weights to the project API
        [Route("{id}/weights")]
        public IHttpActionResult PostWeights(int id, [FromBody] WeightEntry[] entries)
        {
            //TODO: how to respond to the client immediately, and THEN launch volume requests?
            foreach (var entry in entries)
            {
                System.Diagnostics.Debug.WriteLine(entry.ToString());
                // TODO: validate the entry: format of data(?); discard entries older than 2 years   
                // TODO: save the entry
                
                GetVolumeInBackground(entry);
            };
            GetMissingVolumes();

            System.Diagnostics.Debug.WriteLine("Finished posting weights");
            return Ok();
        }

        // DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
    }
}
