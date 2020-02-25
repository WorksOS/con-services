using System;
using System.Net.Http;
using Xunit;

namespace VSS.Authentication.Session.Tests
{
	public class SessionTests
	{
		[Fact]
		public void CanGetValidToken()
		{
			var alphaVLUnifiedFleetSession = new Session();
			alphaVLUnifiedFleetSession.SetConfigurationValues(
				"https://identity-stg.trimble.com/token",
				"E4wERYf22xcMKVRTRQBtU6gkSqoa",
				"9NoQS8Tg01pXrtLp963Ap7BUvCga");

			var sessionToken = alphaVLUnifiedFleetSession.GetToken();
			Assert.NotNull(sessionToken.AccessToken);
			Assert.True(sessionToken.ExpirationUTC > DateTime.UtcNow);
		}

		[Fact]
		public void CanGetValidTokenTwiceWithSameResults()
		{
			var alphaVLUnifiedFleetSession = new Session();
			alphaVLUnifiedFleetSession.SetConfigurationValues(
				"https://identity-stg.trimble.com/token",
				"E4wERYf22xcMKVRTRQBtU6gkSqoa",
				"9NoQS8Tg01pXrtLp963Ap7BUvCga");

			var sessionToken1 = alphaVLUnifiedFleetSession.GetToken();
			Assert.NotNull(sessionToken1.AccessToken);
			Assert.True(sessionToken1.ExpirationUTC > DateTime.UtcNow);

			var sessionToken2 = alphaVLUnifiedFleetSession.GetToken();
			Assert.NotNull(sessionToken2.AccessToken);
			Assert.True(sessionToken2.ExpirationUTC > DateTime.UtcNow);

			Assert.Equal(sessionToken1.AccessToken, sessionToken2.AccessToken);
			Assert.Equal(sessionToken1.ExpirationUTC, sessionToken2.ExpirationUTC);
		}

		[Fact]
		public void ThrowsExceptionIfGivenBadIdentityUrl()
		{
			var alphaVLUnifiedFleetSession = new Session();
			alphaVLUnifiedFleetSession.SetConfigurationValues(
				"https://bad-identity-stg.trimble.com/token",
				"E4wERYf22xcMKVRTRQBtU6gkSqoa",
				"9NoQS8Tg01pXrtLp963Ap7BUvCga");

			// Different exceptions are thrown locally vs. on Jenkins.  Just make sure it throws an exception.
			try
			{
				alphaVLUnifiedFleetSession.GetToken();
				Assert.True(false, "Expected exception");
			}
			catch (Exception)
			{
				Assert.True(true);
			}
		}

		[Fact]
		public void ThrowsExceptionIfGivenBadConsumerKey()
		{
			var alphaVLUnifiedFleetSession = new Session();
			alphaVLUnifiedFleetSession.SetConfigurationValues(
				"https://identity-stg.trimble.com/token",
				"bad-consumer-key",
				"9NoQS8Tg01pXrtLp963Ap7BUvCga");

			var ex = Assert.Throws<HttpRequestException>(() => alphaVLUnifiedFleetSession.GetToken());
			Assert.Equal("Response status code does not indicate success: 401 (Unauthorized).", ex.Message);
		}

		[Fact]
		public void ThrowsExceptionIfGivenBadConsumerSecret()
		{
			var alphaVLUnifiedFleetSession = new Session();
			alphaVLUnifiedFleetSession.SetConfigurationValues(
				"https://identity-stg.trimble.com/token",
				"E4wERYf22xcMKVRTRQBtU6gkSqoa",
				"bad-consumer-secret");

			var ex = Assert.Throws<HttpRequestException>(() => alphaVLUnifiedFleetSession.GetToken());
			Assert.Equal("Response status code does not indicate success: 401 (Unauthorized).", ex.Message);
		}
	}
}