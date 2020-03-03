using AssetSettings.Api.Tests.Registrations;
using AssetSettings.Controller;
using AutoMapper;
using ClientModel.AssetSettings.Request.AssetSettings;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Enum;
using Infrastructure.Common.Constants;
using Infrastructure.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xunit;

namespace AssetSettings.Api.Tests
{
	public class AssetFuelBurnRateSettingsControllerTests
    {
        private readonly IMapper _mapper;
        private readonly AssetConfigRegistrations<AssetFuelBurnRateSettingsController> _injectConfig;
        private AssetFuelBurnRateSettingsController _target;

        public AssetFuelBurnRateSettingsControllerTests()
        {
            _injectConfig = new AssetConfigRegistrations<AssetFuelBurnRateSettingsController>();
			_injectConfig.GetContainer(new ServiceCollection());
        }

        private void SetupController(AssetFuelBurnRateSettingRequest request, Guid? customerUID, Guid? userUID)
        {
            _target = _injectConfig.Resolve<AssetFuelBurnRateSettingsController>();
            _target.ControllerContext = GetMockHttpContext();
            if (customerUID != null)
            {
				_target.ControllerContext.HttpContext.Request.Headers.Add(Constants.VISIONLINK_CUSTOMERUID, customerUID.ToString());
            }
            if (userUID != null)
            {
				_target.ControllerContext.HttpContext.Request.Headers.Add(Constants.USERUID_API, userUID.ToString());
            }
            _target.ControllerContext.HttpContext.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
        }

		private static ControllerContext GetMockHttpContext()
		{
			//TODO: Add Mock HTTP Request object for AssetTargetRequest
			var controllerContext = new ControllerContext();
			controllerContext.HttpContext = new DefaultHttpContext();
			return controllerContext;
		}

		[Fact]
        public void SaveAssetFuelBurnRateSettings_AssetUIDsNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = null,
                IdleTargetValue = 1,
                WorkTargetValue = 10
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });

            SetupController(request, Guid.NewGuid(), Guid.NewGuid());

            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(result);
			Assert.NotNull(response);
			//Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
   //         Assert.NotNull(responseObject.Errors);
   //         Assert.True(responseObject.Errors.Count > 0);
   //         Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.AssetUIDListNull);
   //         Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull));
        }

        [Fact]
        public void FetchAssetFuelBurnRateSettings_InvalidAssetUIDs_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 5,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });

            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            _target.ControllerContext.HttpContext.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[] { Guid.Empty.ToString() })));

            var result = _target.Fetch().Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidAssetUID);
            //Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), Guid.Empty.ToString()));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_InvalidAssetUIDInAssetUIDs_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = new List<string> { Guid.Empty.ToString() },
                IdleTargetValue = 5,
                WorkTargetValue = 10
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });

            request.AssetUIds = new List<string>(this._injectConfig.AssetUIDs);

            request.AssetUIds.Add(Guid.Empty.ToString()); //Invalid AssetUId

            SetupController(request, Guid.NewGuid(), Guid.NewGuid());

            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidAssetUID);
            //Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), Guid.Empty.ToString()));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_CustomerUidNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 5,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, null, Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.CustomerUIDNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_UserUidNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 5,
                WorkTargetValue = 10
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });

            SetupController(request, Guid.NewGuid(), null);
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.UserUIDNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_IdleTargetValueNegative_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = -1,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.IdleValueShouldNotBeNegative);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.IdleValueShouldNotBeNegative));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueZeroIdleTargetValueGreaterThanWork_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 10,
                WorkTargetValue = 0
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkValueShouldBeLessThanIdleValue);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldBeLessThanIdleValue));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkAndIdleTargetValueZero_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 0,
                WorkTargetValue = 0
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueGreaterAndIdleTargetValueZero_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 0,
                WorkTargetValue = 1
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueLessThanIdleTargetValue_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 11,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkValueShouldBeLessThanIdleValue);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldBeLessThanIdleValue));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_IdleTargetValueNegative_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = -1,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.IdleValueShouldNotBeNegative);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.IdleValueShouldNotBeNegative));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueNegative_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 11,
                WorkTargetValue = -1
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkValueShouldNotBeNegative);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldNotBeNegative));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_BothWorkAndIdleTargetValueNegative_GivesOK()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = -1,
                WorkTargetValue = -1
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkAndIdleValueShouldNotBeNegative);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkAndIdleValueShouldNotBeNegative));
        }

        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueLessThanIdleTargetValue_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 2,
                WorkTargetValue = 1
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkValueShouldBeLessThanIdleValue);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldBeLessThanIdleValue));
        }


        [Fact]
        public void SaveAssetFuelBurnRateSettings_ValidAssetUIds_GivesCreated()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 5,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });

            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Sending work and idle target values as nulls to the first time configuration gives bad request
        /// </summary>
        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkAndIdleTargetValuesNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = null,
                WorkTargetValue = null
            };
            _injectConfig.BuildRepositoryStub(0, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkOrIdleValuesShouldNotBeNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkOrIdleValuesShouldNotBeNull));
        }
        
        /// <summary>
        /// Unsetting the Fuel burn rates gives success
        /// </summary>
        [Fact]
        public void SaveAssetFuelBurnRateSettings_UnsetTargetValuesNull_GivesSuccess()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = null,
                WorkTargetValue = null
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
            
        }
        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 10,
                WorkTargetValue = null
            };
            _injectConfig.BuildRepositoryStub(0, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkOrIdleValuesShouldNotBeNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkOrIdleValuesShouldNotBeNull));
        }
        [Fact]
        public void SaveAssetFuelBurnRateSettings_IdleTargetValueNull_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = null,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(0, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.WorkOrIdleValuesShouldNotBeNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkOrIdleValuesShouldNotBeNull));
        }
        [Fact]
        public void SaveAssetFuelBurnRateSettings_WorkTargetValueNull_Upsert_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = 10,
                WorkTargetValue = null
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull));
        }
        [Fact]
        public void SaveAssetFuelBurnRateSettings_IdleTargetValueNull_Upsert_GivesBadRequest()
        {
            var request = new AssetFuelBurnRateSettingRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                IdleTargetValue = null,
                WorkTargetValue = 10
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.IdlingBurnRateinLiPerHour, AssetTargetType.WorkingBurnRateinLiPerHour });
            SetupController(request, Guid.NewGuid(), Guid.NewGuid());
            var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetFuelBurnRateSettingsResponse;
			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal((int)ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull, responseObject.Errors[0].ErrorCode);
            //Assert.Equal(UtilHelpers.GetEnumDescription(ErrorCodes.BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull), responseObject.Errors[0].Message);
        }
    }
}
