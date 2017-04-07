using MasterDataProxies.Models;

namespace MasterDataProxies.Interfaces
{
  /// <summary>
  /// Coordinate system(CS) definition file content and filename to be validated, then sent to Raptor.
  /// </summary>
  public class CoordinateSystemFile : ProjectID
  {
    public byte[] csFileContent { get; private set; }
    public string csFileName { get; private set; }

    public static CoordinateSystemFile CreateCoordinateSystemFile(long projectId, byte[] csFileContent, string csFileName)
    {
      CoordinateSystemFile tempCS = new CoordinateSystemFile();

      tempCS.projectId = projectId;
      tempCS.csFileName = csFileName;
      tempCS.csFileContent = csFileContent;

      return tempCS;
    }
  }
}