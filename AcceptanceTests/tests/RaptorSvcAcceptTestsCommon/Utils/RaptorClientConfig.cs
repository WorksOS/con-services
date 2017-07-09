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

    public static string TestDataPath
    {
      get
      {
        var path = Environment.GetEnvironmentVariable("TEST_DATA_PATH");     
        if (path == null)
        {
          throw new ArgumentNullException("TEST_DATA_PATH");
        }
        return path;
      }
    }

    public static string CompactionSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("COMPACTION_SVC_BASE_URI"));
      }
    }

    public static string NotificationSvcBaseUri
    {
      get
      {
          return ConstructUri(Environment.GetEnvironmentVariable("NOTIFICATION_SVC_BASE_URI"));
      }
    }

    public static string ReportSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("REPORT_SVC_BASE_URI"));
      }
    }
    public static string TagSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("TAG_SVC_BASE_URI"));
      }
    }
    public static string CoordSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("COORD_SVC_BASE_URI"));
      }
    }
    public static string ProdSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("PROD_SVC_BASE_URI"));

      }
    }
    public static string FileAccessSvcBaseUri
    {
      get
      {
        return ConstructUri(Environment.GetEnvironmentVariable("COORD_SVC_BASE_URI"));

      }
    }

    private static string ConstructUri(string subDir)
    {
      var server = "http://" + Environment.GetEnvironmentVariable("RAPTOR_WEBSERVICES_HOST");

      var url = string.Format("{0}{1}", server, subDir);
      return url;
    }
  }
}