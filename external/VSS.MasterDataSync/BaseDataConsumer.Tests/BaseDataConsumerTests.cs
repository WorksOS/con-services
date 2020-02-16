using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using VSS.AutofacContrib.NSubstitute;
using VSS.Messaging.Client.Common.Interfaces;
using VSS.Messaging.Client.Common.Utilities;
using VSS.Messaging.Common;
using VSS.Messaging.Common.Interfaces;
using VSS.Messaging.Connector;
using VSS.Messaging.Connector.Classes;
using VSS.Messaging.Connector.Interfaces;
using VSS.Messaging.Connector.Processors;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.Source.Objects.Classes.MasterData.Make;
using VSS.Messaging.Source.Objects.Classes.MasterData.ProductFamily;
using VSS.Messaging.Source.Objects.Classes.MasterData.SalesModel;
using Xunit;
using VSS.Messaging.BaseDataConsumer;
using System.Data.SqlClient;

namespace BaseDataConsumer.Tests
{
	public class BaseDataConsumerTests
	{
		private readonly IConfiguration configuration;
		private readonly SourcesToDestinations target;
		private readonly ObjectsByType sourceMessages;
		private readonly IDatabase database;

		public BaseDataConsumerTests()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder()
					.AddXmlFile(Path.Combine(currentDirectory, "app.config.xml"))
					.Build();

			ILoggerFactory loggerFactory = new LoggerFactory();

			var logger = loggerFactory.CreateLogger<ProcessorStartup>();

			database = Substitute.For<IDatabase>();

			var diContainer = new AutoSubstitute(diContainerBuilder =>
			{
				diContainerBuilder.RegisterType<IVssKafkaMessageToIDbTable>().As<IProcessorLogic>().SingleInstance();

				diContainerBuilder.RegisterType<AutoMapperProfile>().As<Profile>().SingleInstance();
				diContainerBuilder.RegisterType<MappingUtility>().As<IMappingUtility>().SingleInstance();

				diContainerBuilder.RegisterType<SqlDestination>().As<IDestination>().SingleInstance();
				diContainerBuilder.Register(x => database).As<IDatabase>().SingleInstance();
				diContainerBuilder.Register(x => configuration).As<IConfiguration>().SingleInstance();
				diContainerBuilder.Register(x => logger).As<ILogger>().SingleInstance();
			});

			var type = typeof(Processor);
			var mockLogger = type.GetField("logger", BindingFlags.Public | BindingFlags.Static);
			mockLogger.SetValue(null, diContainer.Resolve<ILogger>());

			sourceMessages = new ObjectsByType();
			diContainer.Resolve<ISource>().GetMessages(new CancellationToken()).Returns(sourceMessages);

			target = diContainer.Resolve<SourcesToDestinations>();
			target.InstanceForTesting = true;
		}

		[Fact]
		public void ProcessorLogic_KafkaMessagesSent_OnlyValidPassedToDestination()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateSalesModelEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleCreateMakeEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateMakeEventKafkaMessage));

			database.NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>()).Returns(x =>
			{
				return 0;
			});

			target.Start();

			database.Received(1).NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.Received(1).InsertOrUpdate(Arg.Any<SqlCommand>());
			database.DidNotReceive().ExecuteProcedure(Arg.Any<DataTable>(), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void ProcessorLogic_MalformedKafkaMessages_NotPassedToDestination()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateSalesModelEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleUpdateProductFamilyEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateMakeEventKafkaMessage));

			target.Start();

			database.DidNotReceive().NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.DidNotReceive().InsertOrUpdate(Arg.Any<SqlCommand>());
			database.DidNotReceive().ExecuteProcedure(Arg.Any<DataTable>(), Arg.Any<DataTable>(), Arg.Any<string>());
		}


		[Fact]
		public void ProcessorLogic_Make_InsertOnlyOnNoOfRecordsAlreadyExistsIsZero()
		{
			sourceMessages.AllObjects.Clear();
			var msgToSave = JsonConvert.DeserializeObject<MakeEvent>(exampleCreateMakeEventKafkaMessage);
			sourceMessages.AddObject(msgToSave);

			database.NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>()).Returns(x =>
			{
				return 0;
			});

			target.Start();

			database.Received(1).NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.Received(1).InsertOrUpdate(Arg.Any<SqlCommand>());
		}

		[Fact]
		public void ProcessorLogic_Make_NoInsertOrUpdateIfNoOfRecordsAlreadyExistsIsMoreThanOne()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleCreateMakeEventKafkaMessage));

			database.NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>()).Returns(x =>
			{
				return 2;
			});

			target.Start();

			database.Received(1).NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.DidNotReceive().InsertOrUpdate(Arg.Any<SqlCommand>());
		}

		[Fact]
		public void ProcessorLogic_Make_UpdateOnlyOnNoOfRecordsAlreadyExistsIsOne()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleCreateMakeEventKafkaMessage));

			database.NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>()).Returns(x =>
			{
				return 1;
			});

			target.Start();

			database.Received(1).NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.Received(1).InsertOrUpdate(Arg.Any<SqlCommand>());
		}

		[Fact]
		public void ProcessorLogic_ProductFamily_MsgPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			var msgToSave = JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateProductFamilyEventKafkaMessage);
			sourceMessages.AddObject(msgToSave);

			target.Start();

			database.Received(1).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void ProcessorLogic_SalesModel_MsgPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			var msgToSave = JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateSalesModelEventKafkaMessage);
			sourceMessages.AddObject(msgToSave);

			target.Start();

			database.Received(1).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void ProcessorLogic_SalesModel_DeleteMsgPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleDeleteSalesModelEventKafkaMessage));

			target.Start();

			database.Received(1).ExecuteProcedure(Arg.Any<DataTable>(), Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<string>());
		}

		[Fact]
		public void PersistLatestMessagesOfSameProductFamilyUIDs_ProductFamily_MsgPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			var msgToSave = JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleUpdateProductFamilyEventKafkaMessage);
			msgToSave.UpdateProductFamilyEvent.ProductFamilyName = "NEW Product Family";
			msgToSave.UpdateProductFamilyEvent.ProductFamilyUID = new Guid("6e881114-7c54-11e9-8f9e-2a86e4085a59");
			msgToSave.UpdateProductFamilyEvent.ReceivedUtc = DateTime.UtcNow;
			sourceMessages.AddObject(msgToSave);
			msgToSave = JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateProductFamilyEventKafkaMessage);
			msgToSave.CreateProductFamilyEvent.ProductFamilyName = "OLD Product Family";
			msgToSave.CreateProductFamilyEvent.ProductFamilyUID = new Guid("6e881114-7c54-11e9-8f9e-2a86e4085a59");
			msgToSave.CreateProductFamilyEvent.ReceivedUtc = DateTime.UtcNow.AddDays(-1);
			sourceMessages.AddObject(msgToSave);

			target.Start();

			database.Received(1).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void PersistLatestMessagesOfSameSalesModelUIDs_SalesModel_MsgPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			var msgToSave = JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateSalesModelEventKafkaMessage);
			msgToSave.CreateSalesModelEvent.SalesModelCode = "OLD code";
			msgToSave.CreateSalesModelEvent.SalesModelUID = new Guid("6e881114-7c54-11e9-8f9e-2a86e4085a59");
			msgToSave.CreateSalesModelEvent.ReceivedUtc = DateTime.UtcNow.AddDays(-1);
			sourceMessages.AddObject(msgToSave);
			msgToSave = JsonConvert.DeserializeObject<SalesModelEvent>(exampleUpdateSalesModelEventKafkaMessage);
			msgToSave.UpdateSalesModelEvent.SalesModelCode = "LATEST code";
			msgToSave.UpdateSalesModelEvent.SalesModelUID = new Guid("6e881114-7c54-11e9-8f9e-2a86e4085a59");
			msgToSave.UpdateSalesModelEvent.ReceivedUtc = DateTime.UtcNow;
			sourceMessages.AddObject(msgToSave);

			DataTable resultTable = new DataTable();
			resultTable.Columns.Add(new DataColumn() { ColumnName = "ModelCode", DataType = typeof(string) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "SerialNumberPrefix", DataType = typeof(string) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "StartRange", DataType = typeof(long) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "EndRange", DataType = typeof(long) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "Description", DataType = typeof(string) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "IconUID", DataType = typeof(Guid) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "ProductFamilyUID", DataType = typeof(Guid) });
			resultTable.Columns.Add(new DataColumn() { ColumnName = "SalesModelUID", DataType = typeof(Guid) });
			DataRow expectedRow = resultTable.NewRow();
			expectedRow["ModelCode"] = "LATEST code";
			expectedRow["SerialNumberPrefix"] = "TH86";
			expectedRow["StartRange"] = "1";
			expectedRow["EndRange"] = "9999";
			expectedRow["Description"] = "TH86";
			expectedRow["IconUID"] = "34373838-6264-3264-2d63-3130322d3131";
			expectedRow["ProductFamilyUID"] = "30323134-6233-3435-2d63-3130342d3131";
			expectedRow["SalesModelUID"] = "6e881114-7c54-11e9-8f9e-2a86e4085a59";

			target.Start();

			database.Received(1).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<DataTable>(), Arg.Any<string>());
			database.Received(1).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows[0].ItemArray.SequenceEqual(expectedRow.ItemArray)), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void ProcessorLogic_TwoEvents_MsgsPassedToStoreProc()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateSalesModelEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateProductFamilyEventKafkaMessage));

			target.Start();

			database.Received(2).ExecuteProcedure(Arg.Is<DataTable>(x => x.Rows.Count == 1), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		[Fact]
		public void ProcessorLogic_ThreeEvents_MsgsPassedToDestination()
		{
			sourceMessages.AllObjects.Clear();
			sourceMessages.AddObject(JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleUpdateProductFamilyEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleCreateMakeEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleCreateSalesModelEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<ProductFamilyEvent>(exampleCreateProductFamilyEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<MakeEvent>(exampleUpdateMakeEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleUpdateSalesModelEventKafkaMessage));
			sourceMessages.AddObject(JsonConvert.DeserializeObject<SalesModelEvent>(exampleDeleteSalesModelEventKafkaMessage));

			database.NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>()).Returns(x =>
			{
				return 0;
			});

			target.Start();

			database.Received(2).NoOfRecordsAlreadyExists(Arg.Any<SqlCommand>());
			database.Received(2).InsertOrUpdate(Arg.Any<SqlCommand>());
			database.Received(2).ExecuteProcedure(Arg.Any<DataTable>(), Arg.Any<DataTable>(), Arg.Any<string>());
		}

		#region ExampleKafkaMessages
		private readonly string exampleCreateMakeEventKafkaMessage = @"{
																	""CreateMakeEvent"": {
																	""MakeCode"": ""A09"",
																	""MakeDesc"": ""AMZ"",
																	""MakeUID"": ""30323535-6461-3838-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-15T13:45:30.1830238Z"",
																	""ReceivedUTC"": ""2019-05-15T13:45:30.1830238Z""
																	}
																}";

		private readonly string exampleUpdateMakeEventKafkaMessage = @"{
																	""UpdateMakeEvent"": {
																	""MakeCode"": ""21A09"",
																	""MakeDesc"": ""A12MZ"",
																	""MakeUID"": ""0790880c-78c2-49b6-bef7-61fd99695d5c"",
																	""ActionUTC"": ""2019-05-14T13:27:30.1830238Z"",
																	""ReceivedUTC"": ""2019-05-14T13:27:30.1830238Z""
																	}
																}";

		private readonly string exampleCreateProductFamilyEventKafkaMessage = @"{
																	""CreateProductFamilyEvent"": {
																	""ProductFamilyName"": ""IND"",
																	""ProductFamilyDesc"": ""INDUSTRIAL ENGINE"",
																	""ProductFamilyUID"": ""30323138-3961-3032-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-15T13:46:20.8630468Z"",
																	""ReceivedUTC"": ""2019-05-15T13:46:20.8630468Z""
																	}
																}";

		private readonly string exampleUpdateProductFamilyEventKafkaMessage = @"{
																	""UpdateProductFamilyEvent"": {
																	""ProductFamilyName"": ""FPSM"",
																	""ProductFamilyDesc"": ""FOREST PRODUCT SWING MACHINE"",
																	""ProductFamilyUID"": ""30323130-3038-6366-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-16T21:51:34.3087401Z"",
																	""ReceivedUTC"": ""2019-05-16T21:51:34.3087401Z""
																	}
																}";

		private readonly string exampleCreateSalesModelEventKafkaMessage = @" {
																	""CreateSalesModelEvent"": {
																	""SalesModelCode"": ""1TH86"",
																	""SalesModelDescription"": ""1TH86"",
																	""SerialNumberPrefix"": ""1TH86"",
																	""StartRange"": 1,
																	""EndRange"": 999,
																	""IconUID"": ""34373838-6264-3264-2d63-3130322d3131"",
																	""ProductFamilyUID"": ""30323134-6233-3435-2d63-3130342d3131"",
																	""SalesModelUID"": ""30323533-3839-3861-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-15T09:48:25.898791Z"",
																	""ReceivedUTC"": ""2019-05-15T09:48:25.898791Z""
																	}
																}";
		private readonly string exampleUpdateSalesModelEventKafkaMessage = @" {
																	""UpdateSalesModelEvent"": {
																	""SalesModelCode"": ""TH86"",
																	""SalesModelDescription"": ""TH86"",
																	""SerialNumberPrefix"": ""TH86"",
																	""StartRange"": 1,
																	""EndRange"": 9999,
																	""IconUID"": ""34373838-6264-3264-2d63-3130322d3131"",
																	""ProductFamilyUID"": ""30323134-6233-3435-2d63-3130342d3131"",
																	""SalesModelUID"": ""30323533-3839-3861-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-16T09:48:25.898791Z"",
																	""ReceivedUTC"": ""2019-05-16T09:48:25.898791Z""
																	}
																}";

		private readonly string exampleDeleteSalesModelEventKafkaMessage = @" {
																	""DeleteSalesModelEvent"": {
																	""SalesModelUID"": ""30323533-3839-3861-2d63-3130342d3131"",
																	""ActionUTC"": ""2019-05-16T09:48:25.898791Z"",
																	""ReceivedUTC"": ""2019-05-16T09:48:25.898791Z""
																	}
																}";
		#endregion
	}
}