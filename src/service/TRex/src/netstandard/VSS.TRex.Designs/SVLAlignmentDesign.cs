using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SVLAlignmentDesign>();

    /// <summary>
    /// Represents the master guidance alignment selected from the NFFFile.
    /// </summary>
    private NFFGuidableAlignmentEntity _data;

    private BoundingWorldExtent3D _boundingBox = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// Constructs a guidance alignment design used for computing filter patches
    /// </summary>
    public SVLAlignmentDesign()
    {
      _data = new NFFGuidableAlignmentEntity();
    }

    /// <summary>
    /// Constructs a guidance alignment design for computing filter patches
    /// </summary>
    public SVLAlignmentDesign(NFFGuidableAlignmentEntity data)
    {
      _data = data;
    }

    private readonly struct Corner
    {
      public readonly int X;
      public readonly int Y;

      public Corner(int x, int y)
      {
        X = x;
        Y = y;
      }
    }

    private static readonly Corner[] _corners =
    {
      new Corner(0, 0),
      new Corner(SubGridTreeConsts.SubGridTreeDimension - 1, 0),
      new Corner(0, SubGridTreeConsts.SubGridTreeDimension - 1),
      new Corner(SubGridTreeConsts.SubGridTreeDimension - 1, SubGridTreeConsts.SubGridTreeDimension - 1)
    };

    /// <summary>
    /// Computes a filter patch for a sub grid with respect to the alignment and a station/offset range over the alignment.
    /// Note: This is a CPU intensive operation. TRex currently uses an approach of polygonal spatial filtering with a boundary
    /// computed from the alignment geometry and station/offset bounds.
    /// </summary>
    public override bool ComputeFilterPatch(double startStn, double endStn, double leftOffset, double rightOffset,
      SubGridTreeBitmapSubGridBits mask, SubGridTreeBitmapSubGridBits patch,
      double originX, double originY, double cellSize, double offset)
    {
      var leftOffsetValue = -leftOffset;
      var rightOffsetValue = rightOffset;

      if (leftOffsetValue > rightOffsetValue)
        MinMax.Swap(ref leftOffsetValue, ref rightOffsetValue);

      //   SIGLogMessage.PublishNoODS(Self, Format('Constructing filter patch for Stn:%.3f-%.3f, Ofs:%.3f-%.3f, originX:%.3f, originY:%.3f',
      //   [startStn, endStn, LeftOffsetValue, RightOffsetValue, originX, originY]));

      if (_data == null)
      {
        _log.LogError("No data element provided to SVL filter patch calculation");
        return false;
      }

      patch.Clear();

      // Check the corners of the sub grid. If all are out of the offset range then assume
      // none of the cells are applicable. All four corners need to be on the same side of the
      // alignment in terms of offset to fail the sub grid.
      var cornersOutOfOffsetRange = 0;
      var cornersOutOfOffsetRangeSign = 0;

      var originXPlusHalfCellSize = originX + cellSize / 2;
      var originYPlusHalfCellSize = originY + cellSize / 2;

      for (var i = 0; i < _corners.Length; i++)
      {
        _data.ComputeStnOfs(originXPlusHalfCellSize + _corners[i].X * cellSize, originYPlusHalfCellSize + _corners[i].Y * cellSize, out var stn, out var ofs);

        if (!(stn != Consts.NullDouble && ofs != Consts.NullDouble && Range.InRange(stn, startStn, endStn)) &&
            !Range.InRange(ofs, leftOffsetValue, rightOffsetValue))
        {
          if (i == 0)
            cornersOutOfOffsetRangeSign = Math.Sign(ofs);

          if (cornersOutOfOffsetRangeSign == Math.Sign(ofs))
            cornersOutOfOffsetRange++;
          else
            break;
        }
      }

      if (cornersOutOfOffsetRange == _corners.Length)
      {
        // Return success with the empty patch
        //SIGLogMessage.PublishNoODS(Self, 'All corners of patch exceed stn:ofs boundary');
        return true;
      }

      // Iterate across the cells in the mask computing and checking the stn:ofs of
      // each point using the previously successful element as a hint for the next
      // computation
      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (!mask.BitSet(i, j)) 
            continue;

          // Force element to be nil for all calculation until we resolve the issue
          // of an in appropriate element 'capturing' the focus and then being used to
          // calculate inappropriate offsets due to it's station range covering the
          // points being computed.

          NFFStationedLineworkEntity element = null;

          _data.ComputeStnOfs(originXPlusHalfCellSize + i * cellSize, originYPlusHalfCellSize + j * cellSize,
            out var stn, out var ofs, ref element);

          if (stn != Consts.NullDouble && ofs != Consts.NullDouble)
            patch.SetBitValue(i, j, Range.InRange(stn, startStn, endStn) && Range.InRange(ofs, leftOffsetValue, rightOffsetValue));
        }
      }

      //  SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction successful with %d bits', [patch.CountBits]));

      return true;
    }

    public override List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      return new List<XYZS>();
    }

    public override List<Fence> GetBoundary()
    {
      return null; // There is no boundary defined for an alignment
    }

    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      x1 = _boundingBox.MinX;
      y1 = _boundingBox.MinY;
      x2 = _boundingBox.MaxX;
      y2 = _boundingBox.MaxY;
    }

    public override BoundingWorldExtent3D GetExtents() => new BoundingWorldExtent3D(_boundingBox);

    public override void GetHeightRange(out double z1, out double z2)
    {
      z1 = Consts.NullDouble;
      z2 = Consts.NullDouble;
    }

    public override bool HasElevationDataForSubGridPatch(double x, double y)
    {
      return false; // Alignments do not supply elevation patches
    }

    public override bool HasElevationDataForSubGridPatch(int subGridX, int subGridY)
    {
      return false; // Alignments do not supply elevation patches
    }

    public override bool HasFiltrationDataForSubGridPatch(double x, double y)
    {
      return false; // Alignments do not supply filter patches
    }

    public override bool HasFiltrationDataForSubGridPatch(int subGridX, int subGridY)
    {
      return false; // Alignments do not supply filter patches
    }

    public override bool InterpolateHeight(ref int hint, double x, double y, double offset, out double z)
    {
      z = Consts.NullDouble;
      return false; // Alignments do not supply elevations
    }

    public override bool InterpolateHeights(float[,] patch, double originX, double originY, double cellSize, double offset)
    {
      return false; // Alignments do not supply elevations
    }

    /// <summary>
    /// Loads the content of an SVL file into a memory model for use
    /// </summary>
    public override DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true)
    {
      DesignLoadResult result;
      var nffFile = NFFFile.CreateFromFile(fileName);

      try
      {
        result = nffFile.GuidanceAlignments.Count > 0 ? DesignLoadResult.NoAlignmentsFound : DesignLoadResult.NoMasterAlignmentsFound;

        for (var i = 0; i < nffFile.GuidanceAlignments.Count; i++)
        {
          if (nffFile.GuidanceAlignments[i].IsMasterAlignment())
          {
            _data = nffFile.GuidanceAlignments[i];

            nffFile.GuidanceAlignments.RemoveAt(i);

            if (_data != null)
            {
              _boundingBox = _data.BoundingBox();
              result = DesignLoadResult.Success;
            }

            break;
          }
        }

        if (result == DesignLoadResult.NoAlignmentsFound)
        {
          _log.LogDebug($"SVL file {fileName} contains no guidance alignments");
        }

        if (result == DesignLoadResult.NoMasterAlignmentsFound)
        {
          _log.LogDebug($"SVL file {fileName} contains {nffFile.GuidanceAlignments.Count} alignments, none of which are master alignments");
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception in {nameof(LoadFromFile)}");
        result = DesignLoadResult.UnknownFailure;
      }

      return result;
    }

    /// <summary>
    /// Loads the SVL design file/s, from storage
    /// </summary>
    public override async Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
      var isDownloaded = await s3FileTransfer.ReadFile(siteModelUid, fileName, localPath);
      return !isDownloaded ? DesignLoadResult.UnknownFailure : DesignLoadResult.Success;
    }

    public (double StartStation, double EndStation) GetStationRange()
    {
      return (_data.StartStation, _data.EndStation);
    }

    public DesignProfilerRequestResult DetermineFilterBoundary(double startStation, double endStation, double leftOffset, double rightOffset, out Fence fence)
    {
      // ReSharper disable once IdentifierTypo
      var determinator = new SVLAlignmentBoundaryDeterminator(_data, startStation, endStation, leftOffset, rightOffset);

      determinator.DetermineBoundary(out var calcResult, out fence);

      return calcResult;
    }

    public override bool RemoveFromStorage(Guid siteModelUid, string fileName)
    {
      var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
      return s3FileTransfer.RemoveFileFromBucket(siteModelUid, fileName);
    }

    /// <summary>
    /// Return an approximation of the memory required to store the master guidance alignment based on 256 bytes per elemnts in the geometry entities
    /// </summary>
    public override long SizeInCache()
    {
      return _data?.Entities.Count * 256 ?? 0;
    }

    /// <summary>
    /// Obtains the master alignment
    /// </summary>
    public NFFGuidableAlignmentEntity GetMasterAlignment() => _data;

    /// <summary>
    /// Returns a list of coordinates traced over the alignment within the station range specified.
    ///  This coordinate list is then typically used to produce a station offset report
    /// </summary>
    public List<StationOffsetPoint> GetOffsetPointsInNEE(double crossSectionInterval, double startStation, double endStation, double[] offsets, out DesignProfilerRequestResult calcResult)
    {
      var result = new List<StationOffsetPoint>(1000);

      double lastX = -1, lastY = -1;

      var subsetStartStation = Math.Max(_data.StartStation, startStation);
      var subsetEndStation = Math.Min(_data.EndStation, endStation);
      var currentStation = subsetStartStation;

      void AddCoord(double x, double y, double station, double offset)
      {
        if (lastX == x && lastY == y)
          return; // don't wont duplicates

        result.Add(new StationOffsetPoint(station, offset, x, y));
        lastX = x;
        lastY = y;
      }

      void AddPointAtStation(double x, double y, double station, double offset, bool rangeTestRequired)
      {
        if (rangeTestRequired) // test for end of range
        {
          if (startStation >= subsetStartStation && station <= subsetEndStation)
            AddCoord(x, y, station, offset); // add station if we have it
        }
        else
          AddCoord(x, y, station, offset); 
      }

      void AddSpotPointsFromElement(NFFStationedLineworkEntity element)
      {
        while (currentStation <= element.EndStation)
        {
          foreach (var offset in offsets)
          {
            element.ComputeXY(currentStation, offset, out var ptX, out var ptY); // get x y at current position
            AddPointAtStation(ptX, ptY, currentStation, offset, rangeTestRequired: false);
          }

          if (currentStation + crossSectionInterval > subsetEndStation && currentStation < subsetEndStation)
            currentStation = subsetEndStation; // making sure last point is picked up
          else
            currentStation += crossSectionInterval;
        }
      }

      // Run down the centre line from start to end station and add these vertices to the point list
      // Pattern of coords returned will be in order of left to right e.g. -2, -1, 0 (center line) , 1 ,2

      calcResult = DesignProfilerRequestResult.UnknownError;

      if (endStation <= startStation)
      {
        calcResult = DesignProfilerRequestResult.InvalidStationValues;
        return null;
      }

      // loop through all elements and add vertices where in range
      foreach (var currentElement in _data.Entities)
      {
        if (currentElement.EndStation < subsetStartStation) // ignore any early elements
          continue;

        if (currentElement.StartStation > subsetEndStation) // are we pass end of range
          break;

        // See if a test for end of range is required
        _ = !(currentElement.StartStation >= subsetStartStation && currentElement.EndStation <= subsetEndStation);

        AddSpotPointsFromElement(currentElement);
      }

      calcResult = DesignProfilerRequestResult.OK;  // Made it this far so flag OK
      return result;
    }

    public override void Dispose()
    {
      _data = null;
      _boundingBox = null;
    }
  }
}
