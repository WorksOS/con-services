using ClientModel.Response;
using CommonModel.Error;
using CommonModel.Exceptions;
using Infrastructure.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities.Logging;

namespace CommonApiLibrary.Middlewares
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private ILoggingService _loggingService;
		private Regex jsonEx_PropertyNameMatcher = new Regex("Required property '(\\w+)'");

		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext httpContext, ILoggingService loggingService)
		{
			_loggingService = loggingService;
			_loggingService.CreateLogger<ExceptionMiddleware>();

			try
			{
				await _next(httpContext);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(httpContext, ex);
			}
		}

		private Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			ExceptionResponse exceptionResponse;
			var jsonSerializationEx = exception as JsonSerializationException;
			var domainEx = exception as DomainException;

			context.Response.ContentType = "application/json";

			_loggingService.Error("An Error has occurred : ", "ExceptionMiddleware.HandleExceptionAsync", exception);
			if (jsonSerializationEx != null && jsonSerializationEx.Message.Contains("Invalid Request"))
			{
				ExceptionErrorCodes errorCode = ExceptionErrorCodes.InvalidRequest;
				string message = UtilHelpers.GetEnumDescription(errorCode);
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				exceptionResponse = new ExceptionResponse(new AssetErrorInfo { ErrorCode = (int)errorCode, Message = string.Format(message, jsonSerializationEx.Message) });
			}
			else if (domainEx != null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				exceptionResponse = new ExceptionResponse(GetErrorLists(domainEx));
			}
			else
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				exceptionResponse = new ExceptionResponse(new AssetErrorInfo { Message = "An Unexpected Error has occurred", ErrorCode = (int)ExceptionErrorCodes.UnexpectedError });
			}

			if (string.Compare(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), EnvironmentName.Development, StringComparison.OrdinalIgnoreCase) == 0)
			{
				exceptionResponse.Exception = exception;
			}

			return context.Response.WriteAsync(JsonConvert.SerializeObject(exceptionResponse));
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
	}
}
