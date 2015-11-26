using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Newtonsoft.Json.Linq;
using VSP.MasterData.Common.KafkaWrapper;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;

namespace VSP.MasterData.Project.WebAPI
{
  public class AutofacContainer
  {
    public void ApplyDependencyInjection()
    {
      var builder = new ContainerBuilder();
      HttpConfiguration configuration = GlobalConfiguration.Configuration;

        var disvoveryServiceURI = new Uri(ConfigurationManager.AppSettings["DiscoveryURI"]);

      string kafkaUri = null;
      string environment = ConfigurationManager.AppSettings["Environment"];
      string topicName = ConfigurationManager.AppSettings["TopicName"];
      string kafkaTopicName = string.Concat(environment, "-", topicName);//Environment specific topic name
      kafkaUri = GetKafkaEndPointURL(disvoveryServiceURI, kafkaTopicName);

      var uriList = new List<string>();
      if (kafkaUri != null) uriList.Add(kafkaUri);

      builder.Register(c => new ProducerWrapper(kafkaTopicName, uriList)).As<IProducerWrapper>().SingleInstance();
      builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

      IContainer container = builder.Build();
      configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
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