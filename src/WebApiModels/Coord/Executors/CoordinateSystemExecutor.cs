using ASNodeDecls;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
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
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    /// <summary>
    /// Converts Production Data Server (PDS) client CS data set to Coordinate Service one.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// 
    protected static CoordinateSystemSettings ConvertResult(TCoordinateSystemSettings settings)
    {
      return CoordinateSystemSettings.CreateCoordinateSystemSettings(
        // Coordinate System...
        settings.CSName,
        settings.CSFileName,
        settings.CSGroup,
        settings.CSIB,
        // Ellipsoid...
        settings.EllipsoidName,
        settings.EllipsoidSemiMajorAxis,
        settings.EllipsoidSemiMinorAxis,
        settings.EllipsoidFlattening,
        settings.EllipsoidFirstEccentricity,
        settings.EllipsoidSecondEccentricity,
        // Datum...
        settings.DatumName,
        settings.DatumMethod,
        settings.DatumMethodType,
        settings.LatitudeDatumGridFileName,
        settings.LongitudeDatumGridFileName,
        settings.IsDatumGridHeightShiftDefined,
        settings.HeightDatumGridFileName,
        settings.DatumDirection,
        settings.DatumTranslationX,
        settings.DatumTranslationY,
        settings.DatumTranslationZ,
        settings.DatumRotationX,
        settings.DatumRotationY,
        settings.DatumRotationZ,
        settings.DatumScaleFactor,
        settings.DatumParametersFileName,
        // Geoid...
        settings.GeoidName,
        settings.GeoidMethod,
        settings.GeoidMethodType,
        settings.GeoidFileName,
        settings.GeoidConstantSeparation,
        settings.GeoidOriginX,
        settings.GeoidOriginY,
        settings.GeoidOriginZ,
        settings.GeoidTranslationZ,
        settings.GeoidRotationX,
        settings.GeoidRotationY,
        settings.GeoidScaleFactor,
        // Projection
        settings.ProjectionType,
        settings.ProjectionParameters,
        settings.AzimuthDirection,
        settings.PositiveCoordinateDirection,
        // Others...
        settings.SiteCalibration,
        settings.VerticalDatumName,
        settings.ShiftGridName,
        settings.SnakeGridName,
        settings.UnsupportedProjection
      );
    }

    /// <summary>
    /// Reference to Coordinate System settings. 
    /// </summary>
    /// 
    protected TCoordinateSystemSettings coordSystemSettings;

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
        var code = SendRequestToPDSClient(item);
            
        if (code == TASNodeErrorStatus.asneOK)
            return ConvertResult(coordSystemSettings);

        throw CreateServiceException<CoordinateSystemExecutor>((int)code);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
