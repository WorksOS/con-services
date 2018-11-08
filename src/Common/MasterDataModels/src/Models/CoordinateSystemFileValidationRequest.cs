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
    public byte[] csFileContent { get; set; }

    /// <summary>
    /// The name of the CS definition file.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
    public string csFileName { get; set; }

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
    public static CoordinateSystemFileValidationRequest CreateCoordinateSystemFileValidationRequest(byte[] csFileContent, string csFileName)
    {
      var tempCS = new CoordinateSystemFileValidationRequest();

      tempCS.csFileName = csFileName;
      tempCS.csFileContent = csFileContent;

      return tempCS;
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public void Validate()
    {
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
    }
  }
}