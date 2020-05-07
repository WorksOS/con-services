namespace VSS.Productivity3D.TagFileGateway.Common.Models.Sns
{
  public class SnsTagFile
  {
    public string OrgId { get; set; }

    public string FileName { get; set; }

    public byte[] Data { get; set; }

    public int FileSize { get; set; }

    public string DownloadUrl { get; set; }
  }
}
