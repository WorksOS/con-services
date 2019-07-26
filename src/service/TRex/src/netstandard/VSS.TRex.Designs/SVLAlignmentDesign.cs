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

namespace VSS.TRex.Designs
{
  public class SVLAlignmentDesign : DesignBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SVLAlignmentDesign>();

    private double _cellSize;

    /// <summary>
    /// Represents the master guidance alignment selected from the NFFFile.
    /// </summary>
    private NFFGuidableAlignmentEntity Data;

    private BoundingWorldExtent3D BoundingBox = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// Constructs a guidance alignment design with a cell size used for computing filter patches
    /// </summary>
    /// <param name="cellSize"></param>
    public SVLAlignmentDesign(double cellSize)
    {
      _cellSize = cellSize;

      Data = new NFFGuidableAlignmentEntity();
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

    public override bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset, SubGridTreeBitmapSubGridBits Mask, SubGridTreeBitmapSubGridBits Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      double LeftOffsetValue = -LeftOffset;
      double RightOffsetValue = RightOffset;

      if (LeftOffsetValue > RightOffsetValue)
        MinMax.Swap(ref LeftOffsetValue, ref RightOffsetValue);

      //  {$IFDEF DEBUG}
      //   SIGLogMessage.PublishNoODS(Self, Format('Constructing filter patch for Stn:%.3f-%.3f, Ofs:%.3f-%.3f, OriginX:%.3f, OriginY:%.3f',
      //   [StartStn, EndStn, LeftOffsetValue, RightOffsetValue, OriginX, OriginY]), slmcDebug);
      //   {$ENDIF}

      if (Data == null)
      {
        Log.LogError("No Data element provided to SVL filter patch calculation");
        return false;
      }

      NFFStationedLineworkEntity Element;
      Patch.Clear();

      // Check the corners of the sub grid. If all are out of the offset range then assume
      // none of the cells are applicable. All four corners need to be on the same side of the
      // alignment in terms of offset to fail the sub grid.
      int CornersOutOfOffsetRange = 0;
      int CornersOutOfOffsetRangeSign = 0;

      double OriginXPlusHalfCellSize = OriginX + CellSize / 2;
      double OriginYPlusHalfCellSize = OriginY + CellSize / 2;

      for (int I = 0; I < Corners.Length; I++)
      {
        Data.ComputeStnOfs(OriginXPlusHalfCellSize + Corners[I].X * CellSize, OriginYPlusHalfCellSize + Corners[I].Y * CellSize, out double Stn, out double Ofs);

        if (!((Stn != Consts.NullDouble && Ofs != Consts.NullDouble) && Range.InRange(Stn, StartStn, EndStn)))
          if (!Range.InRange(Ofs, LeftOffsetValue, RightOffsetValue))
          {
            if (I == 0)
              CornersOutOfOffsetRangeSign = Math.Sign(Ofs);

            if (CornersOutOfOffsetRangeSign == Math.Sign(Ofs))
              CornersOutOfOffsetRange++;
            else
              break;
          }
      }

      if (CornersOutOfOffsetRange == Corners.Length)
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
      for (int I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
      {
        for (int J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
        {
          if (Mask.BitSet(I, J))
          {
            // Force element to be nil for all calculation until we resolve the issue
            // of an in appropriate element 'capturing' the focus and then being used to
            // calculate inappropriate offsets due to it's station range covering the
            // points being computed.

            Element = null;

            Data.ComputeStnOfs(OriginXPlusHalfCellSize + I * CellSize, OriginYPlusHalfCellSize + J * CellSize,
              out double Stn, out double Ofs, ref Element);

            if (Stn != Consts.NullDouble && Ofs != Consts.NullDouble)
              Patch.SetBitValue(I, J, Range.InRange(Stn, StartStn, EndStn) && Range.InRange(Ofs, LeftOffsetValue, RightOffsetValue));
          }
        }
      }

      //  {$IFDEF DEBUG}
      //  SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction successful with %d bits', [Patch.CountBits]), slmcDebug);
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
      x1 = BoundingBox.MinX;
      y1 = BoundingBox.MinY;
      x2 = BoundingBox.MaxX;
      y2 = BoundingBox.MaxY;
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
      DesignLoadResult Result;
      var NFFFile = SVL.NFFFile.CreateFromFile(fileName);

      try
      {
        Result = DesignLoadResult.NoAlignmentsFound;

        for (int I = 0; I < NFFFile.GuidanceAlignments.Count; I++)
        {
          if (NFFFile.GuidanceAlignments[I].IsMasterAlignment())
          {
            Data = NFFFile.GuidanceAlignments[I];

            NFFFile.GuidanceAlignments.RemoveAt(I);

            if (Data != null)
            {
              BoundingBox = Data.BoundingBox();
              Result = DesignLoadResult.Success;
            }

            break;
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception in {nameof(LoadFromFile)}");
        Result = DesignLoadResult.UnknownFailure;
      }

      return Result;
    }

    public override Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      throw new NotImplementedException();
    }
  }
}
