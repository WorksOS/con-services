using VSS.Authentication.Session.Model;

namespace VSS.Authentication.Session
{
	public interface ISession
	{
		void SetConfigurationValues(string identityUrl, string consumerKey, string consumerSecret);

		SessionToken GetToken();
	}
}