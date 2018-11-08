using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Coordinate system settings result object.
  /// </summary>
  ///    
  public class CoordinateSystemSettingsResult : BaseDataResult
  {
    /// <summary>
    /// The coordinate system file name.
    /// </summary>
    /// 
    public string csName { get; set; }

    /// <summary>
    /// The name of the coordinate system group.
    /// </summary>
    /// 
    public string csGroup { get; set; }

    /// <summary>
    /// The coordinate system definition as an array of bytes.
    /// </summary>
    /// 
    public byte[] csib { get; set; }

    /// <summary>
    /// The coordinate system datum name.
    /// </summary>
    /// 
    public string datumName { get; set; }

    /// <summary>
    /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
    /// </summary>
    /// 
    public bool siteCalibration { get; set; }

    /// <summary>
    /// The coordinate system geoid model file name.
    /// </summary>
    /// 
    public string geoidFileName { get; set; }

    /// <summary>
    /// The coordinate system geoid model name.
    /// </summary>
    /// 
    public string geoidName { get; set; }

    /// <summary>
    /// The flag indicates whether or not there are datum grid data in a coordinate system definition.
    /// </summary>
    /// 
    public bool isDatumGrid { get; set; }

    /// <summary>
    /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
    /// </summary>
    /// 
    public bool unsupportedProjection { get; set; }

    /// <summary>
    /// The coordinate system latitude datum grid file name.
    /// </summary>
    /// 
    public string latitudeDatumGridFileName { get; set; }

    /// <summary>
    /// The coordinate system longitude datum grid file name.
    /// </summary>
    /// 
    public string longitudeDatumGridFileName { get; set; }

    /// <summary>
    /// The coordinate system height datum grid file name.
    /// </summary>
    /// 
    public string heightDatumGridFileName { get; set; }

    /// <summary>
    /// The coordinate system shift grid file name.
    /// </summary>
    /// 
    public string shiftGridName { get; set; }

    /// <summary>
    /// The coordinate system snake grid file name.
    /// </summary>
    /// 
    public string snakeGridName { get; set; }

    /// <summary>
    /// The coordinate system vertical datum name.
    /// </summary>
    /// 
    public string verticalDatumName { get; set; }

    
    public static CoordinateSystemSettingsResult CreateCoordinateSystemSettings
      (
            string csName,
            string csGroup,
            byte[] csib,
            string datumName,
            bool siteCalibration,
            string geoidFileName,
            string geoidName,
            bool isDatumGrid,
            string latitudeDatumGridFileName,
            string longitudeDatumGridFileName,
            string heightDatumGridFileName,
            string shiftGridName,
            string snakeGridName,
            string verticalDatumName,
            bool unsupportedProjection
        )
    {
      return new CoordinateSystemSettingsResult()
      {
        csName = csName,
        csGroup = csGroup,
        csib = csib,
        datumName = datumName,
        siteCalibration = siteCalibration,
        geoidFileName = geoidFileName,
        geoidName = geoidName,
        isDatumGrid = isDatumGrid,
        latitudeDatumGridFileName = latitudeDatumGridFileName,
        longitudeDatumGridFileName = longitudeDatumGridFileName,
        heightDatumGridFileName = heightDatumGridFileName,
        shiftGridName = shiftGridName,
        snakeGridName = snakeGridName,
        verticalDatumName = verticalDatumName,
        unsupportedProjection = unsupportedProjection
      };
    }
    
  
  }
}