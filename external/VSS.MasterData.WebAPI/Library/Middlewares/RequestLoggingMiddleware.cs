using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Utilities.Logging;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading.Tasks;
using System.IO;
using Utilities.Logging.Models;

namespace CommonApiLibrary.Middlewares
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private ILoggingService _loggingService;
		private LogRequestContext _logRequestContext;

		public RequestLoggingMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext httpContext, LogRequestContext logRequestContext, ILoggingService loggingService)
		{
			_loggingService = loggingService;
			_logRequestContext = logRequestContext;

			_loggingService.CreateLogger<RequestLoggingMiddleware>();

			_logRequestContext.CorrelationId = Guid.NewGuid(); // For each web request, separate trace identifier will be generated and logged for each log messages
			_logRequestContext.TraceId = httpContext.TraceIdentifier; // For each web request, separate trace identifier will be generated and logged for each log messages
			
			_loggingService.Info("Started request for " + httpContext.Request.GetDisplayUrl() + " - Content - " + await ReadContent(httpContext.Request), "RequestLoggingMiddleware.InvokeAsync");
			await _next(httpContext);
			_loggingService.Info("Completed request", "RequestLoggingMiddleware.InvokeAsync");
		}

		private async Task<string> ReadContent(HttpRequest req)
		{
			string content;

			req.EnableBuffering();

			// Arguments: Stream, Encoding, detect encoding, buffer size 
			// AND, the most important: keep stream opened
			using (StreamReader reader
					  = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
			{
				content = await reader.ReadToEndAsync();
				req.Body.Position = 0;
			}
			return content;
		}
	}
}
