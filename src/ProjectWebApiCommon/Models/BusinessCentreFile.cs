using System;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class BusinessCenterFile
  {
    /// <summary>
    /// Gets or sets the filespace id of the file location on TCC.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "FileSpaceID", Required = Required.Default)]
    public string FileSpaceId { get; set; }

    /// <summary>
    /// Gets or sets the path to the file location on TCC.
    /// </summary>
    [JsonProperty(PropertyName = "Path", Required = Required.Default)]
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the name of the file on TCC.
    /// </summary>
    [JsonProperty(PropertyName = "Name", Required = Required.Default)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the UTC date/time the file was created on TCC.
    /// </summary>
    [JsonProperty(PropertyName = "CreatedUTC", Required = Required.Default)]
    public DateTime CreatedUtc { get; set; }

    public static BusinessCenterFile CreateBusinessCenterFile(
      string fileSpaceId, string path, string name, DateTime createdUtc
    )
    {
      return new BusinessCenterFile
      {
        FileSpaceId = fileSpaceId,
        Path = path,
        Name = name,
        CreatedUtc = createdUtc
      };
    }
  }
}