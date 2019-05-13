using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger log; //This should be created in the constructor of the request subclass (using logger.CreateLogger).
    protected IConfigurationStore configStore;
    protected string customerUid;

    public void Initialize(IConfigurationStore configurationStore, string customerUid)
    {
      configStore = configurationStore;
      this.customerUid = customerUid;
    }
  }
}
