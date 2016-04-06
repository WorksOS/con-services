using System.Collections.Generic;
using System.Configuration;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using LandfillService.AcceptanceTests.Models;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class Config
    {
        public static string LandfillBaseUri = ConfigurationManager.AppSettings["LandFillWebApiBaseUrl"];
        public static string PMBaseUri = ConfigurationManager.AppSettings["ProjectMonitoringWebApiBaseUrl"];

        public static string KafkaDriver 
        { 
            get 
            { 
                return ConfigurationManager.AppSettings["KafkaDriver"]; 
            } 
        }
        public static string KafkaEndpoint 
        { 
            get 
            { 
                return ConfigurationManager.AppSettings["KafkaEndpoint"]; 
            } 
        }

        public static Dictionary<string, Credentials> credentials = new Dictionary<string, Credentials>()
        {
            {"goodCredentials", new Credentials { userName = ConfigurationManager.AppSettings["UserName"] , password = ConfigurationManager.AppSettings["Password"] }},
            {"invalidUsername", new Credentials { userName = "rubbish", password = "zzzzzzzzz123456" } },            
            {"badCredentials", new Credentials { userName = "akorban", password = "badpassword" } },
            {"noCredentials", new Credentials { userName = "", password = "" } }
        };
    }
}