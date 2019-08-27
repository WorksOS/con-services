using System.Net;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;

namespace CoordinateSystemFileResolver.Utils
{
  public class EnvironmentHelper : IEnvironmentHelper
  {
    private readonly IConfigurationStore _configStore;
    private readonly IServiceExceptionHandler _serviceExceptionHandler;

    public EnvironmentHelper(IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler)
    {
      _configStore = configStore;
      _serviceExceptionHandler = serviceExceptionHandler;
    }

    public string GetVariable(string key, int errorNumber)
    {
      var value = _configStore.GetValueString(key);

      if (string.IsNullOrEmpty(value))
      {
        _serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorNumber, $"Missing environment variable {key}");
      }

      return value;
    }
  }
}
