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

namespace LandfillService.WebApi.Controllers
{
    [RoutePrefix("api/v1/projects")]
    public class ProjectsController : ApiController
    {
        private ForemanApiClient foremanApiClient = new ForemanApiClient(); 

        // GET api/projects
        // Get a list of available projects
        [Route("")]
        public IHttpActionResult Get()
        {
            try
            {
                return Ok(foremanApiClient.GetProjects(Request.Headers.GetValues("SessionId").First()));
            }
            catch (ForemanApiException e)
            {
                return Content(e.code, e.Message);
            }
            //return new Project[] { new Project() {id = 543, name = "Dump"}, new Project() {id = 544, name = "Dumpling"} };
        }

        // GET api/projects/5
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

        // PUT api/projects/5
        // Submit weights to the project API
        public string Put(int id, [FromBody]string data)
        {
            System.Diagnostics.Debug.WriteLine(data);
            return data;
        }

        // DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
    }
}
