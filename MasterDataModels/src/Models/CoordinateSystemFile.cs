using Newtonsoft.Json;

namespace MasterDataModels.Models
{
  /// <summary>
  /// Coordinate system(CS) definition file content and filename to be validated, then sent to Raptor.
  /// </summary>
  public class CoordinateSystemFile : ProjectID
  {
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

    /// <summary>
    /// Validation method.
    /// </summary>
    public new void Validate()
    {
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
    }
  }
}