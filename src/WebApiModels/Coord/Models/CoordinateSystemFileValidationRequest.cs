using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.Interfaces;

namespace VSS.Productivity3D.WebApiModels.Coord.Models
{
  /// <summary>
  /// Coordinate system (CS) definition file domain object. Model represents a coordinate system definition.
  /// </summary>
  public class CoordinateSystemFileValidationRequest : IValidatable, IIsProjectIDApplicable
  {
    public const int MAX_FILE_NAME_LENGTH = 256;

    /// <summary>
    /// The content of the CS definition file as an array of bytes.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileContent", Required = Required.Always)]
    [Required]
    public byte[] csFileContent { get; private set; }

    /// <summary>
    /// The name of the CS definition file.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "csFileName", Required = Required.Always)]
    [Required]
    [ValidFilename(MAX_FILE_NAME_LENGTH)]
    [MaxLength(MAX_FILE_NAME_LENGTH)]
    public string csFileName { get; private set; }


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
      CoordinateSystemFileValidationRequest tempCS = new CoordinateSystemFileValidationRequest();

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

    public bool HasProjectID()
    {
      return false;
    }
  }
}
