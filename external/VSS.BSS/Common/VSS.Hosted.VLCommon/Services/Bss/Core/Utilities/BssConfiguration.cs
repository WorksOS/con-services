using System.Configuration;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssConfiguration : IBssConfiguration
  {
    public string ToEmailAddress
    {
      get { return ConfigurationManager.AppSettings["VSSTier3SupportEmail"]; }
    }

    public string FromEmailAddress 
    {
      get { return ConfigurationManager.AppSettings["MailFrom"]; }
    }

    public int MessageQueueFailedCountMaximum
    {
      get { return 5; }
    }
  }
}