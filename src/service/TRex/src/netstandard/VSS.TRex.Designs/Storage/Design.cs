using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Designs.Storage
{
  /// <summary>
  /// Represents the information known about a design
  /// </summary>
  public class Design : IEquatable<IDesign>, IBinaryReaderWriter, IDesign
  {
    /// <summary>
    /// Binary serialization logic
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      _extents.Write(writer);
    }

    /// <summary>
    /// Binary deserialization logic
    /// </summary>
    /// <param name="reader"></param>
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
    /// <param name="projectUid"></param>
    /// <param name="profilePath"></param>
    /// <param name="cellSize"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<(List<XYZS> profile, DesignProfilerRequestResult errorCode)> ComputeProfile(Guid projectUid, XYZ[] profilePath, double cellSize, double offset)
    {
      (List<XYZS> profile, DesignProfilerRequestResult errorCode) result = (null, DesignProfilerRequestResult.OK);

      // Query the DesignProfiler service to get the patch of elevations calculated
      try
      {
        var profileRequest = new DesignProfileRequest();

        var profile = await profileRequest.ExecuteAsync(new CalculateDesignProfileArgument(projectUid, cellSize, DesignDescriptor.DesignID, offset, profilePath));

        result.profile = profile.Profile;

        return result;
      }
      catch
      {
        result.errorCode = DesignProfilerRequestResult.UnknownError;
      }

      return result;
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
    /// <returns></returns>
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
    public async Task<(IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode)> GetDesignHeights(
      Guid siteModelId,
      double offset,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      // Query the DesignProfiler service to get the patch of elevations calculated
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode) result = (null, DesignProfilerRequestResult.OK);

      var elevPatchRequest = new DesignElevationPatchRequest();

      var response = await elevPatchRequest.ExecuteAsync(new CalculateDesignElevationPatchArgument
      {
        CellSize = cellSize,
        ReferenceDesign = new DesignOffset(DesignDescriptor.DesignID, offset),
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        ProjectID = siteModelId
      });

      result.designHeights = response.Heights;
      result.errorCode = response.CalcResult;

      return result;
    }

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    public async Task<(SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode)> GetFilterMask(
      Guid siteModelId,
      SubGridCellAddress originCellAddress,
      double cellSize)
    {
      // Query the DesignProfiler service to get the requested filter mask
      (SubGridTreeBitmapSubGridBits filterMask, DesignProfilerRequestResult errorCode) result = (null, DesignProfilerRequestResult.OK);

      var filterMaskRequest = new DesignFilterSubGridMaskRequest(); 

      var maskResponse = await filterMaskRequest.ExecuteAsync(new DesignSubGridFilterMaskArgument
      {
        CellSize = cellSize,
        ReferenceDesign = new DesignOffset(DesignDescriptor.DesignID, 0),
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        ProjectID = siteModelId
      });

      result.filterMask = maskResponse?.Bits;

      return result;
    }
  }
}
