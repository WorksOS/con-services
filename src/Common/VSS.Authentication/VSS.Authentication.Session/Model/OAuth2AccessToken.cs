using System.Diagnostics.CodeAnalysis;

namespace VSS.Authentication.Session.Model
{
	[ExcludeFromCodeCoverage]
	public class OAuth2AccessToken
	{
		public string Scope { get; set; }

		public string Token_type { get; set; }

		public int Expires_in { get; set; }

		public string Refresh_token { get; set; }

		public string Id_token { get; set; }

		public string Access_token { get; set; }
	}
}