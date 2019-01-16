using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// A design comprised of linework components which describe a road (or part of)
  /// </summary>

  public class AlignmentDesign : DesignBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    private double _minHeight;
    private double _maxHeight;
    private readonly double _cellSize;
    private readonly ISubGridTreeBitMask _subgridIndex;

    public byte[] Data { get; set; } // assuming here there will be some kind of SDK model

    public OptimisedSpatialIndexSubGridTree SpatialIndexOptimised { get; private set; }
    private static readonly float[,] kNullPatch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    static AlignmentDesign()
    {
      // todo when SDK available
      SubGridUtilities.SubGridDimensionalIterator((x, y) => kNullPatch[x, y] = Common.Consts.NullHeight);
    }

    /// <summary>
    /// Constructor for a AlignmentDesign that takes the underlying cell size for the site model that will be used when interpolating heights from the design surface
    /// </summary>
    /// <param name="cellSize"></param>
    public AlignmentDesign(double cellSize)
    {
      // todo when SDK available
      Data = new byte[0];
      this._cellSize = cellSize;
    }


    /// <summary>
    /// Loads the Alignment from an Alignment file, along with the subgrid existence map file if it exists (created otherwise)
    /// </summary>
    /// <param name="localPathAndFileName"></param>
    /// <returns></returns>
    public override DesignLoadResult LoadFromFile(string localPathAndFileName)
    {
      // todo when SDK available
      try
      {
        Data = File.ReadAllBytes(localPathAndFileName);
        Log.LogInformation($"Loaded alignment file {localPathAndFileName} containing bytecount: {Data.Length}.");

        return DesignLoadResult.Success;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception in LoadFromFile");
        return DesignLoadResult.UnknownFailure;
      }
      return DesignLoadResult.Success;
    }

    /// <summary>
    /// Loads the Alignment file/s, from storage
    /// Includes design file and 2 index files (if they exist)
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <param name="localPath"></param>
    /// <param name="loadIndices"></param>
    /// <returns></returns>
    public override DesignLoadResult LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      var isDownloaded = S3FileTransfer.ReadFile(siteModelUid, fileName, localPath).Result;
      if (!isDownloaded)
      {
        return DesignLoadResult.UnknownFailure;
      }

      // todo when SDK available
      //if (loadIndices)
      //{
      //  isDownloaded = S3FileTransfer.ReadFile(siteModelUid, (fileName + Consts.kDesignSubgridIndexFileExt), TRexServerConfig.PersistentCacheStoreLocation).Result;
      //  if (!isDownloaded)
      //  {
      //    return DesignLoadResult.UnableToLoadSubgridIndex;
      //  }

      //  isDownloaded = S3FileTransfer.ReadFile(siteModelUid, (fileName + Consts.kDesignSpatialIndexFileExt), TRexServerConfig.PersistentCacheStoreLocation).Result;
      //  if (!isDownloaded)
      //  {
      //    return DesignLoadResult.UnableToLoadSpatialIndex;
      //  }
      //}

      return DesignLoadResult.Success;
    }


    /// <summary>
    /// Retrieves the ground extents of the Alignment 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      // todo when SDK available
      x1 = 6;
      y1 = 34;
      x2 = 8;
      y2 = 38;
    }

    /// <summary>
    /// Retrieves the elevation range of the vertices in the alignment surface
    /// </summary>
    /// <param name="z1"></param>
    /// <param name="z2"></param>
    public override void GetHeightRange(out double z1, out double z2)
    {
      // todo when SDK available
      z1 = _minHeight;
      z2 = _maxHeight;
    }

    /// <summary>
    /// Interpolates a single spot height from the design, using the optimized spatial index
    /// </summary>
    /// <param name="Hint"></param>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Offset"></param>
    /// <param name="Z"></param>
    /// <returns></returns>
    public override bool InterpolateHeight(ref int Hint,
      double X, double Y,
      double Offset,
      out double Z)
    {
      // todo when SDK available
      Z = Common.Consts.NullReal;
      return false;
    }

    /// <summary>
    /// Interpolates heights from the design for all the cells in a subgrid
    /// </summary>
    /// <param name="Patch"></param>
    /// <param name="OriginX"></param>
    /// <param name="OriginY"></param>
    /// <param name="CellSize"></param>
    /// <param name="Offset"></param>
    /// <returns></returns>
    public override bool InterpolateHeights(float[,] Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      // todo when SDK available
      return false;
    }


    public override bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset,
      SubGridTreeBitmapSubGridBits Mask,
      SubGridTreeBitmapSubGridBits Patch,
      double OriginX, double OriginY,
      double CellSize,
      double Offset)
    {
      // todo when SDK available
      return false;
    }


    public override bool HasElevationDataForSubGridPatch(double X, double Y)
    {
      // todo when SDK available
      return false;
    }

    public override bool HasElevationDataForSubGridPatch(uint SubGridX, uint SubGridY) => _subgridIndex[SubGridX, SubGridY];

    public override bool HasFiltrationDataForSubGridPatch(double X, double Y) => false;

    public override bool HasFiltrationDataForSubGridPatch(uint SubGridX, uint SubgridY) => false;

    /// <summary>
    /// A reference to the internal subgrid existence map for the design
    /// </summary>
    /// <returns></returns>
    public override ISubGridTreeBitMask SubgridOverlayIndex() => _subgridIndex;

    /// <summary>
    /// Computes the requested geometric profile over the design and returns the result
    /// as a vector of X, Y, Z, Station & TriangleIndex records
    /// </summary>
    /// <param name="profilePath"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    public override List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      // todo when SDK available
      return null;
    }
  }
}
