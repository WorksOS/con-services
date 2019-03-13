using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Coordinate system (CS) definition file content and filename to be validated.
  /// </summary>
  public class CoordinateSystemFileValidationRequest
  {
    /// <summary>
    /// The content of the CS definition file as an array of bytes.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileContent", Required = Required.Always)]
    public byte[] CSFileContent { get; set; }

    /// <summary>
    /// The name of the CS definition file.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
    public string CSFileName { get; set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private CoordinateSystemFileValidationRequest()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CoordinateSystemFileValidationRequest class.
    /// </summary>
    /// <param name="csFileContent">The content of the file.</param>
    /// <param name="csFileName">The file's name.</param>
    /// <returns>An instance of the CoordinateSystemFile class.</returns>
    ///
    public CoordinateSystemFileValidationRequest(byte[] csFileContent, string csFileName)
    {
      CSFileContent = csFileContent;
      CSFileName = csFileName;
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public void Validate()
    {
      // Validation rules might be placed in here...
    }
  }
}