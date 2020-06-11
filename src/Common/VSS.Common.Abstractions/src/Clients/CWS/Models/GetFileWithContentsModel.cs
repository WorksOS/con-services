using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class GetFileWithContentsModel : GetFileResponseModel
  {
    public GetFileWithContentsModel(GetFileResponseModel response, byte[] contents)
    {
      FileContents = contents;
      FileName = response.FileName;
      FileSpaceId = response.FileSpaceId;
      DownloadUrl = response.DownloadUrl;
    }

    /// <summary>
    /// Contents of downloaded file
    /// </summary>
    [JsonProperty("fileContents")]
    public byte[] FileContents { get; set; }
  }
}
