namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssEmailService
  {
    void Send(string fromAddress, string toAddress, string subject, string body);
  }
}