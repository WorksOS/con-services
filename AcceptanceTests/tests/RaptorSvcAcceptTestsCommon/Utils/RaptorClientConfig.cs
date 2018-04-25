using System;
using System.Configuration;
using System.Reflection;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  public class RaptorClientConfig
  {
    public static Configuration DLLConfig => ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

    public static string TestDataPath
    {
      get
      {
        const string testDataPath = "TEST_DATA_PATH";

        var path = Environment.GetEnvironmentVariable(testDataPath);
        if (path == null)
        {
          throw new ArgumentNullException(testDataPath);
        }

        return path;
      }
    }

    public static string CompactionSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("COMPACTION_SVC_BASE_URI"));
    public static string NotificationSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("NOTIFICATION_SVC_BASE_URI"));
    public static string ReportSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("REPORT_SVC_BASE_URI"));
    public static string TagSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("TAG_SVC_BASE_URI"));
    public static string CoordSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("COORD_SVC_BASE_URI"));
    public static string ProdSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("PROD_SVC_BASE_URI"));
    public static string FileAccessSvcBaseUri => ConstructUri(Environment.GetEnvironmentVariable("FILE_ACCESS_SVC_BASE_URI"));

    private static string ConstructUri(string subDir)
    {
      var server = "http://" + Environment.GetEnvironmentVariable("RAPTOR_WEBSERVICES_HOST");

      var url = $"{server}{subDir}";
      return url;
    }
  }
}