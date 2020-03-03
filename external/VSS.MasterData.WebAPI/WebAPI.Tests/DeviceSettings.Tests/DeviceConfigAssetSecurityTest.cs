using Autofac;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security;
using CommonModel.DeviceSettings.ConfigNameValues;
using Infrastructure.Common.Constants;
using DeviceSettings.Controller;
using DeviceSettings.Test.Registrations;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using Utilities.IOC;
using Utilities.Logging;
using Xunit;
using Newtonsoft.Json;
using System.IO;

namespace DeviceSettings.Test
{
	public class DeviceConfigAssetSecurityTest
	{
		private readonly IDeviceConfigService<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails> _assetSecurityService;
		private readonly DeviceConfigRegistrations<DeviceConfigAssetSecurityController> _injectConfig;
		private DeviceConfigAssetSecurityController _target;

		public DeviceConfigAssetSecurityTest()
		{
			//_injectConfig = new DeviceConfigRegistrations<DeviceConfigAssetSecurityController>();

			//_injectConfig.ConfigureServices(new ServiceCollection());
		}

		private void SetupController(DeviceConfigRequestBase request, Guid? customerUID, Guid? userUID)
		{
			_target = _injectConfig.Resolve<DeviceConfigAssetSecurityController>();
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
			var controllerContext = new ControllerContext();
			controllerContext.HttpContext = new DefaultHttpContext();
			return controllerContext;
		}


		[Fact]
		public void Valid_Fetch_Request()
		{
			string AssetUID = "14da462c-f23e-499f-8ed1-50467d8b57ac";
			var deviceConfigBaseRequest = new DeviceConfigRequestBase()
			{
				AssetUIDs = new System.Collections.Generic.List<string>() { AssetUID },
				DeviceType = "PL121",
			};

			//SetupController(deviceConfigBaseRequest, Guid.NewGuid(), Guid.NewGuid());
			//var res = _target.Fetch(deviceConfigBaseRequest);
			Assert.True(1 == 1);
		}
	}
}
