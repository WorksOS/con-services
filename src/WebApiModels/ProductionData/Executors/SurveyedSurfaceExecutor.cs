
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;

using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

using ASNodeDecls;
using Microsoft.Extensions.Logging;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Executes POST, PUT,GET and DELETE methods on Surveyed Surfaces resource.
  /// </summary>
  /// 
  public class SurveyedSurfaceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    /// 
    public SurveyedSurfaceExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SurveyedSurfaceExecutor()
    {
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    /// <summary>
    /// Sends a request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">Request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    /// 
    protected virtual bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      surveyedSurfaces = null;

      return true;
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as an execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    protected virtual ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return null;
    }

    /// <summary>
    /// Surveyed Surface data executor (Post/Put/Get/Delete).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A Domain object.</param>
    /// <returns></returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      if ((object)item != null)
      {
        try
        {
          TSurveyedSurfaceDetails[] surveyedSurfaces;

          if (SendRequestToPdsClient(item, out surveyedSurfaces))
            result = ExecutionResult(surveyedSurfaces != null ? convertToSurveyedSurfaceDetails(surveyedSurfaces).ToArray() : null);
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to process Surveyed Surface data request."));
          }
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "No Surveyed Surface data request sent."));
      }

      return result;
    }

    private IEnumerable<SurveyedSurfaceDetails> convertToSurveyedSurfaceDetails(IEnumerable<TSurveyedSurfaceDetails> surveyedSurfaces)
    {
      return surveyedSurfaces.Select(surveyedSurface => SurveyedSurfaceDetails.CreateSurveyedSurfaceDetails(
        id              :surveyedSurface.ID,
        surveyedSurface : DesignDescriptor.CreateDesignDescriptor(
          surveyedSurface.DesignDescriptor.DesignID,
          FileDescriptor.CreateFileDescriptor(
            surveyedSurface.DesignDescriptor.FileSpaceID,
            surveyedSurface.DesignDescriptor.Folder,
            surveyedSurface.DesignDescriptor.FileName),
          surveyedSurface.DesignDescriptor.Offset),
        asAtDate        : surveyedSurface.AsAtDate,
        extents: BoundingBox3DGrid.CreatBoundingBox3DGrid(
          surveyedSurface.Extents.MinX, 
          surveyedSurface.Extents.MinY, 
          surveyedSurface.Extents.MinZ,
          surveyedSurface.Extents.MaxX,
          surveyedSurface.Extents.MaxY,
          surveyedSurface.Extents.MaxZ)));
    }
  }
}