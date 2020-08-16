using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// A design comprised of line work components which describe a road (or part of)
  /// </summary>

  public class AlignmentDesign : DesignBase
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AlignmentDesign>();

    public byte[] Data { get; set; } // assuming here there will be some kind of SDK model

    static AlignmentDesign()
    {
    }

    /// <summary>
    /// Constructor for a AlignmentDesign that takes the underlying cell size for the site model that will be used when interpolating heights from the design surface
    /// </summary>
    public AlignmentDesign(double cellSize)
    {
      // todo when SDK available
      Data = new byte[0];
    }


    /// <summary>
    /// Loads the Alignment from an Alignment file, along with the sub grid existence map file if it exists (created otherwise)
    /// </summary>
    public override DesignLoadResult LoadFromFile(string localPathAndFileName, bool saveIndexFiles = true)
    {
      // todo when SDK available
      try
      {
        Data = File.ReadAllBytes(localPathAndFileName);
        _log.LogInformation($"Loaded alignment file {localPathAndFileName} containing byte count: {Data.Length}.");

        return DesignLoadResult.Success;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in LoadFromFile");
        return DesignLoadResult.UnknownFailure;
      }
    }

    /// <summary>
    /// Loads the Alignment file/s, from storage
    /// Includes design file and 2 index files (if they exist)
    /// </summary>
    public override async Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath,
      bool loadIndices = false)
    {
      var a3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
      var isDownloaded = await a3FileTransfer.ReadFile(siteModelUid, fileName, localPath);

      return await Task.FromResult(isDownloaded ? DesignLoadResult.Success : DesignLoadResult.UnknownFailure);
    }


    /// <summary>
    /// Retrieves the ground extents of the Alignment
    /// </summary>
    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      // todo when SDK available if appropriate
      x1 = 6;
      y1 = 34;
      x2 = 8;
      y2 = 38;
    }

    public override BoundingWorldExtent3D GetExtents() => new BoundingWorldExtent3D(6, 34, 8, 38);

    /// <summary>
    /// Retrieves the elevation range of the vertices in the alignment surface
    /// </summary>
    public override void GetHeightRange(out double z1, out double z2)
    {
      // todo when SDK available if appropriate
      z1 = Consts.NullDouble;
      z2 = Consts.NullDouble;
    }

    /// <summary>
    /// Interpolates a single spot height from the design, using the optimized spatial index
    /// </summary>
    public override bool InterpolateHeight(ref int hint,
      double x, double y,
      double offset,
      out double z)
    {
      // todo when SDK available if appropriate
      z = Consts.NullReal;
      return false;
    }

    /// <summary>
    /// Interpolates heights from the design for all the cells in a sub grid
    /// </summary>
    public override bool InterpolateHeights(float[,] patch, double originX, double originY, double cellSize, double offset)
    {
      // todo when SDK available
      return false;
    }


    public override bool ComputeFilterPatch(double startStn, double endStn, double leftOffset, double rightOffset,
      SubGridTreeBitmapSubGridBits mask,
      SubGridTreeBitmapSubGridBits patch,
      double originX, double originY,
      double cellSize,
      double offset)
    {
      // todo when SDK available
      return false;
    }


    public override bool HasElevationDataForSubGridPatch(double x, double y)
    {
      // todo when SDK available
      return false;
    }

    public override bool HasElevationDataForSubGridPatch(int subGridX, int subGridY) => false;

    public override bool HasFiltrationDataForSubGridPatch(double x, double y) => false;

    public override bool HasFiltrationDataForSubGridPatch(int subGridX, int subGridY) => false;

    /// <summary>
    /// Computes the requested geometric profile over the design and returns the result
    /// as a vector of X, Y, Z, Station & TriangleIndex records
    /// </summary>
    public override List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      // todo when SDK available
      return null;
    }

    /// <summary>
    /// Computes the requested boundary.
    /// </summary>
    public override List<Fence> GetBoundary()
    {
      // todo when SDK available
      return null;
    }

    /// <summary>
    /// Remove file from storage
    /// </summary>
    public override bool RemoveFromStorage(Guid siteModelUid, string fileName)
    {
      var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
      return s3FileTransfer.RemoveFileFromBucket(siteModelUid, fileName);
    }

    public override long SizeInCache()
    {
      return 10 * 1024; // 10Kb 
    }

    public override void Dispose()
    {
    }
  }
}
