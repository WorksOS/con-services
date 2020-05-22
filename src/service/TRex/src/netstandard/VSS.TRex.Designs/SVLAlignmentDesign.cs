using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.Designs
{
  public class SVLAlignmentDesign : DesignBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SVLAlignmentDesign>();

    private double _cellSize;

    /// <summary>
    /// Represents the master guidance alignment selected from the NFFFile.
    /// </summary>
    private NFFGuidableAlignmentEntity data;

    private BoundingWorldExtent3D boundingBox = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// Constructs a guidance alignment design with a cell size used for computing filter patches
    /// </summary>
    /// <param name="cellSize"></param>
    public SVLAlignmentDesign(double cellSize)
    {
      _cellSize = cellSize;

      data = new NFFGuidableAlignmentEntity();
    }

    private struct Corner
    {
      public int X, Y;

      public Corner(int x, int y)
      {
        X = x;
        Y = y;
      }
    }

    private static Corner[] Corners =
    {
      new Corner(0, 0),
      new Corner(SubGridTreeConsts.SubGridTreeDimension - 1, 0),
      new Corner(0, SubGridTreeConsts.SubGridTreeDimension - 1),
      new Corner(SubGridTreeConsts.SubGridTreeDimension - 1, SubGridTreeConsts.SubGridTreeDimension - 1)
    };

    public override bool ComputeFilterPatch(double startStn, double endStn, double leftOffset, double rightOffset, 
      SubGridTreeBitmapSubGridBits mask, SubGridTreeBitmapSubGridBits patch, 
      double originX, double originY, double cellSize, double offset)
    {
      double leftOffsetValue = -leftOffset;
      double rightOffsetValue = rightOffset;

      if (leftOffsetValue > rightOffsetValue)
        MinMax.Swap(ref leftOffsetValue, ref rightOffsetValue);

      //  {$IFDEF DEBUG}
      //   SIGLogMessage.PublishNoODS(Self, Format('Constructing filter patch for Stn:%.3f-%.3f, Ofs:%.3f-%.3f, originX:%.3f, originY:%.3f',
      //   [startStn, endStn, LeftOffsetValue, RightOffsetValue, originX, originY]), slmcDebug);
      //   {$ENDIF}

      if (data == null)
      {
        Log.LogError("No data element provided to SVL filter patch calculation");
        return false;
      }

      patch.Clear();

      // Check the corners of the sub grid. If all are out of the offset range then assume
      // none of the cells are applicable. All four corners need to be on the same side of the
      // alignment in terms of offset to fail the sub grid.
      int cornersOutOfOffsetRange = 0;
      int cornersOutOfOffsetRangeSign = 0;

      double originXPlusHalfCellSize = originX + cellSize / 2;
      double originYPlusHalfCellSize = originY + cellSize / 2;

      for (int i = 0; i < Corners.Length; i++)
      {
        data.ComputeStnOfs(originXPlusHalfCellSize + Corners[i].X * cellSize, originYPlusHalfCellSize + Corners[i].Y * cellSize, out double stn, out double ofs);

        if (!(stn != Consts.NullDouble && ofs != Consts.NullDouble && Range.InRange(stn, startStn, endStn)))
        {
          if (!Range.InRange(ofs, leftOffsetValue, rightOffsetValue))
          {
            if (i == 0)
              cornersOutOfOffsetRangeSign = Math.Sign(ofs);

            if (cornersOutOfOffsetRangeSign == Math.Sign(ofs))
              cornersOutOfOffsetRange++;
            else
              break;
          }
        }
      }

      if (cornersOutOfOffsetRange == Corners.Length)
      {
        // Return success with the empty patch
        //{$IFDEF DEBUG}
        //SIGLogMessage.PublishNoODS(Self, 'All corners of patch exceed stn:ofs boundary', slmcDebug);
        //{$ENDIF}
        return true;
      }

      // Iterate across the cells in the mask computing and checking the stn:ofs of
      // each point using the previously successful element as a hint for the next
      // computation
      for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (mask.BitSet(i, j))
          {
            // Force element to be nil for all calculation until we resolve the issue
            // of an in appropriate element 'capturing' the focus and then being used to
            // calculate inappropriate offsets due to it's station range covering the
            // points being computed.

            NFFStationedLineworkEntity element = null;

            data.ComputeStnOfs(originXPlusHalfCellSize + i * cellSize, originYPlusHalfCellSize + j * cellSize,
              out double stn, out double ofs, ref element);

            if (stn != Consts.NullDouble && ofs != Consts.NullDouble)
              patch.SetBitValue(i, j, Range.InRange(stn, startStn, endStn) && Range.InRange(ofs, leftOffsetValue, rightOffsetValue));
          }
        }
      }

      //  {$IFDEF DEBUG}
      //  SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction successful with %d bits', [patch.CountBits]), slmcDebug);
      //  {$ENDIF}

      return true;
    }

    public override List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      return new List<XYZS>();
    }

    public override List<Fence> GetBoundary()
    {
      throw new NotImplementedException();
    }

    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      x1 = boundingBox.MinX;
      y1 = boundingBox.MinY;
      x2 = boundingBox.MaxX;
      y2 = boundingBox.MaxY;
    }

    public override void GetHeightRange(out double z1, out double z2)
    {
      z1 = Consts.NullDouble;
      z2 = Consts.NullDouble;
    }

    public override bool HasElevationDataForSubGridPatch(double X, double Y)
    {
      return false;
    }

    public override bool HasElevationDataForSubGridPatch(int SubGridX, int SubGridY)
    {
      return false;
    }

    public override bool HasFiltrationDataForSubGridPatch(double X, double Y)
    {
      return false;
    }

    public override bool HasFiltrationDataForSubGridPatch(int SubGridX, int SubGridY)
    {
      return false;
    }

    public override bool InterpolateHeight(ref int Hint, double X, double Y, double Offset, out double Z)
    {
      Z = Consts.NullDouble;
      return false;
    }

    public override bool InterpolateHeights(float[,] Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      return false;
    }

    public override DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true)
    {
      DesignLoadResult result;
      var NFFFile = SVL.NFFFile.CreateFromFile(fileName);

      try
      {
        result = DesignLoadResult.NoAlignmentsFound;

        for (int i = 0; i < NFFFile.GuidanceAlignments.Count; i++)
        {
          if (NFFFile.GuidanceAlignments[i].IsMasterAlignment())
          {
            data = NFFFile.GuidanceAlignments[i];

            NFFFile.GuidanceAlignments.RemoveAt(i);

            if (data != null)
            {
              boundingBox = data.BoundingBox();
              result = DesignLoadResult.Success;
            }

            break;
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception in {nameof(LoadFromFile)}");
        result = DesignLoadResult.UnknownFailure;
      }

      return result;
    }

    /// <summary>
    /// Loads the SVL design file/s, from storage
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <param name="localPath"></param>
    /// <param name="loadIndices"></param>
    /// <returns></returns>
    public override async Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      var isDownloaded = await S3FileTransfer.ReadFile(siteModelUid, fileName, localPath);
      if (!isDownloaded)
        return DesignLoadResult.UnknownFailure;

      return DesignLoadResult.Success;
    }

    public (double StartStation, double EndStation) GetStationRange()
    {
      return (data.StartStation, data.EndStation);
    }

    public DesignProfilerRequestResult DetermineFilterBoundary(double startStation, double endStation, double leftOffset, double rightOffset, out Fence fence)
    {
      var determinator = new SVLAlignmentBoundaryDeterminator(data, startStation, endStation, leftOffset, rightOffset);

      determinator.DetermineBoundary(out var calcResult, out fence);

      return calcResult;
    }

    public override bool RemoveFromStorage(Guid siteModelUid, string fileName)
    {
      return S3FileTransfer.RemoveFileFromBucket(siteModelUid, fileName);
    }

    /// <summary>
    /// Obtains the master alignment
    /// </summary>
    /// <returns></returns>
    public NFFGuidableAlignmentEntity GetMasterAlignment() => data;
  }
}
