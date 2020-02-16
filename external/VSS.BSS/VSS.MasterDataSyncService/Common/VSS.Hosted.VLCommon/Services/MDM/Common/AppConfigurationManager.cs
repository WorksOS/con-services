using System.Configuration;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Hosted.VLCommon.Services.MDM.Common
{
  public class AppConfigurationManager:IConfigurationManager
  {
    public string GetAppSetting(string key)
    {
      return ConfigurationManager.AppSettings[key];
    }
  }
}
