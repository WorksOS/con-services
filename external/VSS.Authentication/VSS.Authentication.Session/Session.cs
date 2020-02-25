using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using VSS.Authentication.Session.Model;

namespace VSS.Authentication.Session
{
	public class Session : ISession
	{
		private string _identityUrl;

		private string _consumerKey;

		private string _consumerSecret;

		private DateTimeOffset _tokenExpiration = DateTime.UtcNow.AddDays(-1);

		private string _accessToken;

		private SessionToken _sessionToken;

		public void SetConfigurationValues(string identityUrl, string consumerKey, string consumerSecret)
		{
			_identityUrl = identityUrl;
			_consumerKey = consumerKey;
			_consumerSecret = consumerSecret;
		}

		public SessionToken GetToken()
		{
			if (DateTimeOffset.UtcNow >= _tokenExpiration)
			{
				var oauth2AccessToken = CallIdentityUrl();
				_accessToken = oauth2AccessToken.Access_token;
				_tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(oauth2AccessToken.Expires_in);
				_sessionToken = new SessionToken { AccessToken = _accessToken, ExpirationUTC = _tokenExpiration };
				return _sessionToken;
			}

			return _sessionToken;
		}

		private OAuth2AccessToken CallIdentityUrl()
		{
			var tpaasBase64Key = ConstructTpaasBase64Key();
			var client = new HttpClient { BaseAddress = new Uri(_identityUrl) };
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {tpaasBase64Key}");
			var request =
				new HttpRequestMessage(HttpMethod.Post, string.Empty)
				{
					Content = new StringContent(
						"grant_type=client_credentials",
						Encoding.UTF8,
						"application/x-www-form-urlencoded")
				};

			var response = client.SendAsync(request).Result;
			response.EnsureSuccessStatusCode();
			var contents = response.Content.ReadAsStringAsync().Result;
			return JsonConvert.DeserializeObject<OAuth2AccessToken>(contents);
		}

		private string ConstructTpaasBase64Key()
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(_consumerKey + ":" + _consumerSecret));
		}
	}
}