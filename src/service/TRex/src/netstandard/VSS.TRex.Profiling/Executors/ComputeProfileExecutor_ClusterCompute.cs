﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Types;

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

    private const int INITIAL_PROFILE_LIST_SIZE = 1000;

    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    private readonly Guid DesignUid;
    private bool ReturnAllPassesAndLayers;

    private ISubGridSegmentCellPassIterator CellPassIterator;
    private ISubGridSegmentIterator SegmentIterator;

    /// <summary>
    /// Constructs the profile analysis executor
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="profileTypeRequired"></param>
    /// <param name="nEECoords"></param>
    /// <param name="filters"></param>
    /// <param name="designUid"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ComputeProfileExecutor_ClusterCompute(Guid projectID, GridDataType profileTypeRequired, XYZ[] nEECoords, IFilterSet filters,
      // todo liftBuildSettings: TICLiftBuildSettings;
      // externalRequestDescriptor: TASNodeRequestDescriptor;
      Guid designUid, bool returnAllPassesAndLayers)
    {
      ProjectID = projectID;
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      Filters = filters;
      DesignUid = designUid;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }

    /// <summary>
    /// Create and configure the segment iterator to be used
    /// </summary>
    /// <param name="passFilter"></param>
    private void SetupForCellPassStackExamination(ICellPassAttributeFilter passFilter)
    {      
      SegmentIterator = new SubGridSegmentIterator(null, null, DIContext.Obtain<ISiteModels>().StorageProxy);

      if (passFilter.ReturnEarliestFilteredCellPass ||
          (passFilter.HasElevationTypeFilter && passFilter.ElevationType == ElevationType.First))
        SegmentIterator.IterationDirection = IterationDirection.Forwards;
      else
        SegmentIterator.IterationDirection = IterationDirection.Backwards;

      if (passFilter.HasMachineFilter)
        SegmentIterator.SetMachineRestriction(passFilter.MachineIDSet);

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
    public ProfileRequestResponse<T> Execute()
    {
      // todo Args.LiftBuildSettings.CCVSummaryTypes := Args.LiftBuildSettings.CCVSummaryTypes + [iccstCompaction];
      // todo Args.LiftBuildSettings.MDPSummaryTypes := Args.LiftBuildSettings.MDPSummaryTypes + [icmdpCompaction];

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
            Log.LogInformation($"#In#: DataModel {ProjectID}, Note! vertices list has insufficient vertices (min of 2 required)");

          ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(ProjectID);

          if (SiteModel == null)
          {
            Log.LogWarning($"Failed to locate sitemodel {ProjectID}");
            return Response = new ProfileRequestResponse<T> {ResultStatus = RequestErrorStatus.NoSuchDataModel};
          }

          // Obtain the subgrid existence map for the project
          ISubGridTreeBitMask ProdDataExistenceMap = SiteModel.ExistenceMap;

          if (ProdDataExistenceMap == null)
          {
            Log.LogWarning($"Failed to locate production data existence map from sitemodel {ProjectID}");
            return Response = new ProfileRequestResponse<T> {ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap};
          }

          ICellSpatialFilter CellFilter = Filters.Filters[0].SpatialFilter;
          ICellPassAttributeFilter PassFilter = Filters.Filters[0].AttributeFilter;

          FilteredValuePopulationControl PopulationControl = new FilteredValuePopulationControl();
          PopulationControl.PreparePopulationControl(ProfileTypeRequired, PassFilter);

          IDesign design = null;
          if (DesignUid != Guid.Empty)
          {
            design = SiteModel.Designs.Locate(DesignUid);

            if (design == null)
              throw new ArgumentException($"Design {DesignUid} is unknown in project {SiteModel.ID}");
          }

          Log.LogInformation("Creating IProfileBuilder");

          IProfilerBuilder<T> Profiler = DIContext.Obtain<IProfilerBuilder<T>>();

          Profiler.Configure(SiteModel, ProdDataExistenceMap, ProfileTypeRequired, PassFilter, CellFilter, design,
            /* todo elevation range design: */null,
            PopulationControl, new CellPassFastEventLookerUpper(SiteModel));

          Log.LogInformation("Building cell profile");
          if (Profiler.CellProfileBuilder.Build(NEECoords, ProfileCells))
          {
            SetupForCellPassStackExamination(PassFilter);

            Log.LogInformation("Building lift profile");
            if (Profiler.CellProfileAnalyzer.Analyze(ProfileCells, CellPassIterator))
            {
              Log.LogInformation("Lift profile building succeeded");

              // Remove null cells in the profiles list. NUll cells are defined by cells with null CellLastHeight.
              // All duplicate null cells will be replaced by a by single null cell entry
              List<T> ThinnedProfileCells = ProfileCells.Where((x, i) =>
                  i == 0 || !ProfileCells[i].IsNull() || (ProfileCells[i].IsNull() && !ProfileCells[i - 1].IsNull())).ToList();

              Response = new ProfileRequestResponse<T>
              {
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
        Log.LogError($"Execute: Exception {E}");
      }

      return new ProfileRequestResponse<T>();
    }
  }
}
