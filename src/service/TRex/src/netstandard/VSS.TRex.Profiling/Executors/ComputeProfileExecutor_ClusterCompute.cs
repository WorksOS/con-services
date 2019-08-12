using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Types;
using VSS.TRex.Types.Types;

namespace VSS.TRex.Profiling.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeProfileExecutor_ClusterCompute<T> where T: class, IProfileCellBase, new()
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeProfileExecutor_ClusterCompute<T>>();

    private readonly Guid ProjectID;
    private readonly GridDataType ProfileTypeRequired;
    private readonly XYZ[] NEECoords;
    private readonly IFilterSet Filters;
    private readonly ProfileStyle ProfileStyle;
    private readonly VolumeComputationType VolumeType;
    private readonly IOverrideParameters Overrides;
    private readonly ILiftParameters LiftParams;

    private const int INITIAL_PROFILE_LIST_SIZE = 1000;

    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    private readonly DesignOffset Design;

    private ISubGridSegmentCellPassIterator CellPassIterator;
    private ISubGridSegmentIterator SegmentIterator;

    private ISiteModel SiteModel;

    /// <summary>
    /// Constructs the profile analysis executor
    /// </summary>
    public ComputeProfileExecutor_ClusterCompute(ProfileStyle profileStyle, Guid projectID, GridDataType profileTypeRequired, XYZ[] nEECoords, IFilterSet filters,
      // externalRequestDescriptor: TASNodeRequestDescriptor;
      DesignOffset design, bool returnAllPassesAndLayers, VolumeComputationType volumeType, IOverrideParameters overrides, ILiftParameters liftParams)
    {
      ProfileStyle = profileStyle;
      ProjectID = projectID;
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      Filters = filters;
      Design = design;
      VolumeType = volumeType;
      Overrides = overrides;
      LiftParams = liftParams;
    }

    /// <summary>
    /// Create and configure the segment iterator to be used
    /// </summary>
    /// <param name="passFilter"></param>
    private void SetupForCellPassStackExamination(ICellPassAttributeFilter passFilter)
    {      
      SegmentIterator = new SubGridSegmentIterator(null, null, SiteModel.PrimaryStorageProxy);

      if (passFilter.ReturnEarliestFilteredCellPass ||
          (passFilter.HasElevationTypeFilter && passFilter.ElevationType == ElevationType.First))
        SegmentIterator.IterationDirection = IterationDirection.Forwards;
      else
        SegmentIterator.IterationDirection = IterationDirection.Backwards;

      if (passFilter.HasMachineFilter)
        SegmentIterator.SetMachineRestriction(passFilter.GetMachineIDsSet());

      // Create and configure the cell pass iterator to be used
      CellPassIterator = new SubGridSegmentCellPassIterator_NonStatic
      {
        SegmentIterator = SegmentIterator
      }; 

      CellPassIterator.SetTimeRange(passFilter.HasTimeFilter, passFilter.StartTime, passFilter.EndTime);
    }

    /// <summary>
    /// Executes the profiler logic in the cluster compute context where each cluster node processes its fraction of the work and returns the
    /// results to the application service context
    /// </summary>
    public async Task<ProfileRequestResponse<T>> ExecuteAsync()
    {
      LiftParams.CCVSummaryTypes |= CCVSummaryTypes.Compaction;
      LiftParams.MDPSummaryTypes |= MDPSummaryTypes.Compaction;

      ProfileRequestResponse<T> Response = null;
      try
      {
        var ProfileCells = new List<T>(INITIAL_PROFILE_LIST_SIZE);

        try
        {
          // Note: Start/end point lat/lon fields have been converted into grid local coordinate system by this point
          if (NEECoords.Length > 1)
            Log.LogInformation($"#In#: DataModel {ProjectID}, Vertices:{NEECoords[0]} -> {NEECoords[1]}");
          else
            Log.LogWarning($"#In#: DataModel {ProjectID}, Note! vertices list has insufficient vertices (min of 2 required)");

          SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(ProjectID);

          if (SiteModel == null)
          {
            Log.LogWarning($"Failed to locate site model {ProjectID}");
            return Response = new ProfileRequestResponse<T> {ResultStatus = RequestErrorStatus.NoSuchDataModel};
          }

          // Obtain the sub grid existence map for the project
          var ProdDataExistenceMap = SiteModel.ExistenceMap;

          var PopulationControl = new FilteredValuePopulationControl();
          PopulationControl.PreparePopulationControl(ProfileTypeRequired, LiftParams, Filters.Filters[0].AttributeFilter);

          IDesign design = null;
          if (Design != null && Design.DesignID != Guid.Empty)
          {

            design = SiteModel.Designs.Locate(Design.DesignID);

            if (design == null)
              throw new ArgumentException($"Design {Design.DesignID} is unknown in project {SiteModel.ID}");
          }

          Log.LogInformation("Creating IProfileBuilder");
          var Profiler = DIContext.Obtain<IProfilerBuilder<T>>();
          if (Profiler == null)
          {
            Log.LogWarning($"Failed to create IProfileBuilder via DI");
            return Response = new ProfileRequestResponse<T> { ResultStatus = RequestErrorStatus.FailedOnRequestProfile};
          }

          Profiler.Configure(ProfileStyle, SiteModel, ProdDataExistenceMap, ProfileTypeRequired, Filters, 
            new DesignWrapper(Design, design),
            /* todo elevation range design + offset: */null, PopulationControl, 
            new CellPassFastEventLookerUpper(SiteModel), VolumeType, Overrides, LiftParams);

          Log.LogInformation("Building cell profile");
          if (await Profiler.CellProfileBuilder.Build(NEECoords, ProfileCells))
          {
            SetupForCellPassStackExamination(Filters.Filters[0].AttributeFilter);

            Log.LogInformation("Building lift profile");
            if (await Profiler.CellProfileAnalyzer.Analyze(ProfileCells, CellPassIterator))
            {
              Log.LogInformation("Lift profile building succeeded");

              // Remove null cells in the profiles list. NUll cells are defined by cells with null CellLastHeight.
              // All duplicate null cells will be replaced by a by single null cell entry
              var ThinnedProfileCells = ProfileCells.Where((x, i) =>
                  i == 0 || !ProfileCells[i].IsNull() || (ProfileCells[i].IsNull() && !ProfileCells[i - 1].IsNull())).ToList();

              Response = new ProfileRequestResponse<T>
              {
                GridDistanceBetweenProfilePoints = Profiler.CellProfileBuilder.GridDistanceBetweenProfilePoints,
                ProfileCells = ThinnedProfileCells,
                ResultStatus = RequestErrorStatus.OK
              };

              return Response;
            }

            Log.LogInformation("Lift profile building failed");
          }
        }
        finally
        {
          Log.LogInformation($"#Out# Execute: DataModel {ProjectID} complete for profile line. #Result#:{Response?.ResultStatus ?? RequestErrorStatus.Exception} with {Response?.ProfileCells?.Count ?? 0} vertices");
        }
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception:");
      }

      return new ProfileRequestResponse<T>();
    }
  }
}
