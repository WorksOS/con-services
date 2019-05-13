using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Models;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  public class CoordinateSystemBaseExecutor : BaseExecutor
  {
    private const string STR_DATUM_DIRECTION_TO_WGS84 = "Local To WGS84";
    private const string STR_DATUM_DIRECTION_TO_LOCAL = "WGS84 To Local";
    private const string AZIMUTH_STR = "Azimuth";
    private const string NORTH_STR = "North";
    private const string SOUTH_STR = "South";
    private const string EAST_STR = "East";
    private const string WEST_STR = "West";
    private const string ORIGIN_LATITUDE = "Origin Latitude";
    private const string ORIGIN_LONGITUDE = "Origin Longitude";
    private const string ORIGIN_NORTH = "Origin North";
    private const string ORIGIN_EAST = "Origin East";
    private const string ORIGIN_SCALE = "Origin Scale";
    private const string MOLODENSKY_DATUM = "Molodensky";
    private const string SEVEN_PARAMETER_DATUM = "SevenParameter";
    private const string MULTIPLE_REGRESSION_DATUM = "MultipleRegressionDatum";
    private const string GRID_DATUM = "GridDatum";
    private const string WGS84_DATUM = "WGS84";

    public CoordinateSystemBaseExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemBaseExecutor()
    {
    }

    protected ContractExecutionResult ConvertResult(string csFileName, CoordinateSystem coordSystem)
    {
      var azimuthDirection = coordSystem.ZoneInfo.IsSouthAzimuth ? SOUTH_STR : NORTH_STR;

      var latAxis = coordSystem.ZoneInfo.IsSouthGrid ? SOUTH_STR : NORTH_STR;
      var lonAxis = coordSystem.ZoneInfo.IsWestGrid ? WEST_STR : EAST_STR;

      return new CoordinateSystemSettings()
      { 
        // Coordinate System...
        CSName = coordSystem.SystemName,
        CSFileName = csFileName,
        CSGroup = coordSystem.ZoneInfo.ZoneGroupName,
        CSIB = Encoding.ASCII.GetBytes(coordSystem.Id),
        // Ellipsoid...
        EllipsoidName = coordSystem.DatumInfo.EllipseName,
        EllipsoidSemiMajorAxis = coordSystem.DatumInfo.EllipseA,
        EllipsoidSemiMinorAxis = 0.0, // ellipsoidSemiMinorAxis
        EllipsoidFlattening = coordSystem.DatumInfo.EllipseInverseFlat,
        EllipsoidFirstEccentricity = 0.0, // ellipsoidFirstEccentricity
        EllipsoidSecondEccentricity = 0.0, // ellipsoidSecondEccentricity
        // Datum...
        DatumName = coordSystem.DatumInfo.DatumName,
        DatumMethod = coordSystem.DatumInfo.DatumType,
        DatumMethodType = ConvertCoordinateSystemDatumMethodType(coordSystem.DatumInfo.DatumType),
        LatitudeShiftDatumGridFileName = coordSystem.DatumInfo.LatitudeShiftGridFileName,
        LongitudeShiftDatumGridFileName = coordSystem.DatumInfo.LongitudeShiftGridFileName,
        IsDatumGridHeightShiftDefined = coordSystem.DatumInfo.HeightShiftGridFileName != string.Empty,
        HeightShiftDatumGridFileName = coordSystem.DatumInfo.HeightShiftGridFileName,
        DatumDirection = coordSystem.DatumInfo.DirectionIsLocalToWGS84 ? STR_DATUM_DIRECTION_TO_WGS84 : STR_DATUM_DIRECTION_TO_LOCAL,
        DatumTranslationX = coordSystem.DatumInfo.TranslationX,
        DatumTranslationY = coordSystem.DatumInfo.TranslationY,
        DatumTranslationZ = coordSystem.DatumInfo.TranslationZ,
        DatumRotationX = coordSystem.DatumInfo.RotationX,
        DatumRotationY = coordSystem.DatumInfo.RotationY,
        DatumRotationZ = coordSystem.DatumInfo.RotationZ,
        DatumScaleFactor = coordSystem.DatumInfo.Scale,
        DatumParametersFileName = string.Empty, // datumParametersFileName
        // Geoid...
        GeoidName = coordSystem.GeooidInfo?.GeoidName,
        GeoidMethod = string.Empty, // geoidMethod
        GeoidMethodType = CoordinateSystemGeoidMethodType.Unknown, // datumMethodType, convert to CoordinateSystemGeoidMethodType
        GeoidFileName = coordSystem.GeooidInfo?.GeoidFileName,
        GeoidConstantSeparation = 0.0, // geoidConstantSeparation 
        GeoidOriginX = 0.0, // geoidOriginX
        GeoidOriginY = 0.0, // geoidOriginY
        GeoidOriginZ = 0.0, // geoidOriginZ
        GeoidTranslationZ = 0.0, // geoidTranslationZ
        GeoidRotationX = 0.0, // geoidRotationX
        GeoidRotationY = 0.0, // geoidRotationY
        GeoidScaleFactor = 0.0, // geoidScaleFactor
        // Projection
        ProjectionType = coordSystem.ZoneInfo.ZoneType,
        ProjectionParameters = GetProjectionParameters(coordSystem.ZoneInfo),
        AzimuthDirection = $"{AZIMUTH_STR} {azimuthDirection}",
        PositiveCoordinateDirection = $"{latAxis} {lonAxis}",
        // Others...
        SiteCalibration = coordSystem.ZoneInfo.HorizontalAdjustment != null || coordSystem.ZoneInfo.VerticalAdjustment != null,
        VerticalDatumName = string.Empty, // verticalDatumName
        ShiftGridName = string.Empty, // shiftGridName
        SnakeGridName = string.Empty, // snakeGridName
        UnsupportedProjection = false // unsupportedProjection
      };
    }

    private ProjectionParameter[] GetProjectionParameters(ZoneInfo coordSystemZoneInfo)
    {
      return new[]
      {
        new ProjectionParameter() { Name = ORIGIN_LATITUDE, Value = coordSystemZoneInfo.OriginLatitude},
        new ProjectionParameter() { Name = ORIGIN_LONGITUDE, Value = coordSystemZoneInfo.OriginLongitude},
        new ProjectionParameter() { Name = ORIGIN_NORTH, Value = coordSystemZoneInfo.OriginNorth},
        new ProjectionParameter() { Name = ORIGIN_EAST, Value = coordSystemZoneInfo.OriginEast},
        new ProjectionParameter() { Name = ORIGIN_SCALE, Value = coordSystemZoneInfo.OriginScale}
      };
    }

    private CoordinateSystemDatumMethodType ConvertCoordinateSystemDatumMethodType(string datumInfoDatumType)
    {
      switch (datumInfoDatumType)
      {
        case MOLODENSKY_DATUM: return CoordinateSystemDatumMethodType.MolodenskyDatum;
        case SEVEN_PARAMETER_DATUM: return CoordinateSystemDatumMethodType.SevenParameterDatum;
        case MULTIPLE_REGRESSION_DATUM: return CoordinateSystemDatumMethodType.MultipleRegressionDatum;
        case GRID_DATUM: return CoordinateSystemDatumMethodType.GridDatum;
        case WGS84_DATUM: return CoordinateSystemDatumMethodType.WGS84Datum;
        default: return CoordinateSystemDatumMethodType.Unknown;
      }
    }

  }
}
