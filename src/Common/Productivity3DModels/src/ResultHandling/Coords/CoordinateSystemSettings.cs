using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
{
  /// <summary>
  /// Coordinate system settings result object.
  /// </summary>
  ///    
  public class CoordinateSystemSettings : ContractExecutionResult
  {
    #region Coordinate System details
    /// <summary>
    /// The coordinate system name.
    /// </summary>
    public string CSName { get; private set; }
    /// <summary>
    /// The coordinate system file name.
    /// </summary>
    public string CSFileName { get; private set; }
    /// <summary>
    /// The name of the coordinate system group.
    /// </summary>
    public string CSGroup { get; private set; }
    /// <summary>
    /// The coordinate system definition as an array of bytes.
    /// </summary>
    public byte[] CSIB { get; private set; }
    #endregion

    #region Ellipsoid details
    /// <summary>
    /// The coordinate system ellipsoid name.
    /// </summary>
    public string EllipsoidName { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid semi major axis value).
    /// </summary>
    public double EllipsoidSemiMajorAxis { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid semi minor axis value.
    /// </summary>
    public double EllipsoidSemiMinorAxis { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid flattening value.
    /// </summary>
    public double EllipsoidFlattening { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid first eccentricity.
    /// </summary>
    public double EllipsoidFirstEccentricity { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid second eccentricity.
    /// </summary>
    public double EllipsoidSecondEccentricity { get; private set; }
    #endregion

    #region Datum details
    /// <summary>
    /// The coordinate system datum name.
    /// </summary>
    public string DatumName { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation method.
    /// </summary>
    public string DatumMethod { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation method's type.
    /// 0 - Unknown
    /// 1 - WGS84
    /// 2 - Molodensky
    /// 3 - Multiple Regression 
    /// 4 - Seven Parameters
    /// 5 - Grid
    /// </summary>
    public CoordinateSystemDatumMethodType DatumMethodType { get; private set; }
    /// <summary>
    /// The coordinate system latitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string LatitudeShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The coordinate system longitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string LongitudeShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The flag to indicate whether the coordinate system height shift for grid datum defined.
    /// Grid transformation method.
    /// </summary>
    public bool IsDatumGridHeightShiftDefined { get; private set; }
    /// <summary>
    /// The coordinate system height shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string HeightShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation direction, i.e. WGS84 to Local or Local to WGS84.
    /// Seven Parameters transformation method.
    /// </summary>
    public string DatumDirection { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationX { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationY { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationZ { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationX { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationY { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationZ { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (scale factor).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumScaleFactor { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (multiple regression parameters file name).
    /// Multiple Regression transformation method.
    /// </summary>
    public string DatumParametersFileName { get; private set; }
    #endregion

    #region Geoid details
    /// <summary>
    /// The coordinate system geoid model name.
    /// </summary>
    public string GeoidName { get; private set; }
    /// <summary>
    /// The coordinate system geoid model method.
    /// </summary>
    public string GeoidMethod { get; private set; }
    /// <summary>
    /// The coordinate system geoid model method's type.
    /// 0 - Unknown 
    /// 1 - Grid Geoid 
    /// 2 - Constant Separation Geoid 
    /// 3 - Site Calibrated Geoid Record
    /// </summary>
    public CoordinateSystemGeoidMethodType GeoidMethodType { get; private set; }
    /// <summary> 
    /// The coordinate system grid geoid model file name.
    /// Grid Geoid method.
    /// </summary>
    public string GeoidFileName { get; private set; }
    /// <summary>
    /// The coordinate system geoid model constant separation value.
    /// Constant Separation Geoid method.
    /// </summary>
    public double GeoidConstantSeparation { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin X value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidOriginX { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin Y value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidOriginY { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin Z value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidOriginZ { get; private set; }
    /// <summary>
    /// The coordinate system geoid model translation along axis Z value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidTranslationZ { get; private set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis X value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidRotationX { get; private set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis Y value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidRotationY { get; private set; }
    /// <summary>
    /// The coordinate system geoid model scale factor value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double GeoidScaleFactor { get; private set; }
    #endregion

    #region Projection details
    /// <summary>
    /// The coordinate system projection type.
    /// </summary>
    public string ProjectionType { get; private set; }
    /// <summary>
    /// The coordinate system projection parameters.
    /// </summary>
    public ProjectionParameter[] ProjectionParameters { get; private set; }
    /// <summary>
    /// The coordinate system azimuth direction.
    /// </summary>
    public string AzimuthDirection { get; private set; }
    /// <summary>
    /// The coordinate system positive coordinate direction.
    /// </summary>
    public string PositiveCoordinateDirection { get; private set; }
    #endregion

    #region Other details
    /// <summary>
    /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
    /// </summary>
    public bool SiteCalibration { get; private set; }
    /// <summary>
    /// The coordinate system vertical datum name.
    /// </summary>
    public string VerticalDatumName { get; private set; }
    /// <summary>
    /// The coordinate system shift grid file name.
    /// </summary>
    public string ShiftGridName { get; private set; }
    /// <summary>
    /// The coordinate system snake grid file name.
    /// </summary>
    public string SnakeGridName { get; private set; }
    /// <summary>
    /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
    /// </summary>
    /// 
    public bool UnsupportedProjection { get; private set; }
    #endregion

    /// <summary>
    /// Private constructor.
    /// </summary>
    private CoordinateSystemSettings()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="csName"></param>
    /// <param name="csFileName"></param>
    /// <param name="csGroup"></param>
    /// <param name="csib"></param>
    /// <param name="ellipsoidName"></param>
    /// <param name="ellipsoidSemiMajorAxis"></param>
    /// <param name="ellipsoidSemiMinorAxis"></param>
    /// <param name="ellipsoidFlattening"></param>
    /// <param name="ellipsoidFirstEccentricity"></param>
    /// <param name="ellipsoidSecondEccentricity"></param>
    /// <param name="datumName"></param>
    /// <param name="datumMethod"></param>
    /// <param name="datumMethodType"></param>
    /// <param name="latitudeShiftDatumGridFileName"></param>
    /// <param name="longitudeShiftDatumGridFileName"></param>
    /// <param name="isDatumGridHeightShiftDefined"></param>
    /// <param name="heightShiftDatumGridFileName"></param>
    /// <param name="datumDirection"></param>
    /// <param name="datumTranslationX"></param>
    /// <param name="datumTranslationY"></param>
    /// <param name="datumTranslationZ"></param>
    /// <param name="datumRotationX"></param>
    /// <param name="datumRotationY"></param>
    /// <param name="datumRotationZ"></param>
    /// <param name="datumScaleFactor"></param>
    /// <param name="datumParametersFileName"></param>
    /// <param name="geoidName"></param>
    /// <param name="geoidMethod"></param>
    /// <param name="geoidMethodType"></param>
    /// <param name="geoidFileName"></param>
    /// <param name="geoidConstantSeparation"></param>
    /// <param name="geoidOriginX"></param>
    /// <param name="geoidOriginY"></param>
    /// <param name="geoidOriginZ"></param>
    /// <param name="geoidTranslationZ"></param>
    /// <param name="geoidRotationX"></param>
    /// <param name="geoidRotationY"></param>
    /// <param name="geoidScaleFactor"></param>
    /// <param name="projectionType"></param>
    /// <param name="projectionParameters"></param>
    /// <param name="azimuthDirection"></param>
    /// <param name="positiveCoordinateDirection"></param>
    /// <param name="siteCalibration"></param>
    /// <param name="verticalDatumName"></param>
    /// <param name="shiftGridName"></param>
    /// <param name="snakeGridName"></param>
    /// <param name="unsupportedProjection"></param>
    /// <returns></returns>
    public CoordinateSystemSettings
    (
      // Coordinate System...
      string csName,
      string csFileName,
      string csGroup,
      byte[] csib,
      // Ellipsoid...
      string ellipsoidName,
      double ellipsoidSemiMajorAxis,
      double ellipsoidSemiMinorAxis,
      double ellipsoidFlattening,
      double ellipsoidFirstEccentricity,
      double ellipsoidSecondEccentricity,
      // Datum...
      string datumName,
      string datumMethod,
      CoordinateSystemDatumMethodType datumMethodType,
      string latitudeShiftDatumGridFileName,
      string longitudeShiftDatumGridFileName,
      bool isDatumGridHeightShiftDefined,
      string heightShiftDatumGridFileName,
      string datumDirection,
      double datumTranslationX,
      double datumTranslationY,
      double datumTranslationZ,
      double datumRotationX,
      double datumRotationY,
      double datumRotationZ,
      double datumScaleFactor,
      string datumParametersFileName,
      // Geoid...
      string geoidName,
      string geoidMethod,
      CoordinateSystemGeoidMethodType geoidMethodType,
      string geoidFileName,
      double geoidConstantSeparation,
      double geoidOriginX,
      double geoidOriginY,
      double geoidOriginZ,
      double geoidTranslationZ,
      double geoidRotationX,
      double geoidRotationY,
      double geoidScaleFactor,
      // Projection
      string projectionType,
      ProjectionParameter[] projectionParameters,
      string azimuthDirection,
      string positiveCoordinateDirection,
      // Others...
      bool siteCalibration,
      string verticalDatumName,
      string shiftGridName,
      string snakeGridName,
      bool unsupportedProjection
    )
    {
      // Coordinate System...
      CSName = csName;
      CSFileName = csFileName;
      CSGroup = csGroup;
      CSIB = csib;
      // Ellipsoid...
      EllipsoidName = ellipsoidName;
      EllipsoidSemiMajorAxis = ellipsoidSemiMajorAxis;
      EllipsoidSemiMinorAxis = ellipsoidSemiMinorAxis;
      EllipsoidFlattening = ellipsoidFlattening;
      EllipsoidFirstEccentricity = ellipsoidFirstEccentricity;
      EllipsoidSecondEccentricity = ellipsoidSecondEccentricity;
      // Datum...
      DatumName = datumName;
      DatumMethod = datumMethod;
      DatumMethodType = datumMethodType;
      LatitudeShiftDatumGridFileName = latitudeShiftDatumGridFileName;
      LatitudeShiftDatumGridFileName = latitudeShiftDatumGridFileName;
      LongitudeShiftDatumGridFileName = longitudeShiftDatumGridFileName;
      IsDatumGridHeightShiftDefined = isDatumGridHeightShiftDefined;
      HeightShiftDatumGridFileName = heightShiftDatumGridFileName;
      DatumDirection = datumDirection;
      DatumTranslationX = datumTranslationX;
      DatumTranslationY = datumTranslationY;
      DatumTranslationZ = datumTranslationZ;
      DatumRotationX = datumRotationX;
      DatumRotationY = datumRotationY;
      DatumRotationZ = datumRotationZ;
      DatumScaleFactor = datumScaleFactor;
      DatumParametersFileName = datumParametersFileName;
      // Geoid...
      GeoidName = geoidName;
      GeoidMethod = geoidMethod;
      GeoidMethodType = geoidMethodType;
      GeoidFileName = geoidFileName;
      GeoidConstantSeparation = geoidConstantSeparation;
      GeoidOriginX = geoidOriginX;
      GeoidOriginY = geoidOriginY;
      GeoidOriginZ = geoidOriginZ;
      GeoidTranslationZ = geoidTranslationZ;
      GeoidRotationX = geoidRotationX;
      GeoidRotationY = geoidRotationY;
      GeoidScaleFactor = geoidScaleFactor;
      // Projection
      ProjectionType = projectionType;
      ProjectionParameters = projectionParameters;
      AzimuthDirection = azimuthDirection;
      PositiveCoordinateDirection = positiveCoordinateDirection;
      // Others...
      SiteCalibration = siteCalibration;
      VerticalDatumName = verticalDatumName;
      ShiftGridName = shiftGridName;
      SnakeGridName = snakeGridName;
      UnsupportedProjection = unsupportedProjection;
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public override string ToString()
    {
      return
        // Coordinate System...
        $"CSName:{CSName}, CSFileName:{CSFileName}, CSGroup:{CSGroup}, CSIB:{CSIB}, " +
        // Ellipsoid...
        $"EllipsoidName:{EllipsoidName}, EllipsoidSemiMajorAxis:{EllipsoidSemiMajorAxis}, EllipsoidSemiMinorAxis:{EllipsoidSemiMinorAxis}, " +
        $"EllipsoidFlattening:{EllipsoidFlattening}, EllipsoidFirstEccentricity:{EllipsoidFirstEccentricity}, EllipsoidSecondEccentricity:{EllipsoidSecondEccentricity}, " +
        // Datum...
        $"DatumName:{DatumName}, DatumMethod:{DatumMethod}, DatumMethodType:{DatumMethodType}, LatitudeShiftDatumGridFileName:{LatitudeShiftDatumGridFileName}, " +
        $"LongitudeShiftDatumGridFileName:{LongitudeShiftDatumGridFileName}, IsDatumGridHeightShiftDefined:{IsDatumGridHeightShiftDefined}, " +
        $"HeightShiftDatumGridFileName:{HeightShiftDatumGridFileName}, DatumDirection:{DatumDirection}, DatumTranslationX:{DatumTranslationX}, " +
        $"DatumTranslationY:{DatumTranslationY}, DatumTranslationZ:{DatumTranslationZ}, DatumRotationX:{DatumRotationX}, DatumRotationY:{DatumRotationY}, " +
        $"DatumRotationZ:{DatumRotationZ}, DatumScaleFactor:{DatumScaleFactor}, DatumParametersFileName:{DatumParametersFileName}, " +
        // Geoid...
        $"GeoidName:{GeoidName}, GeoidMethod:{GeoidMethod}, GeoidMethodType:{GeoidMethodType}, GeoidFileName:{GeoidFileName}, " +
        $"GeoidConstantSeparation:{GeoidConstantSeparation}, GeoidOriginX:{GeoidOriginX}, GeoidOriginY:{GeoidOriginY}, GeoidOriginZ:{GeoidOriginZ}, " +
        $"GeoidTranslationZ:{GeoidTranslationZ}, GeoidRotationX:{GeoidRotationX}, GeoidRotationY:{GeoidRotationY}, GeoidScaleFactor:{GeoidScaleFactor}, " +
        // Projection
        $"ProjectionType:{ProjectionType}, ProjectionParameters:{ProjectionParameters}, AzimuthDirection:{AzimuthDirection}, " +
        $"PositiveCoordinateDirection:{PositiveCoordinateDirection}, " +
        // Others...
        $"SiteCalibration:{SiteCalibration}, VerticalDatumName:{VerticalDatumName}, ShiftGridName:{ShiftGridName}, SnakeGridName:{SnakeGridName}, " +
        $"UnsupportedProjection:{UnsupportedProjection}";
    }
  }
}
