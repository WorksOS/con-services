using System;
using System.Threading.Tasks;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IPipelineProcessorFactory
  {
    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests
    /// </summary>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria</param>
    Task<IPipelineProcessor> NewInstance<TSubGridsRequestArgument>(Guid requestDescriptor,
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
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      ILiftParameters liftParams);

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests, but does not perform the build action on the pipeline processor
    /// </summary>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria</param>
    IPipelineProcessor NewInstanceNoBuild<TSubGridsRequestArgument>(Guid requestDescriptor,
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
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      ILiftParameters liftParams);
  }
}
