using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
///  Settings come from 2 sources:
//    environment variables and 'internal' (appsettings.json)
//   Appsettings will override any environment setting
//   if neither present then we'll use some defaults
/// </summary>

namespace VSS.UnifiedProductivity.Service.Utils
{
    public class KafkaConsumerConfiguration : IConfigurationStore
    {


    private IConfigurationBuilder configBuilder = null;
        private IConfigurationRoot configuration = null;

        public KafkaConsumerConfiguration()
        {
            var builder = configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            try
            {

                builder.SetBasePath(System.AppContext.BaseDirectory) // for appsettings.json location
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            }
            catch (ArgumentException ex)
            {
                
            }
            finally
            {
                configuration = configBuilder.Build();
            }
    }

    public bool Init()
    {
      var log = DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<KafkaConsumerConfiguration>();
      log.LogInformation("KAFKA_URI: " + GetValueString("KAFKA_URI"));
      log.LogInformation("KAFKA_PORT: " + GetValueInt("KAFKA_PORT"));
      log.LogInformation("KAFKA_GROUP_NAME: " + GetValueString("KAFKA_GROUP_NAME"));
      log.LogInformation("KAFKA_STACKSIZE: " + GetValueInt("KAFKA_STACKSIZE"));
      log.LogInformation("KAFKA_AUTO_COMMIT: " +GetValueBool("KAFKA_AUTO_COMMIT"));
      log.LogInformation("KAFKA_OFFSET: " + GetValueString("KAFKA_OFFSET"));
      log.LogInformation("KAFKA_TOPIC_NAME_SUFFIX: " + GetValueString("KAFKA_TOPIC_NAME_SUFFIX"));
      log.LogInformation("KAFKA_POLL_PERIOD: " + GetValueInt("KAFKA_POLL_PERIOD"));
      log.LogInformation("KAFKA_BATCH_SIZE: " + GetValueInt("KAFKA_BATCH_SIZE"));

      if (GetValueString("KAFKA_TOPIC_NAME_SUFFIX") == null
           || GetValueString("KAFKA_URI") == null
           || GetValueInt("KAFKA_PORT") == int.MinValue
           || GetValueString("KAFKA_GROUP_NAME") == null
           || GetValueInt("KAFKA_STACKSIZE") == int.MinValue
           || GetValueBool("KAFKA_AUTO_COMMIT") == null
           || GetValueString("KAFKA_OFFSET") == null
           || GetValueInt("KAFKA_POLL_PERIOD") == int.MinValue
           || GetValueInt("KAFKA_BATCH_SIZE") == int.MinValue
          )
        return false;

      return true;
    }


    public string GetValueString(string key)
    {     
      return configuration[key];
    }

    public int GetValueInt(string key)
    {
      // zero is valid. Returns int.MinValue on error
      int theInt ;
      if ( !int.TryParse(configuration[key], out theInt))
      {
        theInt = -1;
      }
      return theInt;
    }

    public bool? GetValueBool(string key)
    {
      // zero is valid. Returns int.MinValue on error
      bool? theBoolToReturn = null;
      bool theBool;
      if (bool.TryParse(configuration[key], out theBool))
      {
        theBoolToReturn = theBool;
      }
      return theBoolToReturn;
    }

  }
}
