using System.Diagnostics.CodeAnalysis;

namespace VSS.Authentication.Session.Model
{
	using System;

	[ExcludeFromCodeCoverage]
	public class SessionToken
	{
		public string AccessToken;

		public DateTimeOffset ExpirationUTC;
	}
}