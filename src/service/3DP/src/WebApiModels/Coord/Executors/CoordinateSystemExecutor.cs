#if RAPTOR
using ASNodeDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Generic coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutor : RequestExecutorContainer
    {
      /// <summary>
      /// Default constructor for RequestExecutorContainer.Build
      /// </summary>
      public CoordinateSystemExecutor() 
      {
        ProcessErrorCodes();
      }

    /// <summary>
    /// Populates ContractExecutionStates with PDS error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

#if RAPTOR
    /// <summary>
    /// Converts Production Data Server (PDS) client CS data set to Coordinate Service one.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// 
    protected static CoordinateSystemSettings ConvertResult(TCoordinateSystemSettings settings)
    {
      return new CoordinateSystemSettings()
      { 
        // Coordinate System...
        CSName = settings.CSName,
        CSFileName = settings.CSFileName,
        CSGroup = settings.CSGroup,
        CSIB = settings.CSIB,
        // Ellipsoid...
        EllipsoidName = settings.EllipsoidName,
        EllipsoidSemiMajorAxis = settings.EllipsoidSemiMajorAxis,
        EllipsoidSemiMinorAxis = settings.EllipsoidSemiMinorAxis,
        EllipsoidFlattening = settings.EllipsoidFlattening,
        EllipsoidFirstEccentricity = settings.EllipsoidFirstEccentricity,
        EllipsoidSecondEccentricity = settings.EllipsoidSecondEccentricity,
        // Datum...
        DatumName = settings.DatumName,
        DatumMethod = settings.DatumMethod,
        DatumMethodType = RaptorConverters.convertCoordinateSystemDatumMethodType(settings.DatumMethodType),
        LatitudeShiftDatumGridFileName = settings.LatitudeDatumGridFileName,
        LongitudeShiftDatumGridFileName = settings.LongitudeDatumGridFileName,
        IsDatumGridHeightShiftDefined = settings.IsDatumGridHeightShiftDefined,
        HeightShiftDatumGridFileName = settings.HeightDatumGridFileName,
        DatumDirection = settings.DatumDirection,
        DatumTranslationX = settings.DatumTranslationX,
        DatumTranslationY = settings.DatumTranslationY,
        DatumTranslationZ = settings.DatumTranslationZ,
        DatumRotationX = settings.DatumRotationX,
        DatumRotationY = settings.DatumRotationY,
        DatumRotationZ = settings.DatumRotationZ,
        DatumScaleFactor = settings.DatumScaleFactor,
        DatumParametersFileName = settings.DatumParametersFileName,
        // Geoid...
        GeoidName = settings.GeoidName,
        GeoidMethod = settings.GeoidMethod,
        GeoidMethodType = RaptorConverters.convertCoordinateSystemGeoidMethodType(settings.GeoidMethodType),
        GeoidFileName = settings.GeoidFileName,
        GeoidConstantSeparation = settings.GeoidConstantSeparation,
        GeoidOriginX = settings.GeoidOriginX,
        GeoidOriginY = settings.GeoidOriginY,
        GeoidOriginZ = settings.GeoidOriginZ,
        GeoidTranslationZ = settings.GeoidTranslationZ,
        GeoidRotationX = settings.GeoidRotationX,
        GeoidRotationY = settings.GeoidRotationY,
        GeoidScaleFactor = settings.GeoidScaleFactor,
        // Projection
        ProjectionType = settings.ProjectionType,
        ProjectionParameters = RaptorConverters.convertCoordinateSystemProjectionParameters(settings.ProjectionParameters),
        AzimuthDirection = settings.AzimuthDirection,
        PositiveCoordinateDirection = settings.PositiveCoordinateDirection,
        // Others...
        SiteCalibration = settings.SiteCalibration,
        VerticalDatumName = settings.VerticalDatumName,
        ShiftGridName = settings.ShiftGridName,
        SnakeGridName = settings.SnakeGridName,
        UnsupportedProjection = settings.UnsupportedProjection
      };
    }

    /// <summary>
    /// Reference to Coordinate System settings. 
    /// </summary>
    /// 
    protected TCoordinateSystemSettings coordSystemSettings;
#endif

    /// <summary>
    /// Sends a request to TRex Gateway client.
    /// </summary>
    /// <param name="item">A domain object.</param>
    /// <returns>Result of the processed request from TRex Gateway.</returns>
    /// 
    protected virtual CoordinateSystemSettings SendRequestToTRexGatewayClient(object item)
    {
      return null;
    }

#if RAPTOR      
    /// <summary>
    /// Sends a request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">A domain object.</param>
    /// <returns>Result of the processed request from PDS.</returns>
    /// 
    protected virtual TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      return TASNodeErrorStatus.asneUnknown;
    }
#endif

    /// <summary>
    /// Coordinate system definition file executor (Post/Get).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A domain object.</param>
    /// <returns></returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_CS"))
#endif
          return SendRequestToTRexGatewayClient(item);
#if RAPTOR
        var code = SendRequestToPDSClient(item);
            
        if (code == TASNodeErrorStatus.asneOK)
            return ConvertResult(coordSystemSettings);

        throw CreateServiceException<CoordinateSystemExecutor>((int)code);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
