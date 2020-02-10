using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.KafkaModel;
using VSS.MasterData.WebAPI.Preference.Controllers.V1;
using Xunit;

namespace VSS.MasterData.WebAPI.Preference.Tests
{
	public class PreferenceTests
	{
		private readonly IPreferenceService preferenceService;
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private UserPreferenceV1Controller controller;

		public PreferenceTests()
		{
			logger = Substitute.For<ILogger>();
			preferenceService = Substitute.For<IPreferenceService>();
			configuration = Substitute.For<IConfiguration>();
			controller = new UserPreferenceV1Controller(preferenceService, configuration, logger);
		}

		#region Create Target UserPreference
		[Fact]
		public void TestCreateTargetUserPreference_ValidInput_Ok()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestCreateTargetUserPreference_InvalidUserUIDRequest_BadRequest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Target UserUID has not been provided", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_CreatesOnAllowUpdate_Ok()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, true);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_UpdatesOnAllowUpdate_Ok()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, true);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_UpdateNotAllowed_BadRquest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, false);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserPreference already exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_InvalidPreferenceKeyOnCreate_BadRquest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "invalid",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_InvalidPreferenceKeyOnUpdate_BadRquest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "invalid",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, true);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateTargetUserPreference_RequestFailedOnCreate_BadRquest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, false);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestCreateTargetUserPreference_RequestFailedOnUpdate_BadRquest()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, true);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestCreateTargetUserPreference_ExceptionOnCreate_InternalServerError()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestCreateTargetUserPreference_ExceptionOnUpdate_InternalServerError()
		{
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("aff78483-4c39-4049-bcca-624f068eaa92"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.CreateTargetUserPreference(payload, true);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}
		#endregion

		#region Update Target UserPreference
		[Fact]
		public void TestUpdateTargetUserPreference_ValidInput_Ok()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestUpdateTargetUserPreference_InvalidUserUIDRequest_BadRequest()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Target UserUID has not been provided", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestUpdateTargetUserPreference_UserPreferenceNotExist_BadRequest()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => throw new Exception("UserPreference does not Exist"));

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserPreference does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestUpdateTargetUserPreference_RequestFailed_BadRequest()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestUpdateTargetUserPreference_InvalidPreferenceKey_BadRequest()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "invalid",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}

		[Fact]
		public void TestUpdateTargetUserPreference_Exception_InternalServerError()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("91a99dd9-6175-4b61-bff0-4bac45b89d43"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.UpdateTargetUserPreference(payload);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}
		#endregion

		#region Create UserPreference
		[Fact]
		public void TestCreateUserPreference_ValidInput_Ok()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestCreateUserPreference_InvalidUserUIDRequest_BadRequest()
		{
			//Arrange
			Guid? userUID = null;
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUID);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserUID has not been provided", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_CreatesOnAllowUpdate_Ok()
		{
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			//Arrange
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, true);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == userUid));
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_UpdatesOnAllowUpdate_Ok()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, true);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_UpdateNotAllowed_BadRquest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, false);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserPreference already exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_InvalidPreferenceKeyOnCreate_BadRquest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "invalid",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == userUid));
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_InvalidPreferenceKeyOnUpdate_BadRquest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, true);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
			preferenceService.Received(0).CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>());
		}

		[Fact]
		public void TestCreateUserPreference_RequestFailedOnCreate_BadRquest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, false);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestCreateUserPreference_RequestFailedOnUpdate_BadRquest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, true);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestCreateUserPreference_ExceptionOnCreate_InternalServerError()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(false);
			preferenceService.CreateUserPreference(Arg.Any<CreateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<ObjectResult>(result);
			Assert.Equal(500, ((ObjectResult)result).StatusCode);
			Assert.Contains("Something Went Wrong", ((ObjectResult)result).Value.ToString());
			preferenceService.Received(1).CreateUserPreference(Arg.Is<CreateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestCreateUserPreference_ExceptionOnUpdate_InternalServerError()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			CreateUserPreferencePayload payload = new CreateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("d2b9fb2d-76e4-43f6-837d-0de458899070"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(userUid), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.CreateUserPreference(payload, userUid, true);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
		}
		#endregion

		#region Update UserPreference
		[Fact]
		public void TestUpdateUserPreference_ValidInput_Ok()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("2d5fca22-5832-4943-92e1-1c854330d7d9"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.UpdateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestUpdateUserPreference_InvalidUserUIDRequest_BadRequest()
		{
			//Arrange
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("2d5fca22-5832-4943-92e1-1c854330d7d9"),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};

			//Act
			ActionResult result = controller.UpdateUserPreference(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserUID has not been provided", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>());
		}

		[Fact]
		public void TestUpdateUserPreference_RequestFailed_BadRequest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("2d5fca22-5832-4943-92e1-1c854330d7d9"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.UpdateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save row to database", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestUpdateUserPreference_InvalidPreferenceKey_BadRequest()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("2d5fca22-5832-4943-92e1-1c854330d7d9"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "invalid",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { return null; });

			//Act
			ActionResult result = controller.UpdateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == userUid));
		}

		[Fact]
		public void TestUpdateUserPreference_Exception_InternalServerError()
		{
			//Arrange
			Guid userUid = new Guid("4b0c9d8d-b7fc-4474-a49d-1bcc2089c924");
			UpdateUserPreferencePayload payload = new UpdateUserPreferencePayload
			{
				PreferenceKeyUID = new Guid("2d5fca22-5832-4943-92e1-1c854330d7d9"),
				TargetUserUID = new Guid(),
				PreferenceKeyName = "fleet",
				PreferenceJson = "{ \"TimeZone\" : \"UTC\"}",
				SchemaVersion = "1.0",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DoesUserPreferenceExist(Arg.Is(payload.TargetUserUID.Value), Arg.Is(payload.PreferenceKeyUID.Value), payload.PreferenceKeyName).Returns(true);
			preferenceService.UpdateUserPreference(Arg.Any<UpdateUserPreferenceEvent>()).Returns(x => { throw new Exception("Something Went Wrong"); });

			//Act
			ActionResult result = controller.UpdateUserPreference(payload, userUid);

			//Assert
			Assert.IsType<StatusCodeResult>(result);
			Assert.Equal(500, ((StatusCodeResult)result).StatusCode);
			preferenceService.Received(1).UpdateUserPreference(Arg.Is<UpdateUserPreferenceEvent>(x => x.UserUID == payload.TargetUserUID));
		}
		#endregion

		#region Delete UserPreference
		[Fact]
		public void TestDeleteUserPreference_ValidInput_Ok()
		{
			//Arrange
			Guid preferenceUid = new Guid("5a65c453-4414-4e55-b8d9-85a3519b0018");
			Guid userUid = new Guid("723f78c3-dbed-47ec-964d-1b00ead24015");
			string preferenceKeyName = "Units";
			preferenceService.DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.DeleteUserPreference(preferenceKeyName, preferenceUid, userUid);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).DeleteUserPreference(Arg.Is<DeleteUserPreferenceEvent>(x => (x.UserUID == userUid && x.PreferenceKeyName == preferenceKeyName && x.PreferenceKeyUID == preferenceUid)));
		}

		[Fact]
		public void TestDeleteUserPreference_InvalidUserUID_BadRequest()
		{
			//Arrange
			Guid preferenceUid = new Guid("5a65c453-4414-4e55-b8d9-85a3519b0018");
			string preferenceKeyName = "Units";
			preferenceService.DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.DeleteUserPreference(preferenceKeyName, preferenceUid, null);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("UserUID has not been provided", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>());
		}

		[Fact]
		public void TestDeleteUserPreference_PreferenceDoesNotExist_BadRequest()
		{
			//Arrange
			Guid preferenceUid = new Guid("5a65c453-4414-4e55-b8d9-85a3519b0018");
			Guid userUid = new Guid("77ccd143-87be-408a-89e5-b94673a93af3");
			string preferenceKeyName = "Units";
			preferenceService.DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>()).Returns(x => null);

			//Act
			ActionResult result = controller.DeleteUserPreference(preferenceKeyName, preferenceUid, userUid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).DeleteUserPreference(Arg.Is<DeleteUserPreferenceEvent>(x => (x.UserUID == userUid && x.PreferenceKeyName == preferenceKeyName && x.PreferenceKeyUID == preferenceUid)));
		}

		[Fact]
		public void TestDeleteUserPreference_DeleteFailed_BadRequest()
		{
			//Arrange
			Guid preferenceUid = new Guid("5a65c453-4414-4e55-b8d9-85a3519b0018");
			Guid userUid = new Guid("77ccd143-87be-408a-89e5-b94673a93af3");
			string preferenceKeyName = "Units";
			preferenceService.DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>()).Returns(false);

			//Act
			ActionResult result = controller.DeleteUserPreference(preferenceKeyName, preferenceUid, userUid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to delete in db", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).DeleteUserPreference(Arg.Is<DeleteUserPreferenceEvent>(x => (x.UserUID == userUid && x.PreferenceKeyName == preferenceKeyName && x.PreferenceKeyUID == preferenceUid)));
		}

		[Theory]
		[InlineData(null, null)]
		[InlineData("", null)]
		public void TestDeleteUserPreference_InvalidPreferenceInput_BadRequest(string preferenceKeyName, Guid? preferenceKeyuid)
		{
			//Arrange
			Guid userUID = new Guid("fc23af60-7415-41b4-bcde-6525af60251a");
			preferenceService.DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>()).Returns(true);

			//Act
			ActionResult result = controller.DeleteUserPreference(preferenceKeyName, preferenceKeyuid, userUID);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("preferenceKeyUID or preferenceKeyName should be given", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(0).DeleteUserPreference(Arg.Any<DeleteUserPreferenceEvent>());
		}
		#endregion

		#region Get TargetUser Preference
		[Fact]
		public void TestGetTargetUserPreferenceAndKey_ValidInputWithoutKeyName_Ok()
		{
			//Arrrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			Guid pUid1 = new Guid("1fdcd0ad-0812-455a-acac-2641a3ef84e3");
			Guid pUid2 = new Guid("0929bbec-bf1d-45f7-8bfa-e1881646b963");
			List<UserPreference> listUserPreference = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", PreferenceKeyUID = pUid1, SchemaVersion = "1.0" },
				new UserPreference { PreferenceJson = "JSON2", PreferenceKeyName = "Key2", PreferenceKeyUID = pUid2, SchemaVersion = "1.0" }
			};
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(listUserPreference);

			//Act
			ActionResult<UserPreference> result = controller.GetTargetUserPreferencesForUserAndKey(userUid.ToString(), "1.0", null);
			var response = ((List<UserPreference>)(((OkObjectResult)(result.Result)).Value));

			//Assert
			Assert.Equal("Key1", response[0].PreferenceKeyName);
			Assert.Equal("JSON1", response[0].PreferenceJson);
			Assert.Equal(pUid1, response[0].PreferenceKeyUID);
			Assert.Equal("Key2", response[1].PreferenceKeyName);
			Assert.Equal("JSON2", response[1].PreferenceJson);
			Assert.Equal(pUid2, response[1].PreferenceKeyUID);
		}

		[Fact]
		public void TestGetTargetUserPreferenceAndKey_ValidInputWithKeyName_Ok()
		{
			//Arrrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			Guid pUid1 = new Guid("1fdcd0ad-0812-455a-acac-2641a3ef84e3");
			string keyName = "Key";
			List<UserPreference> listUserPreference = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", PreferenceKeyUID = pUid1, SchemaVersion = "1.0" }
			};
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(listUserPreference);

			//Act
			ActionResult<UserPreference> result = controller.GetTargetUserPreferencesForUserAndKey(userUid.ToString(), "1.0", keyName);
			var response = ((UserPreference)(((OkObjectResult)(result.Result)).Value));

			//Assert
			Assert.Equal("Key1", response.PreferenceKeyName);
			Assert.Equal("JSON1", response.PreferenceJson);
			Assert.Equal(pUid1, response.PreferenceKeyUID);
		}

		[Fact]
		public void TestGetTargetUserPreferenceAndKey_InvalidInputUserUID_BadRequest()
		{
			//Act
			ActionResult<UserPreference> result = controller.GetTargetUserPreferencesForUserAndKey(null, "1.0", "Key");

			//Assert
			Assert.IsType<BadRequestObjectResult>(result.Result);
			Assert.Contains("Invalid UserUID", ((BadRequestObjectResult)result.Result).Value.ToString());
		}

		[Fact]
		public void TestGetTargetUserPreferenceAndKey_Exception_InternalServerError()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = "Key";
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => { throw new Exception(); });

			//Act
			ActionResult<UserPreference> result = controller.GetTargetUserPreferencesForUserAndKey(userUid.ToString(), "1.0", keyName);

			//Assert
			Assert.IsType<ObjectResult>(result.Result);
			Assert.Equal(500, ((ObjectResult)(result.Result)).StatusCode);
			Assert.Contains("Unable to read from db", ((ObjectResult)(result.Result)).Value.ToString());
			preferenceService.Received(1).GetUserPreferencesForUser(Arg.Is(userUid), "1.0", keyName);
		}
		#endregion

		#region Create PreferenceKey
		[Fact]
		public void TestCreatePreferenceKey_ValidInput_Ok()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			CreatePreferenceKeyEvent payload = new CreatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.CreatePreferenceKey(Arg.Any<CreatePreferenceKeyEvent>()).Returns(true);

			//Act
			ActionResult result = controller.CreatePreferenceKey(payload);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).CreatePreferenceKey(Arg.Is<CreatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestCreatePreferenceKey_PreferenceKeyDoesNotExist_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			CreatePreferenceKeyEvent payload = new CreatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.CreatePreferenceKey(Arg.Any<CreatePreferenceKeyEvent>()).Returns(x => null);

			//Act
			ActionResult result = controller.CreatePreferenceKey(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey already exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreatePreferenceKey(Arg.Is<CreatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestCreatePreferenceKey_RequestFailed_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			CreatePreferenceKeyEvent payload = new CreatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.CreatePreferenceKey(Arg.Any<CreatePreferenceKeyEvent>()).Returns(false);

			//Act
			ActionResult result = controller.CreatePreferenceKey(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).CreatePreferenceKey(Arg.Is<CreatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestCreatePreferenceKey_Exception_InternalServerError()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			CreatePreferenceKeyEvent payload = new CreatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.CreatePreferenceKey(Arg.Any<CreatePreferenceKeyEvent>()).Returns(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.CreatePreferenceKey(payload);

			//Assert
			Assert.IsType<ObjectResult>(result);
			Assert.Equal(500, ((ObjectResult)(result)).StatusCode);
			preferenceService.Received(1).CreatePreferenceKey(Arg.Is<CreatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}
		#endregion

		#region Update PreferenceKey
		[Fact]
		public void TestUpdatePreferenceKey_ValidInput_Ok()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			UpdatePreferenceKeyEvent payload = new UpdatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdatePreferenceKey(Arg.Any<UpdatePreferenceKeyEvent>()).Returns(true);

			//Act
			ActionResult result = controller.UpdatePreferenceKey(payload);

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).UpdatePreferenceKey(Arg.Is<UpdatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestUpdatePreferenceKey_PreferenceKeyDoesNotExist_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			UpdatePreferenceKeyEvent payload = new UpdatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdatePreferenceKey(Arg.Any<UpdatePreferenceKeyEvent>()).Returns(x => null);

			//Act
			ActionResult result = controller.UpdatePreferenceKey(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdatePreferenceKey(Arg.Is<UpdatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestUpdatePreferenceKey_PreferenceNameAlreadyExist_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			UpdatePreferenceKeyEvent payload = new UpdatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdatePreferenceKey(Arg.Any<UpdatePreferenceKeyEvent>()).Returns(x => throw new Exception("PreferenceKey Name Already Exist"));

			//Act
			ActionResult result = controller.UpdatePreferenceKey(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey Name Already Exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdatePreferenceKey(Arg.Is<UpdatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestUpdatePreferenceKey_RequestFailed_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			UpdatePreferenceKeyEvent payload = new UpdatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdatePreferenceKey(Arg.Any<UpdatePreferenceKeyEvent>()).Returns(false);

			//Act
			ActionResult result = controller.UpdatePreferenceKey(payload);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to save to db. Make sure request is not duplicated and all keys exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).UpdatePreferenceKey(Arg.Is<UpdatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestUpdatePreferenceKey_Exception_InternalServerError()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			UpdatePreferenceKeyEvent payload = new UpdatePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				PreferenceKeyName = "fleet",
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.UpdatePreferenceKey(Arg.Any<UpdatePreferenceKeyEvent>()).Returns(x => { throw new Exception(); });

			//Act
			ActionResult result = controller.UpdatePreferenceKey(payload);

			//Assert
			Assert.IsType<ObjectResult>(result);
			Assert.Equal(500, ((ObjectResult)(result)).StatusCode);
			preferenceService.Received(1).UpdatePreferenceKey(Arg.Is<UpdatePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}
		#endregion

		#region Delete PreferenceKey
		[Fact]
		public void TestDeletePreferenceKey_ValidInput_Ok()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			DeletePreferenceKeyEvent payload = new DeletePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DeletePreferenceKey(Arg.Any<DeletePreferenceKeyEvent>()).Returns(true);

			//Act
			ActionResult result = controller.DeletePreferenceKey(preferenceKeyUid.ToString());

			//Assert
			Assert.IsType<OkResult>(result);
			preferenceService.Received(1).DeletePreferenceKey(Arg.Is<DeletePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("3A586859-B6D4-4BAA-9ED8-CB1E533931748")]
		public void TestDeletePreferenceKey_InvalidInputPreferenceKeyUID_BadRequest(string uid)
		{
			//Act
			ActionResult<UserPreference> result = controller.DeletePreferenceKey(uid);

			//Assert
			Assert.IsType<BadRequestObjectResult>(result.Result);
			Assert.Contains("PreferenceKeyGuid is not valid", ((BadRequestObjectResult)result.Result).Value.ToString());
		}

		[Fact]
		public void TestDeletePreferenceKey_PreferenceKeyDoesNotExist_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			DeletePreferenceKeyEvent payload = new DeletePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DeletePreferenceKey(Arg.Any<DeletePreferenceKeyEvent>()).Returns(x => null);

			//Act
			ActionResult result = controller.DeletePreferenceKey(preferenceKeyUid.ToString());

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("PreferenceKey does not exist", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).DeletePreferenceKey(Arg.Is<DeletePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestDeletePreferenceKey_RequestFailed_BadRquest()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			DeletePreferenceKeyEvent payload = new DeletePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DeletePreferenceKey(Arg.Any<DeletePreferenceKeyEvent>()).Returns(false);

			//Act
			ActionResult result = controller.DeletePreferenceKey(preferenceKeyUid.ToString());

			//Assert
			Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Unable to delete in db", ((BadRequestObjectResult)result).Value);
			preferenceService.Received(1).DeletePreferenceKey(Arg.Is<DeletePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}

		[Fact]
		public void TestDeletePreferenceKey_Exception_InternalServerError()
		{
			//Arrange
			Guid preferenceKeyUid = new Guid("08ba54f9-cf06-4da9-b7e1-60bb365e7301");
			DeletePreferenceKeyEvent payload = new DeletePreferenceKeyEvent
			{
				PreferenceKeyUID = preferenceKeyUid,
				ActionUTC = DateTime.UtcNow
			};
			preferenceService.DeletePreferenceKey(Arg.Any<DeletePreferenceKeyEvent>()).Returns(x => { throw new Exception(""); });

			//Act
			ActionResult result = controller.DeletePreferenceKey(preferenceKeyUid.ToString());

			//Assert
			Assert.IsType<ObjectResult>(result);
			Assert.Equal(500, ((ObjectResult)(result)).StatusCode);
			preferenceService.Received(1).DeletePreferenceKey(Arg.Is<DeletePreferenceKeyEvent>(x => x.PreferenceKeyUID == preferenceKeyUid));
		}
		#endregion

		#region Get User Preference
		[Fact]
		public void TestGetUserPreferencesForUserAndKey_ValidInputWithoutKeyName_Ok()
		{
			//Arrrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			Guid pUid1 = new Guid("1fdcd0ad-0812-455a-acac-2641a3ef84e3");
			Guid pUid2 = new Guid("0929bbec-bf1d-45f7-8bfa-e1881646b963");
			List<UserPreference> listUserPreference = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", PreferenceKeyUID = pUid1, SchemaVersion = "1.0" },
				new UserPreference { PreferenceJson = "JSON2", PreferenceKeyName = "Key2", PreferenceKeyUID = pUid2, SchemaVersion = "1.0" }
			};
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(listUserPreference);

			//Act
			ActionResult<UserPreference> result = controller.GetTargetUserPreferencesForUserAndKey(userUid.ToString(), "1.0", null);
			var response = ((List<UserPreference>)(((OkObjectResult)(result.Result)).Value));

			//Assert
			Assert.Equal("Key1", response[0].PreferenceKeyName);
			Assert.Equal("JSON1", response[0].PreferenceJson);
			Assert.Equal(pUid1, response[0].PreferenceKeyUID);
			Assert.Equal("Key2", response[1].PreferenceKeyName);
			Assert.Equal("JSON2", response[1].PreferenceJson);
			Assert.Equal(pUid2, response[1].PreferenceKeyUID);
		}

		[Fact]
		public void TestGetUserPreferencesForUserAndKey_ValidInputWithKeyName_Ok()
		{
			//Arrrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			Guid pUid1 = new Guid("1fdcd0ad-0812-455a-acac-2641a3ef84e3");
			string keyName = "Key";
			List<UserPreference> listUserPreference = new List<UserPreference>
			{
				new UserPreference { PreferenceJson = "JSON1", PreferenceKeyName = "Key1", PreferenceKeyUID = pUid1, SchemaVersion = "1.0" }
			};
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(listUserPreference);

			//Act
			ActionResult<UserPreference> result = controller.GetUserPreferencesForUserAndKey(userUid, "1.0", keyName);
			var response = ((UserPreference)(((OkObjectResult)(result.Result)).Value));

			//Assert
			Assert.Equal("Key1", response.PreferenceKeyName);
			Assert.Equal("JSON1", response.PreferenceJson);
			Assert.Equal(pUid1, response.PreferenceKeyUID);
		}

		[Fact]
		public void TestGetUserPreferencesForUserAndKey_InvalidInputUserUID_BadRequest()
		{
			//Act
			ActionResult<UserPreference> result = controller.GetUserPreferencesForUserAndKey(null, "1.0", "Key");

			//Assert
			Assert.IsType<BadRequestObjectResult>(result.Result);
		}

		[Fact]
		public void TestGetUserPreferencesForUserAndKey_Exception_InternalServerError()
		{
			//Arrange
			Guid userUid = new Guid("e96d6ded-826e-4cfc-a04e-a07963ba2a80");
			string keyName = "Key";
			preferenceService.GetUserPreferencesForUser(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => { throw new Exception(); });

			//Act
			ActionResult<UserPreference> result = controller.GetUserPreferencesForUserAndKey(userUid, "1.0", keyName);

			//Assert
			Assert.IsType<ObjectResult>(result.Result);
			preferenceService.Received(1).GetUserPreferencesForUser(Arg.Is(userUid), "1.0", keyName);
		}
		#endregion
	}
}