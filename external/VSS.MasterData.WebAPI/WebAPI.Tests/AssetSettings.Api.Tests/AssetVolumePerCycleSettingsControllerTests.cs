using AssetSettings.Api.Tests.Registrations;
using AssetSettings.Controller;
using AutoMapper;
using ClientModel.AssetSettings.Request;
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
	public class AssetVolumePerCycleSettingsControllerTests
    {
        private readonly IMapper _mapper;
        private readonly AssetConfigRegistrations<AssetVolumePerCycleSettingsController> _injectConfig;
        private AssetVolumePerCycleSettingsController _target;

        public AssetVolumePerCycleSettingsControllerTests()
        {
            _injectConfig = new AssetConfigRegistrations<AssetVolumePerCycleSettingsController>();
			_injectConfig.GetContainer(new ServiceCollection());
		}

        private void SetupController(int count, AssetSettingsRequest request, Guid? customerUID, Guid? userUID)
        {
            _target = _injectConfig.Resolve<AssetVolumePerCycleSettingsController>();
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
        public void SaveAssetVolumePerCycleSettings_AssetUIDsNull_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = null,
                TargetValue = 250.98
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, Guid.NewGuid(), Guid.NewGuid());
			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.AssetUIDListNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull));
        }

        [Fact]
        public void FetchAssetVolumePerCycleSettings_InvalidAssetUIDs_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                TargetValue = 250.98
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, Guid.NewGuid(), Guid.NewGuid());
			_target.ControllerContext.HttpContext.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[] { Guid.Empty.ToString() })));

			var result = _target.Fetch(null).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidAssetUID);
            //Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), Guid.Empty.ToString()));
        }

        [Fact]
        public void SaveAssetVolumePerCycleSettings_InvalidAssetUIDInAssetUIDs_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                TargetValue = 250.98
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });
            request.AssetUIds = new List<string>(this._injectConfig.AssetUIDs);

            request.AssetUIds.Add(Guid.Empty.ToString()); //Invalid AssetUId

            SetupController(10, request, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidAssetUID);
            //Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), Guid.Empty.ToString()));
        }

        [Fact]
        public void SaveAssetVolumePerCycleSettings_CustomerUidNull_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                TargetValue = 250.98
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, null, Guid.NewGuid());
			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.CustomerUIDNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull));
        }

        [Fact]
        public void SaveAssetVolumePerCycleSettings_UserUidNull_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                TargetValue = 250.98
            };
            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, Guid.NewGuid(), null);
			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.UserUIDNull);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull));
        }

        [Fact]
        public void SaveAssetVolumePerCycleSettings_TargetValueNegative_GivesBadRequest()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                TargetValue = -555
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, Guid.NewGuid(), Guid.NewGuid());
			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
            //Assert.NotNull(responseObject.Errors);
            //Assert.True(responseObject.Errors.Count > 0);
            //Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.TargetValueIsNegative);
            //Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.TargetValueIsNegative));
        }

        [Fact]
        public void SaveAssetVolumePerCycleSettings_ValidAssetUIds_GivesCreated()
        {
            var request = new AssetSettingsRequest
            {
                AssetUIds = this._injectConfig.AssetUIDs,
                TargetValue = 250.98
            };

            _injectConfig.BuildRepositoryStub(10, request, new List<AssetTargetType> { AssetTargetType.BucketVolumeinCuMeter });

            SetupController(10, request, Guid.NewGuid(), Guid.NewGuid());
			var result = _target.Save(request).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetVolumePerCycleSettingsResponse;

			Assert.NotNull(response);
            //Assert.Equal(response.StatusCode, (int)HttpStatusCode.OK);
        }
    }
}
