using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace VSS.MasterData.WebAPI.Helpers
{
	public static class Utils
	{
		public static Guid? GetUserContext(HttpRequest request)
		{
			request.HttpContext.Request.Headers.TryGetValue(Constants.USERUID_APIRequest, out StringValues values);
			var userUIDHeader = values.FirstOrDefault() == null ? (Guid?)null :
				Guid.Parse(values.First());
			return userUIDHeader;
		}
	}
}
