using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Subscription.Controllers.V1;
using VSS.MasterData.WebAPI.Utilities.Enums;
using Xunit;

namespace VSS.MasterData.WebAPI.Subscription.Tests
{
	public class SubscriptionTests
	{
		private readonly ISubscriptionService subscriptionService;
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private SubscriptionV1Controller controller;

		public SubscriptionTests()
		{
			logger = Substitute.For<ILogger>();
			subscriptionService = Substitute.For<ISubscriptionService>();
			configuration = Substitute.For<IConfiguration>();
			controller = new SubscriptionV1Controller(subscriptionService, configuration, logger);
		}

		#region BulkAssetSubscription
		[Fact]
		public void TestMultipleCreateAssetSubscriptions_ValidInput_Success()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			subscriptionService.CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents).Returns(true);

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_InvalidSource_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupInvalidRequest();

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid source value for the SubscriptionUID:- " + Event.CreateAssetSubscriptionEvents[1].SubscriptionUID, ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_DeviceUIDNULL_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			Event.CreateAssetSubscriptionEvents[0].DeviceUID = null;

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Provide DeviceUID for the SubscriptionUID " + Event.CreateAssetSubscriptionEvents[0].SubscriptionUID, ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>());
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_ThresholdLimit_BadRequest()

		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			configuration["BulkPayloadMaxCount"] = "1";
			controller = new SubscriptionV1Controller(subscriptionService, configuration, logger);

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Bulk CreateAssetSubscription payload count exceeds the threshold limit.", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			Event.CreateAssetSubscriptionEvents[0].SubscriptionType = null;
			subscriptionService.When(x => x.CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>())).Do(x => { throw new Exception("Invalid Asset Subscription Type"); });

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Asset Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>());
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_TransactionFails_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			subscriptionService.CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents).Returns(false);

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated.", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateAssetSubscriptions(Event.CreateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleCreateAssetSubscriptions_Exception_ReturnsServerError()
		{
			//Arrange
			CreateAssetSubscriptions Event = SetupRequest();
			subscriptionService.When(x => x.CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.CreateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>());
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_ValidInput_Success()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			subscriptionService.UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents).Returns(true);

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_InvalidSource_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupInvalidUpdateRequest();
			subscriptionService.UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents).Returns(true);

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid source value for the SubscriptionUID:-" + Event.UpdateAssetSubscriptionEvents[1].SubscriptionUID, ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_MandatoryFieldsMissing_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			Event.UpdateAssetSubscriptionEvents[0].DeviceUID = null;
			Event.UpdateAssetSubscriptionEvents[0].CustomerUID = null;
			Event.UpdateAssetSubscriptionEvents[0].AssetUID = null;
			Event.UpdateAssetSubscriptionEvents[0].StartDate = null;
			Event.UpdateAssetSubscriptionEvents[0].EndDate = null;

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("All Fields are Mandatory for the SubscriptionUID " + Event.UpdateAssetSubscriptionEvents[0].SubscriptionUID, ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscriptions(Arg.Any<List<CreateAssetSubscriptionEvent>>());
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_ThresholdLimit_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			configuration["BulkPayloadMaxCount"] = "1";
			controller = new SubscriptionV1Controller(subscriptionService, configuration, logger);

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Bulk UpdateAssetSubscription payload count exceeds the threshold limit.", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			Event.UpdateAssetSubscriptionEvents[0].SubscriptionType = null;
			subscriptionService.When(x => x.UpdateAssetSubscriptions(Arg.Any<List<UpdateAssetSubscriptionEvent>>())).Do(x => { throw new Exception("Invalid Asset Subscription Type"); });

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Asset Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateAssetSubscriptions(Arg.Any<List<UpdateAssetSubscriptionEvent>>());
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_TransactionFails_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			subscriptionService.UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents).Returns(false);

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateAssetSubscriptions(Event.UpdateAssetSubscriptionEvents);
		}

		[Fact]
		public void TestMultipleUpdateAssetSubscriptions_Exception_ReturnsServerError()
		{
			//Arrange
			UpdateAssetSubscriptions Event = SetupUpdateRequest();
			subscriptionService.When(x => x.UpdateAssetSubscriptions(Arg.Any<List<UpdateAssetSubscriptionEvent>>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.UpdateAssetSubscriptions(Event);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).UpdateAssetSubscriptions(Arg.Any<List<UpdateAssetSubscriptionEvent>>());
		}
		[Fact]

		#endregion

		#region Private Methods
		private static CreateAssetSubscriptions SetupRequest()
		{
			CreateAssetSubscriptions obj = new CreateAssetSubscriptions
			{
				CreateAssetSubscriptionEvents = new List<CreateAssetSubscriptionEvent>{ new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow },
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }}
			};

			return obj;
		}

		private static CreateAssetSubscriptions SetupInvalidRequest()
		{
			CreateAssetSubscriptions obj = new CreateAssetSubscriptions
			{
				CreateAssetSubscriptionEvents = new List<CreateAssetSubscriptionEvent>{ new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new CreateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = "InvalidString",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }
		   }
			};

			return obj;
		}

		private static UpdateAssetSubscriptions SetupUpdateRequest()
		{
			UpdateAssetSubscriptions obj = new UpdateAssetSubscriptions
			{
				UpdateAssetSubscriptionEvents = new List<UpdateAssetSubscriptionEvent>{ new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow },
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }}
			};

			return obj;
		}

		private static UpdateAssetSubscriptions SetupInvalidUpdateRequest()
		{
			UpdateAssetSubscriptions obj = new UpdateAssetSubscriptions
			{
				UpdateAssetSubscriptionEvents = new List<UpdateAssetSubscriptionEvent>{
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow } ,
			new UpdateAssetSubscriptionEvent() { SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = "InvalidString",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow }
		   }
			};

			return obj;
		}
		#endregion

		#region Create AssetSubscription
		[Fact]
		public void TestCreateAssetSubscription_ValidInput_Success()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateAssetSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).CreateAssetSubscription(Arg.Any<CreateAssetSubscriptionEvent>());
		}

		[Theory]
		[InlineData("Store")]
		[InlineData("None")]
		[InlineData("SAV")]
		[InlineData("")]
		[InlineData(null)]
		public void TestCreateAssetSubscription_ValidSubscriptionSource_Success(string source)
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = source,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateAssetSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).CreateAssetSubscription(Arg.Any<CreateAssetSubscriptionEvent>());
		}

		[Fact]
		public void TestCreateAssetSubscription_InvalidSubscriptionSource_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = "Invalid Source",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateAssetSubscription(input).Returns(true);

			//Act
			var result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Source Value", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscription(input);
		}

		[Fact]
		public void TestCreateAssetSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>()).Returns(false);
			subscriptionService.CreateAssetSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>());
			subscriptionService.Received(1).CreateAssetSubscription(input);
		}

		[Fact]
		public void TestCreateAssetSubscription_SubscriptionExist_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>()).Returns(true);

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Asset Subscription already exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateAssetSubscription(input);
			subscriptionService.Received(1).CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>());
		}

		[Fact]
		public void TestCreateAssetSubscription_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateAssetSubscription(Arg.Any<CreateAssetSubscriptionEvent>())).Do(x => { throw new Exception("Invalid Asset Subscription Type"); });

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Asset Subscription Type", ((BadRequestObjectResult)result).Value);
		}

		[Fact]
		public void TestCreateAssetSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			CreateAssetSubscriptionEvent input = new CreateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateAssetSubscription(Arg.Any<CreateAssetSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.CreateAssetSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
		}
		#endregion

		#region Update AssetSubscription
		[Fact]
		public void TestUpdateAssetSubscription_ValidInput_Success()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.SAV.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateAssetSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).UpdateAssetSubscription(input);
		}

		[Theory]
		[InlineData("Store")]
		[InlineData("None")]
		[InlineData("SAV")]
		[InlineData("")]
		[InlineData(null)]
		public void TestUpdateAssetSubscription_ValidSubscriptionSource_Success(string source)
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = source,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>()).Returns(true);

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).UpdateAssetSubscription(input);
		}

		[Fact]
		public void TestUpdateAssetSubscription_SubscriptionNotExist_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>())).Do(x => { throw new Exception("Asset Subscription not exists!"); });

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Asset Subscription not exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateAssetSubscription(input);
		}

		[Fact]
		public void TestUpdateAssetSubscription_InvalidSubscriptionSource_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = "Invalid Source",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>()).Returns(true);

			//Act
			var result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Source Value", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).UpdateAssetSubscription(input);
		}

		[Fact]
		public void TestUpdateAssetSubscription_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>())).Do(x => { throw new Exception("Invalid Asset Subscription Type"); });

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Asset Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateAssetSubscription(input);
		}

		[Fact]
		public void TestUpdateAssetSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				SubscriptionType = "Essentials",
				Source = SubscriptionSource.Store.ToString(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateAssetSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Update Asset Subscription Request Failed", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateAssetSubscription(input);
		}

		[Fact]
		public void TestUpdateAssetSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			UpdateAssetSubscriptionEvent input = new UpdateAssetSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				AssetUID = Guid.NewGuid(),
				DeviceUID = Guid.NewGuid(),
				Source = SubscriptionSource.SAV.ToString(),
				SubscriptionType = "Essentials",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.UpdateAssetSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).UpdateAssetSubscription(Arg.Any<UpdateAssetSubscriptionEvent>());
		}
		#endregion

		#region Customer Subscription
		[Fact]
		public void TestCreateCustomerSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateCustomerSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.CreateCustomerSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).CreateCustomerSubscription(input);
		}

		[Fact]
		public void TestCreateCustomerSubscription_Exception_BadRequest()
		{
			//Arrange
			var input = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateCustomerSubscription(Arg.Any<CreateCustomerSubscriptionEvent>())).Do(x => { throw new Exception("Invalid Customer Subscription Type"); });

			//Act
			ActionResult result = controller.CreateCustomerSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Customer Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateCustomerSubscription(input);
		}

		[Fact]
		public void TestCreateCustomerSubscription_SubscriptionExist_BadRequest()
		{
			//Arrange
			var input = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>()).Returns(true);

			//Act
			ActionResult result = controller.CreateCustomerSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Customer Subscription already exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateCustomerSubscription(input);
		}

		[Fact]
		public void TestCreateCustomerSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateCustomerSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.CreateCustomerSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateCustomerSubscription(input);
		}

		[Fact]
		public void TestCreateCustomerSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new CreateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Manual 3D Monitoring",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateCustomerSubscription(Arg.Any<CreateCustomerSubscriptionEvent>())).Do(x => { throw new Exception(); });
			subscriptionService.CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>()).Returns(false);

			//Act
			ActionResult result = controller.CreateCustomerSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).CreateCustomerSubscription(Arg.Any<CreateCustomerSubscriptionEvent>());
		}

		[Fact]
		public void TestUpdateCustomerSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateCustomerSubscription(Arg.Any<UpdateCustomerSubscriptionEvent>()).Returns(true);

			//Act
			ActionResult result = controller.UpdateCustomerSubscription(input);

			//Assert
			Assert.Equal(typeof(OkResult), result.GetType());
			subscriptionService.Received(1).UpdateCustomerSubscription(Arg.Any<UpdateCustomerSubscriptionEvent>());
		}

		[Fact]
		public void TestUpdateCustomerSubscription_SubscriptionNotExist_BadRequest()
		{
			//Arrange
			var input = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateCustomerSubscription(Arg.Any<UpdateCustomerSubscriptionEvent>())).Do(x => { throw new Exception("Customer Subscription not exists!"); });

			//Act
			ActionResult result = controller.UpdateCustomerSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Customer Subscription not exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateCustomerSubscription(input);
		}

		[Fact]
		public void TestUpdateCustomerSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateCustomerSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.UpdateCustomerSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateCustomerSubscription(input);
		}

		[Fact]
		public void TestUpdateCustomerSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new UpdateCustomerSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateCustomerSubscription(Arg.Any<UpdateCustomerSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.UpdateCustomerSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).UpdateCustomerSubscription(Arg.Any<UpdateCustomerSubscriptionEvent>());
		}
		#endregion

		#region Project Subscription
		[Fact]
		public void TestCreateProjectSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateProjectSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.CreateProjectSubscription(input);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).CreateProjectSubscription(input);
		}

		[Fact]
		public void TestCreateProjectSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateProjectSubscription(Arg.Any<CreateProjectSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.CreateProjectSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).CreateProjectSubscription(Arg.Any<CreateProjectSubscriptionEvent>());
		}

		[Fact]
		public void TestCreateProjectSubscription_SubscriptionExist_BadRequest()
		{
			//Arrange
			var input = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CheckExistingSubscription(Arg.Any<Guid>(), Arg.Any<string>()).Returns(true);

			//Act
			ActionResult result = controller.CreateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Project Subscription already exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(0).CreateProjectSubscription(input);
		}

		[Fact]
		public void TestCreateProjectSubscription_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			var input = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.CreateProjectSubscription(Arg.Any<CreateProjectSubscriptionEvent>())).Do(x => { throw new Exception("Invalid Project Subscription Type"); });

			//Act
			ActionResult result = controller.CreateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Project Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateProjectSubscription(input);
		}

		[Fact]
		public void TestCreateProjectSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new CreateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.CreateProjectSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.CreateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).CreateProjectSubscription(input);
		}

		[Fact]
		public void TestUpdateProjectSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateProjectSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.UpdateProjectSubscription(input);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).UpdateProjectSubscription(input);
		}

		[Fact]
		public void TestUpdateProjectSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateProjectSubscription(Arg.Any<UpdateProjectSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.UpdateProjectSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).UpdateProjectSubscription(Arg.Any<UpdateProjectSubscriptionEvent>());
		}

		[Fact]
		public void TestUpdateProjectSubscription_SubscriptionNotExist_BadRequest()
		{
			//Arrange
			var input = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateProjectSubscription(Arg.Any<UpdateProjectSubscriptionEvent>())).Do(x => { throw new Exception("Project Subscription not exists!"); });

			//Act
			ActionResult result = controller.UpdateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Project Subscription not exists!", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateProjectSubscription(input);
		}

		[Fact]
		public void TestUpdateProjectSubscription_InvalidSubscriptionType_BadRequest()
		{
			//Arrange
			var input = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.UpdateProjectSubscription(Arg.Any<UpdateProjectSubscriptionEvent>())).Do(x => { throw new Exception("Invalid Project Subscription Type"); });

			//Act
			ActionResult result = controller.UpdateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid Project Subscription Type", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateProjectSubscription(input);
		}

		[Fact]
		public void TestUpdateProjectSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new UpdateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				CustomerUID = Guid.NewGuid(),
				SubscriptionType = "Landfill",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.UpdateProjectSubscription(input).Returns(false);

			//Act
			ActionResult result = controller.UpdateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("SubscriptionUID not Found in Db.Make Sure Create Subscription request comes before Update Subscription", ((BadRequestObjectResult)result).Value);
			subscriptionService.Received(1).UpdateProjectSubscription(input);
		}
		#endregion

		#region Associate Project
		[Fact]
		public void TestAssociateProjectSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.AssociateProjectSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.AssociateProjectSubscription(input);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).AssociateProjectSubscription(input);
		}

		[Fact]
		public void TestAssociateProjectSubscription_InvalidSubscriptionUID_BadRequest()
		{
			//Arrange
			var input = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.AssociateProjectSubscription(Arg.Any<AssociateProjectSubscriptionEvent>())).Do(x => { throw new Exception("Invalid ProjectSubscriptionUID"); });

			//Act
			ActionResult result = controller.AssociateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid ProjectSubscriptionUID", ((BadRequestObjectResult)(result)).Value);
			subscriptionService.Received(1).AssociateProjectSubscription(input);
		}

		[Fact]
		public void TestAssociateProjectSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.AssociateProjectSubscription(Arg.Any<AssociateProjectSubscriptionEvent>()).Returns(false);

			//Act
			ActionResult result = controller.AssociateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)(result)).Value);
			subscriptionService.Received(1).AssociateProjectSubscription(input);
		}

		[Fact]
		public void TestAssociateProjectSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new AssociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.AssociateProjectSubscription(Arg.Any<AssociateProjectSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.AssociateProjectSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).AssociateProjectSubscription(input);
		}

		#endregion

		#region Dissociate Project
		[Fact]
		public void TestDissociateProjectSubscription_ValidInput_Success()
		{
			//Arrange
			var input = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.DissociateProjectSubscription(input).Returns(true);

			//Act
			ActionResult result = controller.DissociateProjectSubscription(input);

			//Assert
			Assert.IsType<OkResult>(result);
			subscriptionService.Received(1).DissociateProjectSubscription(input);
		}

		[Fact]
		public void TestDissociateProjectSubscription_InvalidSubscriptionUID_BadRequest()
		{
			//Arrange
			var input = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.DissociateProjectSubscription(Arg.Any<DissociateProjectSubscriptionEvent>())).Do(x => { throw new Exception("Invalid ProjectSubscriptionUID"); });

			//Act
			ActionResult result = controller.DissociateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid ProjectSubscriptionUID", ((BadRequestObjectResult)(result)).Value);
			subscriptionService.Received(1).DissociateProjectSubscription(input);
		}

		[Fact]
		public void TestDissociateProjectSubscription_TransactionFails_BadRequest()
		{
			//Arrange
			var input = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			//Act
			subscriptionService.DissociateProjectSubscription(Arg.Any<DissociateProjectSubscriptionEvent>()).Returns(false);
			ActionResult result = controller.DissociateProjectSubscription(input);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)(result)).Value);
			subscriptionService.Received(1).DissociateProjectSubscription(input);
		}

		[Fact]
		public void TestDissociateProjectSubscription_Exception_ReturnsServerError()
		{
			//Arrange
			var input = new DissociateProjectSubscriptionEvent
			{
				SubscriptionUID = Guid.NewGuid(),
				ProjectUID = Guid.NewGuid(),
				ActionUTC = DateTime.UtcNow,
				ReceivedUTC = DateTime.UtcNow
			};

			subscriptionService.When(x => x.DissociateProjectSubscription(Arg.Any<DissociateProjectSubscriptionEvent>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.DissociateProjectSubscription(input);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			subscriptionService.Received(1).DissociateProjectSubscription(input);
		}

		#endregion

		#region Get Subscription
		[Fact]
		public void TestGetSubscriptionByCustomerId_Valid_GivesOk()
		{
			//Arrange
			CustomerSubscriptionList customerSubscriptionList = new CustomerSubscriptionList();
			customerSubscriptionList.Subscriptions = new List<CustomerSubscriptionModel>() {
				new CustomerSubscriptionModel { SubscriptionType = "Essentials", StartDate = DateTime.Now, EndDate = DateTime.Now }
			};

			subscriptionService.GetSubscriptionForCustomer(Arg.Any<Guid>()).Returns(customerSubscriptionList.Subscriptions);

			//Act
			ActionResult response = controller.GetSubscriptionByCustomerId(Guid.NewGuid());

			//Assert
			Assert.IsType<OkObjectResult>(response);
			Assert.Equal(customerSubscriptionList.Subscriptions, ((CustomerSubscriptionList)((OkObjectResult)response).Value).Subscriptions);
			subscriptionService.Received(1).GetSubscriptionForCustomer(Arg.Any<Guid>());
		}

		[Fact]
		public void TestGetSubscriptionByCustomerId_EmptyGuid_GivesBadRequest()
		{
			//Act
			ActionResult response = controller.GetSubscriptionByCustomerId(Guid.Empty);

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public void TestGetSubscriptionByCustomerId_Exception_GivesBadRequest()
		{
			//Arrange
			subscriptionService.When(x => x.GetSubscriptionForCustomer(Arg.Any<Guid>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult response = controller.GetSubscriptionByCustomerId(Guid.NewGuid());

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public void TestGetActiveSubscriptionByCustomerId_Valid_GivesOk()
		{
			//Arrange
			ActiveProjectCustomerSubscriptionList activeProjectCustomerSubscriptionList = new ActiveProjectCustomerSubscriptionList();
			activeProjectCustomerSubscriptionList.Subscriptions = new List<ActiveProjectCustomerSubscriptionModel>() {
				new ActiveProjectCustomerSubscriptionModel { SubscriptionType = "Essentials", StartDate = DateTime.Now, EndDate = DateTime.Now, SubscriptionGuid = Guid.NewGuid() }
			};

			subscriptionService.GetActiveProjectSubscriptionForCustomer(Arg.Any<Guid>()).Returns(activeProjectCustomerSubscriptionList.Subscriptions);

			//Act
			ActionResult response = controller.GetActiveProjectSubscriptionByCustomerId(Guid.NewGuid());

			//Assert
			Assert.IsType<OkObjectResult>(response);
			Assert.Equal(activeProjectCustomerSubscriptionList.Subscriptions, ((ActiveProjectCustomerSubscriptionList)((OkObjectResult)response).Value).Subscriptions);
			subscriptionService.Received(1).GetActiveProjectSubscriptionForCustomer(Arg.Any<Guid>());
		}

		[Fact]
		public void TestGetActiveSubscriptionByCustomerId_EmptyGuid_GivesBadRequest()
		{
			//Act
			ActionResult response = controller.GetActiveProjectSubscriptionByCustomerId(Guid.Empty);

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public void TestGetActiveSubscriptionByCustomerId_Exception_GivesBadRequest()
		{
			//Arrange
			subscriptionService.When(x => x.GetActiveProjectSubscriptionForCustomer(Arg.Any<Guid>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult response = controller.GetActiveProjectSubscriptionByCustomerId(Guid.NewGuid());

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public void TestGetSubscriptionByAssetId_GivesOk()
		{
			//Arrange
			AssetSubscriptionModel assetSubscription = new AssetSubscriptionModel { SubscriptionStatus = "Active", AssetUID = Guid.NewGuid(), OwnersVisibility = new List<OwnerVisibility>() };
			subscriptionService.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(assetSubscription);

			//Act
			ActionResult response = controller.GetSubscriptionByAssetId(Guid.NewGuid());

			//Assert
			Assert.IsType<OkObjectResult>(response);
			Assert.Equal(assetSubscription.AssetUID, ((AssetSubscriptionModel)((OkObjectResult)response).Value).AssetUID);
			Assert.Equal(assetSubscription.SubscriptionStatus, ((AssetSubscriptionModel)((OkObjectResult)response).Value).SubscriptionStatus);
			subscriptionService.Received(1).GetSubscriptionForAsset(Arg.Any<Guid>());
		}

		[Fact]
		public void TestGetSubscriptionByAssetId_NoCustomerDetails_GivesOk()
		{
			//Arrange
			List<OwnerVisibility> ownerList = new List<OwnerVisibility>
			{
				new OwnerVisibility()
				{
					CustomerUID = Guid.NewGuid(),
					SubscriptionEndDate = DateTime.MaxValue,
					SubscriptionStartDate = DateTime.Now,
					SubscriptionName = "Essentials",
					SubscriptionStatus = "Active",
					SubscriptionUID = Guid.NewGuid()
				}
			};
			AssetSubscriptionModel assetSubscription = new AssetSubscriptionModel { SubscriptionStatus = "Active", AssetUID = Guid.NewGuid(), OwnersVisibility = ownerList };
			subscriptionService.GetSubscriptionForAsset(Arg.Any<Guid>()).Returns(assetSubscription);

			//Act
			ActionResult response = controller.GetSubscriptionByAssetId(Guid.NewGuid());

			//Assert
			Assert.IsType<OkObjectResult>(response);
			Assert.Equal(assetSubscription.AssetUID, ((AssetSubscriptionModel)((OkObjectResult)response).Value).AssetUID);
			Assert.Equal(assetSubscription.SubscriptionStatus, ((AssetSubscriptionModel)((OkObjectResult)response).Value).SubscriptionStatus);
			Assert.Equal(assetSubscription.OwnersVisibility, ((AssetSubscriptionModel)((OkObjectResult)response).Value).OwnersVisibility);
			subscriptionService.Received(1).GetSubscriptionForAsset(Arg.Any<Guid>());
		}

		[Fact]
		public void TestGetSubscriptionByAssetId_EmptyGuid_GivesBadRequest()
		{
			//Act
			ActionResult response = controller.GetSubscriptionByAssetId(Guid.Empty);

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}

		[Fact]
		public void TestGetSubscriptionByAssetId_Exception_GivesBadRequest()
		{
			//Act
			subscriptionService.When(x => x.GetSubscriptionForAsset(Arg.Any<Guid>())).Do(x => { throw new Exception(); });

			//Act
			ActionResult response = controller.GetSubscriptionByAssetId(Guid.NewGuid());

			//Assert
			Assert.IsType<BadRequestObjectResult>(response);
		}
		#endregion

		#region DateTimeOverflowCorrection
		[Fact]
		public void TestDateTimeOverflowCorrection_Success()
		{
			//Arrange
			CreateAssetSubscriptionEvent createAssetSubscriptionEvent = JsonConvert.DeserializeObject<CreateAssetSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"AssetUID\":\"5b811c51-6156-4547-acb5-031ddda1a1ff\",\"SubscriptionType\":\"Essentials\",\"StartDate\":\"2019-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"9999-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			UpdateAssetSubscriptionEvent updateAssetSubscriptionEvent = JsonConvert.DeserializeObject<UpdateAssetSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"AssetUID\":\"5b811c51-6156-4547-acb5-031ddda1a1ff\",\"SubscriptionType\":\"Essentials\",\"StartDate\":\"9999-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"2019-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			CreateCustomerSubscriptionEvent createCustomerSubscriptionEvent = JsonConvert.DeserializeObject<CreateCustomerSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"2019-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"9999-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			UpdateCustomerSubscriptionEvent updateCustomerSubscriptionEvent = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"9999-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"2019-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			CreateProjectSubscriptionEvent createProjectSubscriptionEvent = JsonConvert.DeserializeObject<CreateProjectSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"2019-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"9999-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			UpdateProjectSubscriptionEvent updateProjectSubscriptionEvent = JsonConvert.DeserializeObject<UpdateProjectSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"9999-12-31T23:59:59.9999999Z\"," +
				"\"EndDate\":\"2019-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			AssociateProjectSubscriptionEvent associateProjectSubscriptionEvent = JsonConvert.DeserializeObject<AssociateProjectSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"ProjectUID\":\"d6b1b168-f04f-4f9d-b80a-0aebf36926c2\",\"EffectiveDate\":\"9999-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			DissociateProjectSubscriptionEvent dissociateProjectSubscriptionEvent = JsonConvert.DeserializeObject<DissociateProjectSubscriptionEvent>("{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\"," +
				"\"ProjectUID\":\"d6b1b168-f04f-4f9d-b80a-0aebf36926c2\",\"EffectiveDate\":\"9999-12-31T23:59:59.9999999Z\",\"ActionUTC\":\"2019-08-25T23:40:00Z\",\"ReceivedUTC\":\"2019-11-16T08:31:40.4129206Z\"}");

			var date = new DateTime(9999, 12, 31, 23, 59, 59);

			//Act
			controller.CreateAssetSubscription(createAssetSubscriptionEvent);
			controller.UpdateAssetSubscription(updateAssetSubscriptionEvent);
			controller.CreateCustomerSubscription(createCustomerSubscriptionEvent);
			controller.UpdateCustomerSubscription(updateCustomerSubscriptionEvent);
			controller.CreateProjectSubscription(createProjectSubscriptionEvent);
			controller.UpdateProjectSubscription(updateProjectSubscriptionEvent);
			controller.AssociateProjectSubscription(associateProjectSubscriptionEvent);
			controller.DissociateProjectSubscription(dissociateProjectSubscriptionEvent);

			//Assert
			subscriptionService.Received(1).CreateAssetSubscription(Arg.Is<CreateAssetSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).UpdateAssetSubscription(Arg.Is<UpdateAssetSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).CreateCustomerSubscription(Arg.Is<CreateCustomerSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).UpdateCustomerSubscription(Arg.Is<UpdateCustomerSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).CreateProjectSubscription(Arg.Is<CreateProjectSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).UpdateProjectSubscription(Arg.Is<UpdateProjectSubscriptionEvent>(x => x.StartDate.Value <= date && x.EndDate.Value <= date));
			subscriptionService.Received(1).AssociateProjectSubscription(Arg.Is<AssociateProjectSubscriptionEvent>(x => x.EffectiveDate.Value <= date));
			subscriptionService.Received(1).DissociateProjectSubscription(Arg.Is<DissociateProjectSubscriptionEvent>(x => x.EffectiveDate.Value <= date));
		}
		#endregion
	}
}