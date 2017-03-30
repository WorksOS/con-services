using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Reflection;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Utils;
using Newtonsoft.Json;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    public class RaptorClientConfig
    {
        public static Configuration DLLConfig
        {
            get
            {
                return ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            }
        }
        public static string TestEnvironment
        {
            get
            {
              //return "Local"; 
              return "Dev"; 
              // DLLConfig.AppSettings.Settings["TestEnvironment"].Value;
            }
        }
        public static string TestDataPath
        {
            get
            {
              //Local
              //return "../../../tests/TestData/TestData/"; 
              //Dev
              return "../../TestData/";
              //DLLConfig.AppSettings.Settings["TestDataPath"].Value;
            }
        }

        public static string CompactionSvcBaseUri
        {
          get
          {
            if (TestEnvironment == "Tc")
              return ConstructUri(":3001");
            else if (TestEnvironment == "Dev")
              return ConstructUri(":80");
            else if (TestEnvironment == "Local")
              return ConstructUri(":5000");
            else
              return ConstructUri("/compaction");
          }
        }
        public static string ReportSvcBaseUri
        {
            get
            {
                if (TestEnvironment == "Tc")
                    return ConstructUri(":3001");
                else if (TestEnvironment == "Dev")
                    return ConstructUri(":80");
                else if (TestEnvironment == "Local")
                  return ConstructUri(":5000");
                else
                    return ConstructUri("/Report");
            }
        }
        public static string TagSvcBaseUri
        {
            get
            { 
                if (TestEnvironment == "Tc")
                    return ConstructUri(":3000");
                else if (TestEnvironment == "Dev")
                  return ConstructUri(":80");
                else if (TestEnvironment == "Local")
                  return ConstructUri(":5000");
                else
                    return ConstructUri("/TagProc");
            }
        }
        public static string CoordSvcBaseUri
        {
            get
            {
                if (TestEnvironment == "Tc")
                    return ConstructUri(":3002");
                else if (TestEnvironment == "Dev")
                  return ConstructUri(":80");
                else if (TestEnvironment == "Local")
                    return ConstructUri(":5000");
                else
                    return ConstructUri("/Coord");
            }
        }
        public static string ProdSvcBaseUri
        {
            get
            {
                if (TestEnvironment == "Tc")
                    return ConstructUri(":3003");
              else if (TestEnvironment == "Dev")
                return ConstructUri(":80");
              else if (TestEnvironment == "Local")
                   return ConstructUri(":5000");
                else
                    return ConstructUri("/ProdData");
            }
        }
        public static string ProjectSvcBaseUri
        {
            get
            {
                if (TestEnvironment == "Tc")
                    return ConstructUri(":3004");
                else if (TestEnvironment == "Local")
                    return ConstructUri(":37609");
                else
                    return ConstructUri("/Project");
            }
        }

        private static string ConstructUri(string subDir)
        {
            string server = null;
            if (TestEnvironment == "Tc")
                server = "http://mer-vm-tc-01.ap.trimblecorp.net";
            else if (TestEnvironment == "Dev")
            {
              string addr = "http://" + File.ReadAllText(TestDataPath + "webapiaddress.txt");
              addr = addr.Replace("\n", "").Replace("\r", "").Trim();
              Console.WriteLine("Host WebAPI url>" + addr+"<");
              server = addr;
            }
          else if (TestEnvironment == "T01")
            server = "http://t01-aslv01.vssengg.com/RaptorWebAPI";
          else if (TestEnvironment == "Local")
            server = "http://localhost";

          if (string.IsNullOrEmpty(server))
                return null;

            var url = string.Format("{0}{1}", server, subDir);
            return url;
        }
    }
}