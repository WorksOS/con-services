using System;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApiModels.Coord.ResultHandling
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
    public string csName { get; private set; }
    /// <summary>
    /// The coordinate system file name.
    /// </summary>
    public string csFileName { get; private set; }
    /// <summary>
    /// The name of the coordinate system group.
    /// </summary>
    public string csGroup { get; private set; }
    /// <summary>
    /// The coordinate system definition as an array of bytes.
    /// </summary>
    public byte[] csib { get; private set; }
    #endregion

    #region Ellipsoid details
    /// <summary>
    /// The coordinate system ellipsoid name.
    /// </summary>
    public string ellipsoidName { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid semi major axis value).
    /// </summary>
    public double ellipsoidSemiMajorAxis { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid semi minor axis value.
    /// </summary>
    public double ellipsoidSemiMinorAxis { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid flattening value.
    /// </summary>
    public double ellipsoidFlattening { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid first eccentricity.
    /// </summary>
    public double ellipsoidFirstEccentricity { get; private set; }
    /// <summary>
    /// The coordinate system ellipsoid second eccentricity.
    /// </summary>
    public double ellipsoidSecondEccentricity { get; private set; }
    #endregion

    #region Datum details
    /// <summary>
    /// The coordinate system datum name.
    /// </summary>
    public string datumName { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation method.
    /// </summary>
    public string datumMethod { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation method's type.
    /// 0 - Unknown
    /// 1 - WGS84
    /// 2 - Molodensky
    /// 3 - Multiple Regression 
    /// 4 - Seven Parameters
    /// 5 - Grid
    /// </summary>
    public TCoordinateSystemDatumMethod datumMethodType { get; private set; }
    /// <summary>
    /// The coordinate system latitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string latitudeShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The coordinate system longitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string longitudeShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The flag to indicate whether the coordinate system height shift for grid datum defined.
    /// Grid transformation method.
    /// </summary>
    public bool isDatumGridHeightShiftDefined { get; private set; }
    /// <summary>
    /// The coordinate system height shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string heightShiftDatumGridFileName { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation direction, i.e. WGS84 to Local or Local to WGS84.
    /// Seven Parameters transformation method.
    /// </summary>
    public string datumDirection { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumTranslationX { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumTranslationY { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumTranslationZ { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumRotationX { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumRotationY { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumRotationZ { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (scale factor).
    /// Seven Parameters transformation method.
    /// </summary>
    public double datumScaleFactor { get; private set; }
    /// <summary>
    /// The coordinate system datum transformation (multiple regression parameters file name).
    /// Multiple Regression transformation method.
    /// </summary>
    public string datumParametersFileName { get; private set; }
    #endregion

    #region Geoid details
    /// <summary>
    /// The coordinate system geoid model name.
    /// </summary>
    public string geoidName { get; private set; }
    /// <summary>
    /// The coordinate system geoid model method.
    /// </summary>
    public string geoidMethod { get; private set; }
    /// <summary>
    /// The coordinate system geoid model method's type.
    /// 0 - Unknown 
    /// 1 - Grid Geoid 
    /// 2 - Constant Separation Geoid 
    /// 3 - Site Calibrated Geoid Record
    /// </summary>
    public TCoordinateSystemGeoidMethod geoidMethodType { get; private set; }
    /// <summary> 
    /// The coordinate system grid geoid model file name.
    /// Grid Geoid method.
    /// </summary>
    public string geoidFileName { get; private set; }
    /// <summary>
    /// The coordinate system geoid model constant separation value.
    /// Constant Separation Geoid method.
    /// </summary>
    public double geoidConstantSeparation { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin X value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidOriginX { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin Y value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidOriginY { get; private set; }
    /// <summary>
    /// The coordinate system geoid model origin Z value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidOriginZ { get; private set; }
    /// <summary>
    /// The coordinate system geoid model translation along axis Z value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidTranslationZ { get; private set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis X value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidRotationX { get; private set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis Y value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidRotationY { get; private set; }
    /// <summary>
    /// The coordinate system geoid model scale factor value.
    /// Site Calibrated Geoid method.
    /// </summary>
    public double geoidScaleFactor { get; private set; }
    #endregion

    #region Projection details
    /// <summary>
    /// The coordinate system projection type.
    /// </summary>
    public string projectionType { get; private set; }
    /// <summary>
    /// The coordinate system projection parameters.
    /// </summary>
    public TProjectionParameters projectionParameters { get; private set; }
    /// <summary>
    /// The coordinate system azimuth direction.
    /// </summary>
    public string azimuthDirection { get; private set; }
    /// <summary>
    /// The coordinate system positive coordinate direction.
    /// </summary>
    public string positiveCoordinateDirection { get; private set; }
    #endregion

    #region Other details
    /// <summary>
    /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
    /// </summary>
    public bool siteCalibration { get; private set; }
    /// <summary>
    /// The coordinate system vertical datum name.
    /// </summary>
    public string verticalDatumName { get; private set; }
    /// <summary>
    /// The coordinate system shift grid file name.
    /// </summary>
    public string shiftGridName { get; private set; }
    /// <summary>
    /// The coordinate system snake grid file name.
    /// </summary>
    public string snakeGridName { get; private set; }
    /// <summary>
    /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
    /// </summary>
    /// 
    public bool unsupportedProjection { get; private set; }
    #endregion

    /// <summary>
    /// Private constructor.
    /// </summary>
    private CoordinateSystemSettings() 
    {}
    
    public static CoordinateSystemSettings CreateCoordinateSystemSettings
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
      TCoordinateSystemDatumMethod datumMethodType,
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
      TCoordinateSystemGeoidMethod geoidMethodType,
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
      TProjectionParameters projectionParameters,
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
      return new CoordinateSystemSettings
      {
        // Coordinate System...
        csName = csName, 
        csFileName = csFileName,
        csGroup = csGroup, 
        csib = csib,
        // Ellipsoid...
        ellipsoidName = ellipsoidName,
        ellipsoidSemiMajorAxis = ellipsoidSemiMajorAxis,
        ellipsoidSemiMinorAxis = ellipsoidSemiMinorAxis,
        ellipsoidFlattening = ellipsoidFlattening,
        ellipsoidFirstEccentricity = ellipsoidFirstEccentricity,
        ellipsoidSecondEccentricity = ellipsoidSecondEccentricity,
        // Datum...
        datumName = datumName,
        datumMethod = datumMethod,
        datumMethodType = datumMethodType,
        latitudeShiftDatumGridFileName = latitudeShiftDatumGridFileName,
        longitudeShiftDatumGridFileName = longitudeShiftDatumGridFileName,
        isDatumGridHeightShiftDefined = isDatumGridHeightShiftDefined,
        heightShiftDatumGridFileName = heightShiftDatumGridFileName,
        datumDirection = datumDirection,
        datumTranslationX = datumTranslationX,
        datumTranslationY = datumTranslationY,
        datumTranslationZ = datumTranslationZ,
        datumRotationX = datumRotationX,
        datumRotationY = datumRotationY,
        datumRotationZ = datumRotationZ,
        datumScaleFactor = datumScaleFactor,
        datumParametersFileName = datumParametersFileName,
        // Geoid...
        geoidName = geoidName,
        geoidMethod = geoidMethod,
        geoidMethodType = geoidMethodType,
        geoidFileName = geoidFileName,
        geoidConstantSeparation = geoidConstantSeparation,
        geoidOriginX = geoidOriginX,
        geoidOriginY = geoidOriginY,
        geoidOriginZ = geoidOriginZ,
        geoidTranslationZ = geoidTranslationZ,
        geoidRotationX = geoidRotationX,
        geoidRotationY = geoidRotationY,
        geoidScaleFactor = geoidScaleFactor,
        // Projection
        projectionType = projectionType,
        projectionParameters = projectionParameters,
        azimuthDirection = azimuthDirection,
        positiveCoordinateDirection = positiveCoordinateDirection,
        // Others...
        siteCalibration = siteCalibration,
        verticalDatumName = verticalDatumName,
        shiftGridName = shiftGridName,
        snakeGridName = snakeGridName,
        unsupportedProjection = unsupportedProjection
      };
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public override string ToString()
    {
      return 
        // Coordinate System...
        $"csName:{csName}, csFileName:{csFileName}, csGroup:{csGroup}, csib:{csib}, " +
        // Ellipsoid...
        $"ellipsoidName:{ellipsoidName}, ellipsoidSemiMajorAxis:{ellipsoidSemiMajorAxis}, ellipsoidSemiMinorAxis:{ellipsoidSemiMinorAxis}, " +
        $"ellipsoidFlattening:{ellipsoidFlattening}, ellipsoidFirstEccentricity:{ellipsoidFirstEccentricity}, ellipsoidSecondEccentricity:{ellipsoidSecondEccentricity}, " +
        // Datum...
        $"datumName:{datumName}, datumMethod:{datumMethod}, datumMethodType:{datumMethodType}, latitudeShiftDatumGridFileName:{latitudeShiftDatumGridFileName}, " +
        $"longitudeShiftDatumGridFileName:{longitudeShiftDatumGridFileName}, isDatumGridHeightShiftDefined:{isDatumGridHeightShiftDefined}, " +
        $"heightShiftDatumGridFileName:{heightShiftDatumGridFileName}, datumDirection:{datumDirection}, datumTranslationX:{datumTranslationX}, " +
        $"datumTranslationY:{datumTranslationY}, datumTranslationZ:{datumTranslationZ}, datumRotationX:{datumRotationX}, datumRotationY:{datumRotationY}, " +
        $"datumRotationZ:{datumRotationZ}, datumScaleFactor:{datumScaleFactor}, datumParametersFileName:{datumScaleFactor}, " +
        // Geoid...
        $"geoidName:{geoidName}, geoidMethod:{geoidMethod}, geoidMethodType:{geoidMethodType}, geoidFileName:{geoidFileName}, " +
        $"geoidConstantSeparation:{geoidFileName}, geoidOriginX:{geoidOriginX}, geoidOriginY:{geoidOriginY}, geoidOriginZ:{geoidOriginZ}, " +
        $"geoidTranslationZ:{geoidTranslationZ}, geoidRotationX:{geoidRotationX}, geoidRotationY:{geoidRotationY}, geoidScaleFactor:{geoidScaleFactor}, " +
        // Projection
        $"projectionType:{projectionType}, projectionParameters:{projectionParameters}, azimuthDirection:{azimuthDirection}, " +
        $"positiveCoordinateDirection:{azimuthDirection}, " +
        // Others...
        $"siteCalibration:{siteCalibration}, verticalDatumName:{verticalDatumName}, shiftGridName:{shiftGridName}, snakeGridName:{snakeGridName}, " +
        $"unsupportedProjection:{unsupportedProjection}";       
    }
  }
}