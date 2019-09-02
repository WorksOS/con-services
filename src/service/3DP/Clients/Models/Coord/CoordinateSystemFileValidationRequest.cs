using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Productivity3D.Models.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Models.Coord
{
  /// <summary>
  /// Coordinate system (CS) definition file domain object. Model represents a coordinate system definition.
  /// </summary>
  public class CoordinateSystemFileValidationRequest : IIsProjectIDApplicable
  {
    public const int MAX_FILE_NAME_LENGTH = 256;

    /// <summary>
    /// The content of the CS definition file as an array of bytes.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileContent", Required = Required.Always)]
    [Required]
    public byte[] CSFileContent { get; private set; }

    /// <summary>
    /// The name of the CS definition file.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
    [Required]
    [ValidFilename(MAX_FILE_NAME_LENGTH)]
    [MaxLength(MAX_FILE_NAME_LENGTH)]
    public string CSFileName { get; private set; }


    /// <summary>
    /// Default private constructor.
    /// </summary>
    /// 
    private CoordinateSystemFileValidationRequest()
    {
      // ...
    }
    
    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="csFileContent">The content of the file.</param>
    /// <param name="csFileName">The file's name.</param>
    /// <returns>An instance of the CoordinateSystemFile class.</returns>
    /// 
    public CoordinateSystemFileValidationRequest(byte[] csFileContent, string csFileName)
    {
      CSFileName = csFileName;
      CSFileContent = csFileContent;
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public void Validate()
    {
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
    }

    public bool HasProjectID()
    {
      return false;
    }
  }
}
