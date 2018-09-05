using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.Executors
{
  /// <summary>
  /// Calculate a surface patch for a subgrid by querying a set of supplied surveyed surfaces and extracting
  /// earliest, latest or composite elevation information from those surveyed surfaces
  /// </summary>
  public class CalculateSurfaceElevationPatch
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateSurfaceElevationPatch>();

    /// <summary>
    /// Local reference to the client subgrid factory
    /// </summary>
    private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.Factory();

    /// <summary>
    /// Private reference to the arguments provided to the executor
    /// </summary>
    private SurfaceElevationPatchArgument Args { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateSurfaceElevationPatch()
    {
    }

    /// <summary>
    /// Constructor for the executor accepting the arguments for its operation
    /// </summary>
    /// <param name="args"></param>
    public CalculateSurfaceElevationPatch(SurfaceElevationPatchArgument args) : this()
    {
      Args = args;
    }

    /// <summary>
    /// Performs the donkey work of the elevation patch calculation
    /// </summary>
    /// <param name="CalcResult"></param>
    /// <returns></returns>
    private IClientLeafSubGrid /*ClientHeightAndTimeLeafSubGrid */ Calc(out DesignProfilerRequestResult CalcResult)
    {
      CalcResult = DesignProfilerRequestResult.UnknownError;

      DesignBase Design;
      int Hint = -1;

      try
      {
        // if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
        //   SIGLogMessage.PublishNoODS(Self, Format('In %s.Execute for DataModel:%d  OTGCellBottomLeftX:%d  OTGCellBottomLeftY:%d', [Self.ClassName, Args.DataModelID, Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY]), slmcDebug);
        // InterlockedIncrement64(DesignProfilerRequestStats.NumSurfacePatchesComputed);

        try
        {
          IClientLeafSubGrid Patch = ClientLeafSubGridFactory.GetSubGrid(
            Args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
              ? GridDataType.CompositeHeights
              : GridDataType.HeightAndTime);

          if (Patch == null)
            return null;

          // Assign 
          ClientHeightAndTimeLeafSubGrid PatchSingle =
            Args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations
              ? Patch as ClientHeightAndTimeLeafSubGrid
              : null;

          ClientCompositeHeightsLeafSubgrid PatchComposite =
            Args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
              ? Patch as ClientCompositeHeightsLeafSubgrid
              : null;

          Patch.CellSize = Args.CellSize;
          Patch.SetAbsoluteOriginPosition(Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY);
          Patch.CalculateWorldOrigin(out double OriginX, out double OriginY);

          double CellSize = Args.CellSize;
          double HalfCellSize = CellSize / 2;
          double OriginXPlusHalfCellSize = OriginX + HalfCellSize;
          double OriginYPlusHalfCellSize = OriginY + HalfCellSize;

          // Work down through the list of surfaces in the time ordering provided by the caller
          for (int i = 0; i < Args.IncludedSurveyedSurfaces.Count; i++)
          {
            if (Args.ProcessingMap.IsEmpty())
              break;

            ISurveyedSurface ThisSurveyedSurface = Args.IncludedSurveyedSurfaces[i];

            // Lock & load the design
            Design = DesignFiles.Designs.Lock(ThisSurveyedSurface.Get_DesignDescriptor(), Args.SiteModelID, Args.CellSize, out _);

            if (Design == null)
            {
              Log.LogError($"Failed to read design file {ThisSurveyedSurface.Get_DesignDescriptor()} in {nameof(CalculateSurfaceElevationPatch)}");
              CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
              return null;
            }

            try
            {
              Design.AcquireExclusiveInterlock();
              try
              {
                if (!Design.HasElevationDataForSubGridPatch(
                  Args.OTGCellBottomLeftX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                  Args.OTGCellBottomLeftY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
                  continue;

                long AsAtDate = ThisSurveyedSurface.AsAtDate.ToBinary();
                double Offset = ThisSurveyedSurface.Get_DesignDescriptor().Offset;

                // Walk across the subgrid checking for a design elevation for each appropriate cell
                // based on the processing bit mask passed in
                Args.ProcessingMap.ForEachSetBit((x, y) =>
                {
                  // If we can interpolate a height for the requested cell, then update the cell height
                  // and decrement the bit count so that we know when we've handled all the requested cells

                  if (Design.InterpolateHeight(ref Hint,
                    OriginXPlusHalfCellSize + CellSize * x, OriginYPlusHalfCellSize + CellSize * y,
                    Offset, out double z))
                  {
                    switch (Args.SurveyedSurfacePatchType)
                    {
                      // Check for compositie elevation processing
                      case SurveyedSurfacePatchType.CompositeElevations:
                      {
                        // Set the first elevation if not already set
                        if (PatchComposite.Cells[x, y].FirstHeightTime == 0)
                        {
                          PatchComposite.Cells[x, y].FirstHeightTime = AsAtDate;
                          PatchComposite.Cells[x, y].FirstHeight = (float) z;
                        }

                        // Always set the latest elevation (surfaces ordered by increasing date)
                        PatchComposite.Cells[x, y].LastHeightTime = AsAtDate;
                        PatchComposite.Cells[x, y].LastHeight = (float) z;

                        // Update the lowest height
                        if (PatchComposite.Cells[x, y].LowestHeightTime == 0 ||
                            PatchComposite.Cells[x, y].LowestHeight > z)
                        {
                          PatchComposite.Cells[x, y].LowestHeightTime = AsAtDate;
                          PatchComposite.Cells[x, y].LowestHeight = (float) z;
                        }

                        // Update the highest height
                        if (PatchComposite.Cells[x, y].HighestHeightTime == 0 ||
                            PatchComposite.Cells[x, y].HighestHeight > z)
                        {
                          PatchComposite.Cells[x, y].HighestHeightTime = AsAtDate;
                          PatchComposite.Cells[x, y].HighestHeight = (float) z;
                        }

                        break;
                      }

                      // checked for earliest/latest singular value processing
                      case SurveyedSurfacePatchType.LatestSingleElevation:
                      case SurveyedSurfacePatchType.EarliestSingleElevation:
                      {

                        PatchSingle.Cells[x, y] = (float) z;
                        PatchSingle.Times[x, y] = AsAtDate;
                        break;
                      }

                      default:
                        Debug.Assert(false, $"Unknown SurveyedSurfacePatchType: {Args.SurveyedSurfacePatchType}");
                        break;
                    }

                    // Only clear the processing bit if earliest or latest information is wanted from the surveyed surfaces
                    if (Args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations)
                      Args.ProcessingMap.ClearBit(x, y);
                  }

                  return true;
                });
              }
              finally
              {
                Design.ReleaseExclusiveInterlock();
              }
            }
            finally
            {
              DesignFiles.Designs.UnLock(ThisSurveyedSurface.Get_DesignDescriptor(), Design);
            }
          }

          CalcResult = DesignProfilerRequestResult.OK;

          return Patch;
        }
        finally
        {
          //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
          //Log.LogInformation($"Out {nameof(CalculateSurfaceElevationPatch)}.Execute");
        }
      }
      catch (Exception E)
      {
        Log.LogError($"Exception: {E}");
      }

      return null;
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public IClientLeafSubGrid /*ClientHeightAndTimeLeafSubGrid*/ Execute()
    {
      try
      {
        // Perform the design profile calculation
        try
        {
          // Calculate the patch of elevations and return it
          IClientLeafSubGrid /*ClientHeightAndTimeLeafSubGrid*/
            result = Calc(out DesignProfilerRequestResult CalcResult);

          if (result == null)
          {
            // TODO: Handle case of failure to request patch of elevations from design
          }

          return result;
        }
        finally
        {
          //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
          // Log.LogInformation($"#Out# {nameof(CalculateSurfaceElevationPatch)}.Execute #Result# {CalcResult}");
        }
      }
      catch (Exception E)
      {
        Log.LogError($"{nameof(CalculateSurfaceElevationPatch)}.Execute: Exception {E}");
        return null;
      }
    }
  }
}
