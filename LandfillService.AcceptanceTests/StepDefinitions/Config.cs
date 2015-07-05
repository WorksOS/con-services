using System;
using System.Collections.Generic;
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
        public static string ServiceUrl = "http://10.210.246.188/LandfillService/api/v1/";
        public static Dictionary<string, Credentials> credentials = new Dictionary<string, Credentials>()
        {
            {"goodCredentials", new Credentials { userName = "dglassenbury", password = "Visionlink15_" } },
            {"invalidUsername", new Credentials { userName = "rubbish", password = "zzzzzzzzz123456" } },            
            {"badCredentials", new Credentials { userName = "akorban", password = "badpassword" } },
            {"noCredentials", new Credentials { userName = "", password = "" } }
        };
    }
}
