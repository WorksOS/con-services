using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Factories
{
  public class PipelineProcessorFactory : IPipelineProcessorFactory
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="dataModelID"></param>
    /// <param name="gridDataType"></param>
    /// <param name="response"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="task"></param>
    /// <param name="pipeline"></param>
    /// <param name="requestAnalyser"></param>
    /// <param name="requireSurveyedSurfaceInformation"></param>
    /// <param name="requestRequiresAccessToDesignFileExistenceMap"></param>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria</param>
    public IPipelineProcessor NewInstance(Guid requestDescriptor,
      Guid dataModelID,
      GridDataType gridDataType,
      ISubGridsPipelinedReponseBase response,
      IFilterSet filters,
      DesignOffset cutFillDesign,
      ITRexTask task,
      ISubGridPipelineBase pipeline,
      IRequestAnalyser requestAnalyser,
      bool requireSurveyedSurfaceInformation,
      bool requestRequiresAccessToDesignFileExistenceMap,
      BoundingIntegerExtent2D overrideSpatialCellRestriction)
    {
      var pipelineProcessor = NewInstanceNoBuild
        (requestDescriptor, dataModelID, gridDataType, response, filters, cutFillDesign, 
        task, pipeline, requestAnalyser, requireSurveyedSurfaceInformation, requestRequiresAccessToDesignFileExistenceMap,
        overrideSpatialCellRestriction);

      if (!pipelineProcessor.Build())
      {
        Log.LogError($"Failed to build pipeline processor for request to model {dataModelID}");
        pipelineProcessor = null;
      }

      return pipelineProcessor;
    }

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests, but does not perform the build action on the pipeline processor
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="dataModelID"></param>
    /// <param name="gridDataType"></param>
    /// <param name="response"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesign"></param>
    /// <param name="task"></param>
    /// <param name="pipeline"></param>
    /// <param name="requestAnalyser"></param>
    /// <param name="requireSurveyedSurfaceInformation"></param>
    /// <param name="requestRequiresAccessToDesignFileExistenceMap"></param>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria</param>
    public IPipelineProcessor NewInstanceNoBuild(Guid requestDescriptor,
      Guid dataModelID,
      GridDataType gridDataType,
      ISubGridsPipelinedReponseBase response,
      IFilterSet filters,
      DesignOffset cutFillDesign,
      ITRexTask task,
      ISubGridPipelineBase pipeline,
      IRequestAnalyser requestAnalyser,
      bool requireSurveyedSurfaceInformation,
      bool requestRequiresAccessToDesignFileExistenceMap,
      BoundingIntegerExtent2D overrideSpatialCellRestriction)
    {
      return new PipelineProcessor
        (requestDescriptor, dataModelID, gridDataType, response, filters, cutFillDesign,
        task, pipeline, requestAnalyser, requireSurveyedSurfaceInformation, requestRequiresAccessToDesignFileExistenceMap,
        overrideSpatialCellRestriction);
    }
  }
}
