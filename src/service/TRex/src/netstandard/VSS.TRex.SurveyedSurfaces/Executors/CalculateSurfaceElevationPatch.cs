using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.Executors
{
  /// <summary>
  /// Calculate a surface patch for a sub grid by querying a set of supplied surveyed surfaces and extracting
  /// earliest, latest or composite elevation information from those surveyed surfaces
  /// </summary>
  public class CalculateSurfaceElevationPatch
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateSurfaceElevationPatch>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private readonly IClientLeafSubGridFactory _clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// Private reference to the arguments provided to the executor
    /// </summary>
    private readonly ISurfaceElevationPatchArgument _args;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateSurfaceElevationPatch()
    {
    }

    /// <summary>
    /// Constructor for the executor accepting the arguments for its operation
    /// </summary>
    public CalculateSurfaceElevationPatch(ISurfaceElevationPatchArgument args) : this()
    {
      _args = args;
    }

    /// <summary>
    /// Performs the donkey work of the elevation patch calculation
    /// </summary>
    private IClientLeafSubGrid Calc(out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      if (!Enum.IsDefined(typeof(SurveyedSurfacePatchType), _args.SurveyedSurfacePatchType))
      {
        _log.LogError($"Unknown SurveyedSurfacePatchType: {_args.SurveyedSurfacePatchType}, returning null");
        return null;
      }

      if (_args.IncludedSurveyedSurfaces == null)
      {
        _log.LogError($"Included surveyed surfaces list is null, returning null");
        return null;
      }

      var patch = _clientLeafSubGridFactory.GetSubGridEx(
        _args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
          ? GridDataType.CompositeHeights
          : GridDataType.HeightAndTime,
          _args.CellSize, SubGridTreeConsts.SubGridTreeLevels,
          _args.OTGCellBottomLeftX, _args.OTGCellBottomLeftY);

      // Assign 
      var patchSingle = _args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations
        ? patch as ClientHeightAndTimeLeafSubGrid : null;

      var patchComposite = _args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
        ? patch as ClientCompositeHeightsLeafSubgrid : null;

      patch.CalculateWorldOrigin(out var originX, out var originY);

      var cellSize = _args.CellSize;
      var halfCellSize = cellSize / 2;
      var originXPlusHalfCellSize = originX + halfCellSize;
      var originYPlusHalfCellSize = originY + halfCellSize;

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(_args.SiteModelID);
      var designs = DIContext.Obtain<IDesignFiles>();

      // Work down through the list of surfaces in the time ordering provided by the caller
      for (var i = 0; i < _args.IncludedSurveyedSurfaces.Length; i++)
      {
        if (_args.ProcessingMap.IsEmpty())
          break;

        var thisSurveyedSurface = siteModel.SurveyedSurfaces.Locate(_args.IncludedSurveyedSurfaces[i]);
        if (thisSurveyedSurface == null)
        {
          _log.LogError($"Surveyed surface {_args.IncludedSurveyedSurfaces[i]} not found in site model, returning null");
          calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
          return null;
        }

        // Lock & load the design
        var design = designs.Lock(thisSurveyedSurface.DesignDescriptor.DesignID, _args.SiteModelID, _args.CellSize, out _);

        if (design == null)
        {
          _log.LogError($"Failed to lock design file {thisSurveyedSurface.DesignDescriptor} in {nameof(CalculateSurfaceElevationPatch)}");
          calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
          return null;
        }

        try
        {
          if (!design.HasElevationDataForSubGridPatch(
            _args.OTGCellBottomLeftX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
            _args.OTGCellBottomLeftY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
          {
            continue;
          }

          var asAtDate = thisSurveyedSurface.AsAtDate.Ticks;
          var hint = -1;

          // Walk across the sub grid checking for a design elevation for each appropriate cell
          // based on the processing bit mask passed in
          _args.ProcessingMap.ForEachSetBit((x, y) =>
          {
            // If we can interpolate a height for the requested cell, then update the cell height
            // and decrement the bit count so that we know when we've handled all the requested cells

            if (design.InterpolateHeight(ref hint,
                originXPlusHalfCellSize + cellSize * x, originYPlusHalfCellSize + cellSize * y,
                0, out var z))
            {
              // Check for composite elevation processing
              if (_args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations)
              {
                // Set the first elevation if not already set
                if (patchComposite.Cells[x, y].FirstHeightTime == 0)
                {
                  patchComposite.Cells[x, y].FirstHeightTime = asAtDate;
                  patchComposite.Cells[x, y].FirstHeight = (float) z;
                }

                // Always set the latest elevation (surfaces ordered by increasing date)
                patchComposite.Cells[x, y].LastHeightTime = asAtDate;
                patchComposite.Cells[x, y].LastHeight = (float) z;

                // Update the lowest height
                if (patchComposite.Cells[x, y].LowestHeightTime == 0 ||
                    patchComposite.Cells[x, y].LowestHeight > z)
                {
                  patchComposite.Cells[x, y].LowestHeightTime = asAtDate;
                  patchComposite.Cells[x, y].LowestHeight = (float) z;
                }

                // Update the highest height
                if (patchComposite.Cells[x, y].HighestHeightTime == 0 ||
                    patchComposite.Cells[x, y].HighestHeight > z)
                {
                  patchComposite.Cells[x, y].HighestHeightTime = asAtDate;
                  patchComposite.Cells[x, y].HighestHeight = (float) z;
                }
              }
              else // earliest/latest singular value processing
              {
                patchSingle.Times[x, y] = asAtDate;
                patchSingle.Cells[x, y] = (float) z;
              }
            }

            // Only clear the processing bit if earliest or latest information is wanted from the surveyed surfaces
            if (_args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations)
              _args.ProcessingMap.ClearBit(x, y);

            return true;
          });
        }
        finally
        {
          designs.UnLock(thisSurveyedSurface.DesignDescriptor.DesignID, design);
        }
      }

      calcResult = DesignProfilerRequestResult.OK;

      return patch;
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public IClientLeafSubGrid Execute()
    {
      // Perform the design profile calculation
      try
      {
        // Calculate the patch of elevations and return it
        var result = Calc(out var calcResult);

        // TODO: Handle case of failure to request patch of elevations from design

        return result;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred calculating surveyed surface patch");
        return null;
      }
    }
  }
}
