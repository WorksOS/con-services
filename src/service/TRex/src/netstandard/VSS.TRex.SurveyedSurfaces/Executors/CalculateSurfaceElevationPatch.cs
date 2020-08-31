using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
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
    /// Performs the donkey work of the elevation patch calculation
    /// </summary>
    public IClientLeafSubGrid Execute(ISiteModel siteModel, int otgCellBottomLeftX, int otgCellBottomLeftY, double cellSize, SurveyedSurfacePatchType patchType,
      Guid[] includedSurveyedSurfaces, IDesignFiles designs, ISurveyedSurfaces surveyedSurfaces, SubGridTreeBitmapSubGridBits processingMap)
    {
      var calcResult = DesignProfilerRequestResult.UnknownError;
      
      try
      {
        if (!Enum.IsDefined(typeof(SurveyedSurfacePatchType), patchType))
        {
          _log.LogError($"Unknown SurveyedSurfacePatchType: {patchType}, returning null");
          return null;
        }

        if (includedSurveyedSurfaces == null)
        {
          _log.LogError("Included surveyed surfaces list is null, returning null");
          return null;
        }

        var patch = _clientLeafSubGridFactory.GetSubGridEx(
          patchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime,
          cellSize, SubGridTreeConsts.SubGridTreeLevels,
          otgCellBottomLeftX, otgCellBottomLeftY);

        // Assign 
        var patchSingle = patchType != SurveyedSurfacePatchType.CompositeElevations
          ? patch as ClientHeightAndTimeLeafSubGrid
          : null;

        var patchComposite = patchType == SurveyedSurfacePatchType.CompositeElevations
          ? patch as ClientCompositeHeightsLeafSubgrid
          : null;

        patch.CalculateWorldOrigin(out var originX, out var originY);

        var halfCellSize = cellSize / 2;
        var originXPlusHalfCellSize = originX + halfCellSize;
        var originYPlusHalfCellSize = originY + halfCellSize;

        // Work down through the list of surfaces in the time ordering provided by the caller
        foreach (var surveyedSurfaceUid in includedSurveyedSurfaces)
        {
          if (processingMap.IsEmpty())
            break;

          var thisSurveyedSurface = surveyedSurfaces.Locate(surveyedSurfaceUid);
          if (thisSurveyedSurface == null)
          {
            _log.LogError($"Surveyed surface {surveyedSurfaceUid} not found in site model, returning null");
            calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
            return null;
          }

          // Lock & load the design
          var design = designs.Lock(thisSurveyedSurface.DesignDescriptor.DesignID, siteModel, cellSize, out _);

          if (design == null)
          {
            _log.LogError($"Failed to lock design file {thisSurveyedSurface.DesignDescriptor} in {nameof(CalculateSurfaceElevationPatch)}");
            calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
            return null;
          }

          try
          {
            if (!design.HasElevationDataForSubGridPatch(
              otgCellBottomLeftX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
              otgCellBottomLeftY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
            {
              continue;
            }

            var asAtDate = thisSurveyedSurface.AsAtDate.Ticks;
            var hint = -1;

            // Walk across the sub grid checking for a design elevation for each appropriate cell
            // based on the processing bit mask passed in
            processingMap.ForEachSetBit((x, y) =>
            {
              // If we can interpolate a height for the requested cell, then update the cell height
              // and decrement the bit count so that we know when we've handled all the requested cells

              if (design.InterpolateHeight(ref hint,
                originXPlusHalfCellSize + cellSize * x, originYPlusHalfCellSize + cellSize * y,
                0, out var z))
              {
                // Check for composite elevation processing
                if (patchType == SurveyedSurfacePatchType.CompositeElevations)
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
              if (patchType != SurveyedSurfacePatchType.CompositeElevations)
                processingMap.ClearBit(x, y);

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
      catch (Exception e)
      {
        _log.LogError(e, $"Exception occurred calculating surveyed surface patch, calcResult = {calcResult}");
        return null;
      }
    }
  }
}
