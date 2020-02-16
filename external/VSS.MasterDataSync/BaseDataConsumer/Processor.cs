using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.Client.Common.Classes;
using VSS.Messaging.Common.Interfaces;
using VSS.Messaging.Connector;
using VSS.Messaging.Connector.Classes;
using VSS.Messaging.Connector.Interfaces;

namespace VSS.Messaging.BaseDataConsumer
{
	[ExcludeFromCodeCoverage]
	public class Processor
	{
		public static ILogger logger;
		public static void Main()
		{
			var diContainerBuilder = new ContainerBuilder();
			string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			IConfiguration configuration = new ConfigurationBuilder()
					.AddXmlFile(Path.Combine(currentDirectory, "app.config.xml"))
					.Build();

			//source: define where we are pulling messages/records from
			diContainerBuilder.RegisterType<KafkaSource>().As<ISource>().SingleInstance();

			//the logic and mapping we want between source and destination
			diContainerBuilder.RegisterType<AutoMapperProfile>().As<Profile>().SingleInstance();
			diContainerBuilder.RegisterType<IVssKafkaMessageToIDbTable>().As<IProcessorLogic>().SingleInstance();

			//destination: define where we are putting records 
			diContainerBuilder.RegisterType<SqlDestination>().As<IDestination>().SingleInstance();
			diContainerBuilder.RegisterType<Database>().As<IDatabase>().SingleInstance();
			diContainerBuilder.Register(x => configuration).As<IConfiguration>().SingleInstance();

			string kafkaDriver = configuration["kafkaDriver"];
			if (string.IsNullOrEmpty(kafkaDriver) || kafkaDriver.ToUpper() == "IKVM")
			{
#if NET471
				diContainerBuilder.RegisterType<VSS.Messaging.Client.Ikvm.IkvmConsumer>().As<VSS.Messaging.Client.Common.Interfaces.IConsumer>().SingleInstance();
#endif
			}
			else
			{
				diContainerBuilder.RegisterType<VSS.Messaging.Client.Confluent.ConfluentConsumer>().As<VSS.Messaging.Client.Common.Interfaces.IConsumer>().SingleInstance();
			}
			IContainer container = ProcessorStartup.GetContainerForProcessor(diContainerBuilder);
			logger = container.Resolve<ILogger>();
			ProcessorStartup.StartProcessor(container);
		}
	}
}