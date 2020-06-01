using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
{
  /// <summary>
  /// Coordinate system settings result object.
  /// </summary>
  ///    
  public class CoordinateSystemSettings : ContractExecutionResult, IMasterDataModel
  {
    #region Coordinate System details
    /// <summary>
    /// The coordinate system name.
    /// </summary>
    public string CSName { get; set; }
    /// <summary>
    /// The coordinate system file name.
    /// </summary>
    public string CSFileName { get; set; }
    /// <summary>
    /// The name of the coordinate system group.
    /// </summary>
    public string CSGroup { get; set; }
    /// <summary>
    /// The coordinate system definition as an array of bytes.
    /// </summary>
    public byte[] CSIB { get; set; }
    #endregion

    #region Ellipsoid details
    /// <summary>
    /// The coordinate system ellipsoid name.
    /// </summary>
    public string EllipsoidName { get; set; }
    /// <summary>
    /// The coordinate system ellipsoid semi major axis value).
    /// </summary>
    public double EllipsoidSemiMajorAxis { get; set; }
    /// <summary>
    /// The coordinate system ellipsoid semi minor axis value.
    /// </summary>
    public double EllipsoidSemiMinorAxis { get; set; }
    /// <summary>
    /// The coordinate system ellipsoid flattening value.
    /// </summary>
    public double EllipsoidFlattening { get; set; }
    /// <summary>
    /// The coordinate system ellipsoid first eccentricity.
    /// </summary>
    public double EllipsoidFirstEccentricity { get; set; }
    /// <summary>
    /// The coordinate system ellipsoid second eccentricity.
    /// </summary>
    public double EllipsoidSecondEccentricity { get; set; }
    #endregion

    #region Datum details
    /// <summary>
    /// The coordinate system datum name.
    /// </summary>
    public string DatumName { get; set; }
    /// <summary>
    /// The coordinate system datum transformation method.
    /// </summary>
    public string DatumMethod { get; set; }
    /// <summary>
    /// The coordinate system datum transformation method's type.
    /// 0 - Unknown
    /// 1 - WGS84
    /// 2 - Molodensky
    /// 3 - Multiple Regression 
    /// 4 - Seven Parameters
    /// 5 - Grid
    /// </summary>
    public CoordinateSystemDatumMethodType DatumMethodType { get; set; }
    /// <summary>
    /// The coordinate system latitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string LatitudeShiftDatumGridFileName { get; set; }
    /// <summary>
    /// The coordinate system longitude shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string LongitudeShiftDatumGridFileName { get; set; }
    /// <summary>
    /// The flag to indicate whether the coordinate system height shift for grid datum defined.
    /// Grid transformation method.
    /// </summary>
    public bool IsDatumGridHeightShiftDefined { get; set; }
    /// <summary>
    /// The coordinate system height shift for grid datum file name.
    /// Grid transformation method.
    /// </summary>
    public string HeightShiftDatumGridFileName { get; set; }
    /// <summary>
    /// The coordinate system datum transformation direction, i.e. WGS84 to Local or Local to WGS84.
    /// Seven Parameters transformation method.
    /// </summary>
    public string DatumDirection { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationX { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationY { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (translation along axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumTranslationZ { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis X value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationX { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Y value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationY { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (rotation around axis Z value).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumRotationZ { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (scale factor).
    /// Seven Parameters transformation method.
    /// </summary>
    public double DatumScaleFactor { get; set; }
    /// <summary>
    /// The coordinate system datum transformation (multiple regression parameters file name).
    /// Multiple Regression transformation method.
    /// </summary>
    public string DatumParametersFileName { get; set; }
    #endregion

    #region CwsGeoid details
    /// <summary>
    /// The coordinate system geoid model name.
    /// </summary>
    public string GeoidName { get; set; }
    /// <summary>
    /// The coordinate system geoid model method.
    /// </summary>
    public string GeoidMethod { get; set; }
    /// <summary>
    /// The coordinate system geoid model method's type.
    /// 0 - Unknown 
    /// 1 - Grid CwsGeoid 
    /// 2 - Constant Separation CwsGeoid 
    /// 3 - Site Calibrated CwsGeoid Record
    /// </summary>
    public CoordinateSystemGeoidMethodType GeoidMethodType { get; set; }
    /// <summary> 
    /// The coordinate system grid geoid model file name.
    /// Grid CwsGeoid method.
    /// </summary>
    public string GeoidFileName { get; set; }
    /// <summary>
    /// The coordinate system geoid model constant separation value.
    /// Constant Separation CwsGeoid method.
    /// </summary>
    public double GeoidConstantSeparation { get; set; }
    /// <summary>
    /// The coordinate system geoid model origin X value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidOriginX { get; set; }
    /// <summary>
    /// The coordinate system geoid model origin Y value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidOriginY { get; set; }
    /// <summary>
    /// The coordinate system geoid model origin Z value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidOriginZ { get; set; }
    /// <summary>
    /// The coordinate system geoid model translation along axis Z value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidTranslationZ { get; set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis X value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidRotationX { get; set; }
    /// <summary>
    /// The coordinate system geoid model rotation around axis Y value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidRotationY { get; set; }
    /// <summary>
    /// The coordinate system geoid model scale factor value.
    /// Site Calibrated CwsGeoid method.
    /// </summary>
    public double GeoidScaleFactor { get; set; }
    #endregion

    #region Projection details
    /// <summary>
    /// The coordinate system projection type.
    /// </summary>
    public string ProjectionType { get; set; }
    /// <summary>
    /// The coordinate system projection parameters.
    /// </summary>
    public ProjectionParameter[] ProjectionParameters { get; set; }
    /// <summary>
    /// The coordinate system azimuth direction.
    /// </summary>
    public string AzimuthDirection { get; set; }
    /// <summary>
    /// The coordinate system positive coordinate direction.
    /// </summary>
    public string PositiveCoordinateDirection { get; set; }
    #endregion

    #region Other details
    /// <summary>
    /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
    /// </summary>
    public bool SiteCalibration { get; set; }
    /// <summary>
    /// The coordinate system vertical datum name.
    /// </summary>
    public string VerticalDatumName { get; set; }
    /// <summary>
    /// The coordinate system shift grid file name.
    /// </summary>
    public string ShiftGridName { get; set; }
    /// <summary>
    /// The coordinate system snake grid file name.
    /// </summary>
    public string SnakeGridName { get; set; }
    /// <summary>
    /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
    /// </summary>
    /// 
    public bool UnsupportedProjection { get; set; }
    #endregion

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
        // CwsGeoid...
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

    public List<string> GetIdentifiers()
    {
      return new List<string>();
    }
  }
}
