namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public interface IWebClientWrapper
  {
    byte[] DownloadData(string address);
  }
}
