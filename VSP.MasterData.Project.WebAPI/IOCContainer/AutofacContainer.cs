using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Newtonsoft.Json.Linq;
using VSP.MasterData.Common.Logging;
using VSS.Kafka.DotNetClient;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Kafka.DotNetClient.Producer;

namespace VSP.MasterData.Project.WebAPI
{
  public class AutofacContainer
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public void ApplyDependencyInjection()
    {
      try
      {
        var builder = new ContainerBuilder();
        HttpConfiguration configuration = GlobalConfiguration.Configuration;

        string confluentBaseUrl = null;
        string kafkaTopicName = ConfigurationManager.AppSettings["KafkaTopicName"];

        if (string.IsNullOrWhiteSpace(kafkaTopicName))
          throw new ArgumentNullException("Kafka Topic name is empty");

        confluentBaseUrl = ConfigurationManager.AppSettings["RestProxyBaseUrl"];

        if (string.IsNullOrWhiteSpace(confluentBaseUrl))
          throw new ArgumentNullException("RestProxy Base url is empty");

        confluentBaseUrl = ConfigurationManager.AppSettings["RestProxyBaseUrl"];

        if (string.IsNullOrWhiteSpace(confluentBaseUrl))
          throw new ArgumentNullException("RestProxy Base Url is empty");

        var configSettings = new AppConfigSettings();
        builder.Register(c => new BinaryProducer(kafkaTopicName, MessageFormat.Binary, configSettings)).As<IProducer>().SingleInstance();
        builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

        IContainer container = builder.Build();
        configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
      }
      catch (ArgumentNullException ex)
      {
        Log.IfError(string.Format("Message {0} \n StackTrace {1}", ex.Message, ex.StackTrace));
      }
    }

   private static string GetKafkaEndPointURL(Uri _url, string kafkatopicName)
    {
      try
      {
        string jsonStr;
        using (var wc = new WebClient())
        {
          jsonStr = wc.DownloadString(_url);
        }
        JObject jsonObj = JObject.Parse(jsonStr);
        var token = jsonObj.SelectToken("$.Topics[?(@.Name == '" + kafkatopicName + "')].URL");
        return token.ToString();
      }
      catch (Exception)
      {

      }
      return null;
    }
  }
}