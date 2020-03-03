using CommonModel.Error;
using CommonModel.Exceptions;
using Infrastructure.Common.Constants;
using Infrastructure.Common.Helpers;
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

namespace CommonApiLibrary
{
	public class ApiControllerBase : ControllerBase
	{
		protected async Task<string> ReadRequestContentAsStringAsync()
		{
			var bodyStr = "";
			Request.EnableBuffering();
			//Request.Body.Position = 0;
			using (StreamReader reader
					  = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
			{
				bodyStr = await reader.ReadToEndAsync();
				Request.Body.Position = 0;
			}
			return bodyStr;
		}

		protected async Task<T> ReadRequestContentAsync<T>(T request)
		{	
			var requestContent = await this.ReadRequestContentAsStringAsync();
			if (!string.IsNullOrEmpty(requestContent))
			{
				try
				{
					request = JsonConvert.DeserializeObject<T>(requestContent);
				}
				catch (JsonSerializationException ex) 
				{ 
					throw new JsonSerializationException("Invalid Request : " + ex.Message); 
				}
				if (request == null)
				{
					throw new JsonSerializationException("Invalid Request");
				}
			}
			return request;
		}


		protected List<AssetErrorInfo> GetErrorLists(DomainException domainEx)
		{
			var errorLists = new List<AssetErrorInfo>();
			if (domainEx.Errors == null)
			{
				domainEx.Errors = new List<ErrorInfo>();
			}
			if (domainEx.Error != null)
			{
				errorLists.Add(new AssetErrorInfo
				{
					ErrorCode = (int)ExceptionErrorCodes.InvalidRequest,
					Message = UtilHelpers.GetEnumDescription(ExceptionErrorCodes.InvalidRequest)
				});
			}
			foreach (var error in domainEx.Errors)
			{
				var assetError = error as AssetErrorInfo;
				if (assetError != null)
				{
					errorLists.Add(new AssetErrorInfo
					{
						AssetUID = assetError.AssetUID,
						ErrorCode = assetError.ErrorCode,
						Message = assetError.Message
					});
				}
				else
				{
					errorLists.Add(new AssetErrorInfo
					{
						ErrorCode = error.ErrorCode,
						Message = error.Message
					});
				}
			}
			return errorLists;
		}

		protected JsonResult SendResponse<TOutput>(HttpStatusCode httpStatusCode, TOutput outputObject)
		{
			return new JsonResult(outputObject) { StatusCode = (int)httpStatusCode };
		}

		protected StatusCodeResult SendResponse(HttpStatusCode httpStatusCode)
		{
			return new StatusCodeResult((int)httpStatusCode);
		}

		protected Guid? GetCustomerContext(HttpRequest Request)
		{
			StringValues customerUidValues;
			var customerUIDHeader = Request.Headers.TryGetValue(Constants.VISIONLINK_CUSTOMERUID, out customerUidValues)
							  ? customerUidValues.FirstOrDefault()
							  : string.Empty;
			return !string.IsNullOrEmpty(customerUIDHeader) ? Guid.Parse(customerUIDHeader) : (Guid?)null;
		}

		protected Guid? GetUserContext(HttpRequest Request)
		{
			StringValues userUidValues;
			var userUIDHeader = Request.Headers.TryGetValue(Constants.USERUID_API, out userUidValues) ? Guid.Parse(userUidValues.FirstOrDefault())
						  : (Guid?)null;
			return userUIDHeader;
		}

	}
}
