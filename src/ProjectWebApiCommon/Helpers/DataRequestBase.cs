using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger log;
    protected IConfigurationStore configStore;
    protected string customerUid;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, string customerUid)
    {
      this.log = log;
      configStore = configurationStore;
      this.customerUid = customerUid;
    }
  }
}