using log4net;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssEmailService : IBssEmailService
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public void Send(string fromAddress, string toAddress, string subject, string body)
    {
      API.Email.AddToQueue(fromAddress,toAddress,subject,body,true, false,"BSS Email Svc");
      log.InfoFormat("Email sent to {0} with subject: {1}.", toAddress, subject);
    }
  }
}