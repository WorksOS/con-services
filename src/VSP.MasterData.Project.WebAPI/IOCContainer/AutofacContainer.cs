using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using java.util;
using java.util.concurrent;
using log4net;
using Newtonsoft.Json.Linq;
using org.apache.kafka.clients.producer;
using VSP.MasterData.Common.Logging;
using VSS.Project.Data;
using VSS.Project.Data.Interfaces;

namespace VSP.MasterData.Project.WebAPI
{
  public interface IProducer 
  {
    Future send(ProducerRecord record);
  }

  public class JavaProducer : KafkaProducer, IProducer 
  {
    public JavaProducer(Properties properties) : base(properties)
    {
    }
  }

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


        var props = new Properties();
        props.put("bootstrap.servers", confluentBaseUrl);
        props.put("acks", "all");
        props.put("retries", "0");
        props.put("batch.size", "16384");
        props.put("linger.ms", "1");
        props.put("buffer.memory", "33554432");
        props.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
        props.put("value.serializer", "org.apache.kafka.common.serialization.StringSerializer");

        var javaProducer = new JavaProducer(props);
        var repository = new MySqlProjectRepository();

        builder.Register(c => javaProducer).As<IProducer>().SingleInstance();
        builder.Register(c => repository).As<IProjectService>().SingleInstance();

        builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

        IContainer container = builder.Build();
        configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
      }
      catch (ArgumentNullException ex)
      {
        Log.IfError(string.Format("Message {0} \n StackTrace {1}", ex.Message, ex.StackTrace));
      }
    }

  }
}