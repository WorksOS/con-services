using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Configuration;

namespace CCSS.CWS.Client
{
  public static class CwsClientMockExtensionMethods
  {
    public const string MOCK_ACCOUNT_KEY = "MOCK_CWS_ACCOUNT";
    public const string MOCK_DESIGN_KEY = "MOCK_CWS_DESIGN";
    public const string MOCK_DEVICE_KEY = "MOCK_CWS_DEVICE";
    public const string MOCK_PROFILE_KEY = "MOCK_CWS_PROFILE";
    public const string MOCK_PROJECT_KEY = "MOCK_CWS_PROJECT";
    public const string MOCK_USER_KEY = "MOCK_CWS_USER";

    public static IServiceCollection AddCwsClient<TInterface, TReal, TMock>(this IServiceCollection services, string variable)
      where TReal : class, TInterface
      where TMock : class, TInterface
      where TInterface : class
    {
      var configStore = services.BuildServiceProvider().GetService<IConfigurationStore>();
      // If we have not been told to use the real interface, then use the mock
      var useMock = configStore.GetValueBool(variable, true);
      if (!useMock)
      {
        services.AddTransient<TInterface, TReal>();
      }
      else
      {
        services.AddTransient<TInterface, TMock>();
      }

      return services;
    }
  }
}
