using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    public class Config
    {
        //public static string ServiceUrl = "http://localhost:59674/api/v1/";
        public static string ServiceUrl = ConfigurationManager.AppSettings["LandFillServiceApiUrl"];
        public static string PMServiceUrl = ConfigurationManager.AppSettings["ProjectMonitoringApiUrl"];
        public static Dictionary<string, Credentials> credentials = new Dictionary<string, Credentials>()
        {
            {"goodCredentials", new Credentials { userName = ConfigurationManager.AppSettings["UserName"] , password = ConfigurationManager.AppSettings["Password"] }},
            {"invalidUsername", new Credentials { userName = "rubbish", password = "zzzzzzzzz123456" } },            
            {"badCredentials", new Credentials { userName = "akorban", password = "badpassword" } },
            {"noCredentials", new Credentials { userName = "", password = "" } }
        };
    }
}
