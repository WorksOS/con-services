using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace LandfillService.WebApi.Controllers
{
    public class ProjectsController : ApiController
    {
        // GET api/projects
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/projects/5
        // Get project 
        public string Get(int id)
        {

            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:59674/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // by calling .Result you are performing a synchronous call
            var response = client.GetAsync("projects").Result;

            if (!response.IsSuccessStatusCode)
                return "<error>";

            var responseContent = response.Content;

            // by calling .Result you are synchronously reading the result
            var res = responseContent.ReadAsStringAsync().Result;

            System.Diagnostics.Debug.WriteLine(res);
            
            return res;
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
