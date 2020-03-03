using CommonApiLibrary;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;

namespace DeviceSettings.Controller
{
	public class DeviceConfigApiControllerBase : ApiControllerBase
	{
		protected readonly ILoggingService _loggingService;
		protected readonly ConfigNameValueCollection _attributeMaps;

		public DeviceConfigApiControllerBase(ConfigNameValueCollection attributeMaps, ILoggingService loggingService)
		{
			this._loggingService = loggingService;
			this._attributeMaps = attributeMaps;
		}
	}
}
