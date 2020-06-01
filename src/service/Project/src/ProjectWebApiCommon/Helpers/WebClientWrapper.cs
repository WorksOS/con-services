using System.Net;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  /// Wraps the WebClient class.
  /// </summary>
  public class WebClientWrapper : IWebClientWrapper
  {
    private WebClient WebClient;

    public WebClientWrapper()
    {
      WebClient = new WebClient();
    }

    public byte[] DownloadData(string address)
    {
      return WebClient.DownloadData(address);
    }
  }
}
