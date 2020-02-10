using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Kafka.Factory.Consumer;
using VSS.Kafka.Factory.Interface;
using VSS.Kafka.Factory.Model;
using VSS.Kafka.Factory.Publisher;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Config;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Config
{
  public class KafkaServicesConfig
  {
    //Initialize Logger
    private static Log4Net Log = new Log4Net(typeof(KafkaServicesConfig));
    public static int InitialWaitingTimeForReceivingResponseInSeconds = int.Parse(System.Configuration.ConfigurationManager.AppSettings["InitialWaitingTimeForReceivingResponseInSeconds"]);
    public static string KafkaUri = System.Configuration.ConfigurationManager.AppSettings["SubscriptionServiceKafkaUri"];
    public static string KafkaDriver = ConfigurationManager.AppSettings["KafkaDriver"];
    public const string MISAKAI = "MISAKAI";
    public const string RPL = "RPL";

    public static MisakaiPublisher MisakaiPublisher;
    public static RPLPublisher RPLPublisher;
    public static IKafkaConsumer CommonConsumer;

    public static Task KafkaConsumer;

    public static void InitializeKafkaProducer(string inputTopic)
    {
      if (KafkaDriver.Equals(RPL))
      {
        InitializeRPLProducer(inputTopic);
      }
      else if (KafkaDriver.Equals(MISAKAI))
      {
        InitializeMisakaiProducer(inputTopic);
      }
    }

    private static void InitializeRPLProducer(string inputTopic)
    {
      // Initialize and start producer
      try
      {
        List<string> UriList = new List<string> { KafkaUri };
        RPLPublisher = new RPLPublisher(inputTopic);
        LogResult.Report(Log, "log_ForInfo", "Kafka Producer Initialized " + "Topic: " + inputTopic);
      }
      catch (Exception ex)
      {
        LogResult.Report(Log, "log_ForError", "An error occured while initializing producer");
        LogResult.Report(Log, "log_ForError", ex.Message);
        LogResult.Report(Log, "log_ForError", ex.StackTrace);
      }
    }

    private static void InitializeMisakaiProducer(string inputTopic)
    {
      // Initialize and start producer
      try
      {
        List<string> UriList = new List<string> { KafkaUri };
        MisakaiPublisher = new VSS.Kafka.Factory.Publisher.MisakaiPublisher(inputTopic, UriList);
        LogResult.Report(Log, "log_ForInfo", "Kafka Producer Initialized " + "Topic: " + inputTopic);
      }
      catch (Exception ex)
      {
        LogResult.Report(Log, "log_ForError", "An error occured while initializing producer");
        LogResult.Report(Log, "log_ForError", ex.Message);
        LogResult.Report(Log, "log_ForError", ex.StackTrace);
      }
    }

    public static void ProduceMessage(string message, string key = "")
    {
      PayloadMessage payloadMessage = new PayloadMessage
      {
        Value = message,
        Key = key
      };
      if (KafkaDriver.Equals(RPL))
      {
        RPLPublisher.Publish(payloadMessage);
      }
      else if (KafkaDriver.Equals(MISAKAI))
      {
        MisakaiPublisher.Publish(payloadMessage);
      }
    }

    public static void DisposePublisher()
    {
      if (KafkaDriver.Equals(RPL))
      {
        //RPLPublisher.Dispose();
        LogResult.Report(Log, "log_ForInfo", "Kafka Publisher Disposed");
      }
    }

    public static void DisposeConsumer()
    {
      if (KafkaDriver.Equals(RPL))
      {
        CommonConsumer.Dispose();
        LogResult.Report(Log, "log_ForInfo", "Kafka Consumer Disposed");
      }
    }

    public static void InitializeKafkaConsumer(IHandler handler)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      ConsumerSettings settings = null;

      settings = new ConsumerSettings()
      {
        GroupName = "SubscriptionServiceAutomation",
        TopicName = SubscriptionServiceConfig.SubscriptionServiceTopic,
        KafkaUri = KafkaUri,
        AutoCommit = true,
        BatchRead = false,
        ReadAsync = true,
        ReadFromEnd = false,
        MaxQueueSize = 50000,
        MaxBatchSize = 2000
      };

      if (KafkaDriver.Equals(RPL))
      {
        CommonConsumer = new RPLConsumer();
      }
      else if (KafkaDriver.Equals(MISAKAI))
      {
        CommonConsumer = new MisakaiConsumer();
      }
      else
        CommonConsumer = new JavaConsumer();

      LogResult.Report(Log, "log_ForInfo", "Kafka Consumer Initialized For " + "Topic: " + SubscriptionServiceConfig.SubscriptionServiceTopic);
      KafkaConsumer = Task.Factory.StartNew(() => CommonConsumer.StartConsuming(handler, settings, cancellationTokenSource.Token));

    }
  }
}
