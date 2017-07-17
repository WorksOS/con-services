using System;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Coordinate system(CS) definition file content and filename to be validated, then sent to Raptor.
  /// </summary>
  public class CoordinateSystemFile
  { 
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
    public long? projectId { get; private set; }

    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid? projectUid { get; private set; }

    /// <summary>
    /// The content of the CS definition file as an array of bytes.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileContent", Required = Required.Always)]
    public byte[] csFileContent { get; private set; }

    /// <summary>
    /// The name of the CS definition file.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
    public string csFileName { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private CoordinateSystemFile()
    {
      // ...
    }

    public static CoordinateSystemFile CreateCoordinateSystemFile(long projectId, byte[] csFileContent, string csFileName)
    {
      CoordinateSystemFile tempCS = new CoordinateSystemFile
      {
        projectId = projectId,
        csFileName = csFileName,
        csFileContent = csFileContent
      };

      return tempCS;
    }

  }
}