namespace MasterDataProxies.Models
{
  /// <summary>
  /// Coordinate system (CS) definition file content and filename to be validated.
  /// </summary>
  public class CoordinateSystemFileValidationRequest
  {
    public byte[] csFileContent { get; private set; }
    public string csFileName { get; private set; }

    public static CoordinateSystemFileValidationRequest CreateCoordinateSystemFileValidationRequest(byte[] csFileContent, string csFileName)
    {
      CoordinateSystemFileValidationRequest tempCS = new CoordinateSystemFileValidationRequest();

      tempCS.csFileName = csFileName;
      tempCS.csFileContent = csFileContent;

      return tempCS;
    }
  }
}