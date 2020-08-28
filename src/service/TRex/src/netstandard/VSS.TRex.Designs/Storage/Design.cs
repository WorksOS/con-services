using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Storage
{
  /// <summary>
  /// Represents the information known about a design
  /// </summary>
  public class Design : IEquatable<IDesign>, IBinaryReaderWriter, IDesign
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<Design>();

    private CalculateDesignElevationPatch _designElevationCalculator = new CalculateDesignElevationPatch();
    /// <summary>
    /// Binary serialization logic
    /// </summary>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      _extents.Write(writer);
    }

    /// <summary>
    /// Binary deserialization logic
    /// </summary>
    public void Read(BinaryReader reader)
    {
      ID = reader.ReadGuid();
      DesignDescriptor.Read(reader);
      _extents.Read(reader);
    }

    /// <summary>
    /// The internal identifier of the design
    /// </summary>
    public Guid ID { get; private set; } = Guid.Empty;

    /// <summary>
    /// The full design descriptor representing the design
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; }

    /// <summary>
    /// Computes a geometric profile across the design given a series of vertices describing the path to be profiled.
    /// </summary>
    public async Task<(List<XYZS> profile, DesignProfilerRequestResult errorCode)> ComputeProfile(Guid projectUid, WGS84Point startPoint, WGS84Point endPoint, double cellSize, double offset, bool arePositionsGrid)
    {
      // Query the DesignProfiler service to get the patch of elevations calculated
      try
      {
        var profileRequest = new DesignProfileRequest();
        var arg = new CalculateDesignProfileArgument
        {
          ProjectID = projectUid,
          CellSize = cellSize,
          StartPoint = startPoint,
          EndPoint = endPoint,
          PositionsAreGrid = arePositionsGrid
        };
        arg.ReferenceDesign.DesignID = DesignDescriptor.DesignID;
        arg.ReferenceDesign.Offset = offset;

        var profileResult = await profileRequest.ExecuteAsync(arg);
        return (profileResult.Profile, profileResult.RequestResult);
      }
      catch
      {
        return (null, DesignProfilerRequestResult.UnknownError );
      }
    }

    /// <summary>
    /// The rectangular bounding extents of the design in grid coordinates
    /// </summary>
    private readonly BoundingWorldExtent3D _extents;

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public Design()
    {
      _extents = new BoundingWorldExtent3D();
      DesignDescriptor = new DesignDescriptor();
    }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
    /// </summary>
    public BoundingWorldExtent3D Extents => new BoundingWorldExtent3D(_extents);

    /// <summary>
    /// Constructor accepting full design state
    /// </summary>
    public Design(Guid iD,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents)
    {
      ID = iD;
      DesignDescriptor = designDescriptor;
      _extents = extents;
    }

    /// <summary>
    /// Produces a deep clone of the design
    /// </summary>
    public IDesign Clone() => new Design(ID, DesignDescriptor, new BoundingWorldExtent3D(Extents));

    /// <summary>
    /// ToString() for Design
    /// </summary>
    public override string ToString()
    {
      return $"ID:{ID}, DesignID:{DesignDescriptor.DesignID};{DesignDescriptor.Folder};{DesignDescriptor.FileName} [{Extents}]";
    }

    /// <summary>
    /// Determine if two designs are equal
    /// </summary>
    public bool Equals(IDesign other)
    {
      return other != null &&
             ID == other.ID &&
             DesignDescriptor.Equals(other.DesignDescriptor) &&
             Extents.Equals(other.Extents);
    }

    /// <summary>
    /// Calculates a spot elevation designated location on this design
    /// </summary>
    public async Task<(double spotHeight, DesignProfilerRequestResult errorCode)> GetDesignSpotHeight(Guid siteModelId, double offset, double spotX, double spotY)
    {
      // Query the DesignProfiler service to get the spot elevation calculated
      var elevSpotRequest = new DesignElevationSpotRequest();

      var response = await elevSpotRequest.ExecuteAsync(new CalculateDesignElevationSpotArgument
        (siteModelId, spotX, spotY, new DesignOffset(DesignDescriptor.DesignID, offset)));

      return (response.Elevation, response.CalcResult);
    }

    /// <summary>
    /// Calculates an elevation sub grid for a designated sub grid on this design
    /// </summary>
    public (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode) GetDesignHeightsViaLocalCompute(
      ISiteModelBase siteModel,
      double offset,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      var heightsResult = _designElevationCalculator.Execute(siteModel, new DesignOffset(DesignDescriptor.DesignID, offset), 
        cellSize, originCellAddress.X, originCellAddress.Y, out var calcResult);

      return (heightsResult, calcResult);
    }

    /// <summary>
    /// Calculates an elevation sub grid for a designated sub grid on this design
    /// </summary>
    public async Task<(IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode)> GetDesignHeightsViaDesignElevationService(
      Guid siteModelId,
      double offset,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      // Query the DesignProfiler service to get the patch of elevations calculated
      var elevPatchRequest = new DesignElevationPatchRequest();

      var response = await elevPatchRequest.ExecuteAsync(new CalculateDesignElevationPatchArgument
      {
        CellSize = cellSize,
        ReferenceDesign = new DesignOffset(DesignDescriptor.DesignID, offset),
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        ProjectID = siteModelId
      });

      return (response.Heights, response.CalcResult);
    }

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    public (SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode) GetFilterMaskViaLocalCompute(
      ISiteModelBase siteModel,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      // Calculate an elevation patch for the requested location and convert it into a bitmask detailing which cells have non-null values
      var patch = _designElevationCalculator.Execute(siteModel, new DesignOffset(DesignDescriptor.DesignID, 0),
        cellSize, originCellAddress.X, originCellAddress.Y, out var calcResult);

      if (patch == null)
      {
        _log.LogWarning($"Request for design elevation patch that does not exist: Project: {siteModel.ID}, design {DesignDescriptor.DesignID}, location {originCellAddress.X}:{originCellAddress.Y}, calcResult: {calcResult}");
        return (null, calcResult); // Requestors should not ask for sub grids that don't exist in the design.
      }

      var mask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
      var patchCells = patch.Cells;

      for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (patchCells[i, j].Equals(Common.Consts.NullHeight))
            mask[i, j] = true;
        }
      }

      return (mask, calcResult);
    }

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    public async Task<(SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode)> GetFilterMaskViaDesignElevationService(
      Guid siteModelId,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      // Query the DesignProfiler service to get the requested filter mask
      var filterMaskRequest = new DesignFilterSubGridMaskRequest();

      var maskResponse = await filterMaskRequest.ExecuteAsync(new DesignSubGridFilterMaskArgument
      {
        CellSize = cellSize,
        ReferenceDesign = new DesignOffset(DesignDescriptor.DesignID, 0),
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        ProjectID = siteModelId
      });

      return (maskResponse?.Bits, maskResponse.RequestResult);
    }
  }
}
