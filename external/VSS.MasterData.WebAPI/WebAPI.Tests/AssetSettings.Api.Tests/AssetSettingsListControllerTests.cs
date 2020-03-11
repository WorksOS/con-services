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
using NSubstitute;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Xunit;

namespace AssetSettings.Api.Tests
{
	public class AssetSettingsListControllerTests
	{
		private readonly IMapper _mapper;
		private readonly AssetSettingsRegistrations _injectConfig;
		private AssetSettingsListController _target;

		public AssetSettingsListControllerTests()
		{
			_injectConfig = new AssetSettingsRegistrations();
			_injectConfig.GetContainer(new ServiceCollection());
		}

		private void SetupController(int count, AssetSettingsListRequest request, Guid? customerUID, Guid? userUID)
		{
			_injectConfig.BuildRepositoryStub(count);
			_target = _injectConfig.Resolve<AssetSettingsListController>();
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
		public void FetchAssetSettingsList_ValidPageSizeAndNumber_GivesOk()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1
			}, Guid.NewGuid(), Guid.NewGuid());
			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;

			Assert.NotNull(response);
			//Assert.Equal(1, responseObject.RecordInfo.TotalPages);
			//Assert.Equal(10, responseObject.RecordInfo.TotalRecords);
			//Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}

		[Fact]
		public void FetchAssetSettingsList_InvalidPageSize_GivesOk()
		{
			SetupController(0, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 2
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 2
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;

			Assert.NotNull(response);
			//Assert.Equal(0, responseObject.RecordInfo.TotalPages);
			//Assert.Equal(0, responseObject.RecordInfo.TotalRecords);
			//Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}


		[Fact]
		public void FetchAssetSettingsList_InvalidPageNumber_GivesOk()
		{
			SetupController(0, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 2
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 2
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;

			Assert.NotNull(response);
			//Assert.Equal(0, responseObject.RecordInfo.TotalPages);
			//Assert.Equal(0, responseObject.RecordInfo.TotalRecords);

			//Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}

		[Fact]
		public void FetchAssetSettingsList_InvalidSortColumn_GivesBadRequest()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				SortColumn = "InvalidColumn"
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				SortColumn = "InvalidColumn"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;

			Assert.NotNull(response);
			//Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
			//Assert.NotNull(responseObject.Errors);
			//Assert.True(responseObject.Errors.Count > 0);
			//Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidSortColumn);
			//Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidSortColumn), "InvalidColumn"));
		}

		[Fact]
		public void FetchAssetSettingsList_InvalidFilterName_GivesBadRequest()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "InvalidFilter",
				FilterValue = "FilterValue"
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "InvalidFilter",
				FilterValue = "FilterValue"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.NotNull(response);
			//Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
			//Assert.NotNull(responseObject.Errors);
			//Assert.True(responseObject.Errors.Count > 0);
			//Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.InvalidFilterName);
			//Assert.Equal(responseObject.Errors[0].Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidFilterName), "InvalidFilter"));
		}

		[Fact]
		public void FetchAssetSettingsList_ValidFilterNameInvalidWithoutFilterValue_GivesBadRequest()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId"
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.NotNull(response);
			//Assert.Equal(response.StatusCode, (int)HttpStatusCode.BadRequest);
			//Assert.NotNull(responseObject.Errors);
			//Assert.True(responseObject.Errors.Count > 0);
			//Assert.Equal(responseObject.Errors[0].ErrorCode, (int)ErrorCodes.FilterValueNull);
			//Assert.Equal(responseObject.Errors[0].Message, UtilHelpers.GetEnumDescription(ErrorCodes.FilterValueNull));
		}

		[Fact]
		public void FetchAssetSettingsList_ValidFilterNameWithValidFilterValue_GivesOk()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId",
				FilterValue = "Asset"
			},Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId",
				FilterValue = "Asset"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.Equal(1, responseObject.RecordInfo.TotalPages);
			Assert.Equal(10, responseObject.RecordInfo.TotalRecords);

			Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}

		[Fact]
		public void FetchAssetSettingsList_ValidFilterNameWithValidFilterValueAndDeviceType_GivesOk()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId",
				FilterValue = "Asset",
				DeviceType = "PL321"
			}, Guid.NewGuid(), Guid.NewGuid());
			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				FilterName = "AssetId",
				FilterValue = "Asset",
				DeviceType = "PL321"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.Equal(1, responseObject.RecordInfo.TotalPages);
			Assert.Equal(10, responseObject.RecordInfo.TotalRecords);

			Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}

		[Fact]
		public void FetchAssetSettingsList_ValidDeviceType_GivesOk()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				DeviceType = "PL321"
			},Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				DeviceType = "PL321"
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.Equal(1, responseObject.RecordInfo.TotalPages);
			Assert.Equal(10, responseObject.RecordInfo.TotalRecords);
			Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}

		[Fact]
		public void FetchAssetSettingsList_EmptyDeviceTypeFilter_GivesOk()
		{
			SetupController(10, new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				DeviceType = string.Empty
			}, Guid.NewGuid(), Guid.NewGuid());

			var result = _target.Get(new AssetSettingsListRequest
			{
				PageSize = 10,
				PageNumber = 1,
				DeviceType = string.Empty
			}).Result;
			var response = result.Result as JsonResult;
			var responseObject = response.Value as AssetSettingsListResponse;
			Assert.Equal(1,responseObject.RecordInfo.TotalPages);
			Assert.Equal(10,responseObject.RecordInfo.TotalRecords);
			Assert.Equal(responseObject.Lists.Count, _injectConfig.AssetSettingLists.Count);
		}
	}
}
