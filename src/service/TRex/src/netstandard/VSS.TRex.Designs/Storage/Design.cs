using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
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
    /// Singleton request used by all designs. This request encapsulates the Ignite reference which
    /// is relatively slow to initialise when making many calls.
    /// </summary>
    private DesignElevationSpotRequest elevSpotRequest;

    /// <summary>
    /// Singleton request used by all designs. This request encapsulates the Ignite reference which
    /// is relatively slow to initialise when making many calls.
    /// </summary>
    private DesignElevationPatchRequest elevPatchRequest;

    /// <summary>
    /// Singleton request used by all designs. This request encapsulates the Ignite reference which
    /// is relatively slow to initialise when making many calls.
    /// </summary>
    private DesignProfileRequest profileRequest;

    /// <summary>
    /// Singleton request used by all designs. This request encapsulates the Ignite reference which
    /// is relatively slow to initialise when making many calls.
    /// </summary>
    private DesignFilterSubGridMaskRequest filterMaskRequest; 

    /// <summary>
    /// Binary serialization logic
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      extents.Write(writer);
    }

    /// <summary>
    /// Binary serialization logic
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Binary deserialization logic
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      ID = reader.ReadGuid();
      DesignDescriptor.Read(reader);
      extents.Read(reader);
    }

    /// <summary>
    /// The internal identifier of the design
    /// </summary>
    public Guid ID { get; private set; } = Guid.Empty;

    /// <summary>
    /// The full design descriptor representing the design
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; private set; }

    /// <summary>
    /// Computes a geometric profile across the design given a series of vertices describing the path to be profiled.
    /// </summary>
    /// <param name="projectUID"></param>
    /// <param name="profilePath"></param>
    /// <param name="cellSize"></param>
    /// <param name="offset"></param>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public List<XYZS> ComputeProfile(Guid projectUID, XYZ[] profilePath, double cellSize, double offset, out DesignProfilerRequestResult errorCode)
    {
      // Query the DesignProfiler service to get the patch of elevations calculated
      errorCode = DesignProfilerRequestResult.OK;

      try
      {
        if (profileRequest == null)
          profileRequest = new DesignProfileRequest();

        var profile = profileRequest.Execute(new CalculateDesignProfileArgument(projectUID, cellSize, DesignDescriptor.DesignID, offset, profilePath));

        return profile.Profile;
      }
      catch
      {
        errorCode = DesignProfilerRequestResult.UnknownError;
      }

      return null;
    }

    /// <summary>
    /// The rectangular bounding extents of the design in grid coordinates
    /// </summary>
    private readonly BoundingWorldExtent3D extents;

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public Design()
    {
      extents = new BoundingWorldExtent3D();
      DesignDescriptor = new DesignDescriptor();
    }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
    /// </summary>
    public BoundingWorldExtent3D Extents
    {
      get
      {
        BoundingWorldExtent3D result = new BoundingWorldExtent3D(extents);

        return result;
      }
    }

    /// <summary>
    /// Constructor accepting full design state
    /// </summary>
    /// <param name="iD"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents_"></param>
    public Design(Guid iD,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents_)
    {
      ID = iD;
      DesignDescriptor = designDescriptor;
      extents = extents_;
    }

    /// <summary>
    /// Produces a deep clone of the design
    /// </summary>
    /// <returns></returns>
    public IDesign Clone() => new Design(ID, DesignDescriptor, new BoundingWorldExtent3D(Extents));

    /// <summary>
    /// ToString() for Design
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"ID:{ID}, DesignID:{DesignDescriptor.DesignID};{DesignDescriptor.Folder};{DesignDescriptor.FileName} [{Extents}]";
    }

    /// <summary>
    /// Determine if two designs are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
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
    /// <param name="siteModelID"></param>
    /// <param name="offset"></param>
    /// <param name="spotX"></param>
    /// <param name="spotY"></param>
    /// <param name="spotHeight"></param>
    /// <param name="errorCode"></param>
    public void GetDesignSpotHeight(Guid siteModelID, double offset,
      double spotX, double spotY,
      out double spotHeight,
      out DesignProfilerRequestResult errorCode)
    {
      // Query the DesignProfiler service to get the spot elevation calculated
      errorCode = DesignProfilerRequestResult.OK;

      if (elevSpotRequest == null)
        elevSpotRequest = new DesignElevationSpotRequest();

      spotHeight = elevSpotRequest.Execute(new CalculateDesignElevationSpotArgument
        (siteModelID, spotX, spotY, DesignDescriptor.DesignID, offset));
    }

    /// <summary>
    /// Calculates an elevation sub grid for a designated sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="offset"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <param name="designHeights"></param>
    /// <param name="errorCode"></param>
    public void GetDesignHeights(Guid siteModelID, double offset,
      SubGridCellAddress originCellAddress,
      double cellSize,
      out IClientHeightLeafSubGrid designHeights,
      out DesignProfilerRequestResult errorCode)
    {
      // Query the DesignProfiler service to get the patch of elevations calculated
      errorCode = DesignProfilerRequestResult.OK;
      designHeights = null;

      if (elevPatchRequest == null)
        elevPatchRequest = new DesignElevationPatchRequest();

      var response = elevPatchRequest.Execute(new CalculateDesignElevationPatchArgument
      {
        CellSize = cellSize,
        ReferenceDesign.DesignID = DesignDescriptor.DesignID,
        ReferenceDesign.Offset = offset,
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        // ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled),
        ProjectID = siteModelID
      });

      designHeights = response.Heights;
      errorCode = response.CalcResult;
    }

    /// <summary>
    /// Calculates a filter mask for a designated sub grid on this design
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="originCellAddress"></param>
    /// <param name="cellSize"></param>
    /// <param name="filterMask"></param>
    /// <param name="errorCode"></param>
    public void GetFilterMask(Guid siteModelID,
      SubGridCellAddress originCellAddress,
      double cellSize,
      out SubGridTreeBitmapSubGridBits filterMask,
      out DesignProfilerRequestResult errorCode)
    {
      // Query the DesignProfiler service to get the requested filter mask
      errorCode = DesignProfilerRequestResult.OK;

      if (filterMaskRequest == null)
        filterMaskRequest = new DesignFilterSubGridMaskRequest(); 

      var maskResponse = filterMaskRequest.Execute(new DesignSubGridFilterMaskArgument
      {
        CellSize = cellSize,
        ReferenceDesign.DesignID = DesignDescriptor.DesignID,
        OriginX = originCellAddress.X,
        OriginY = originCellAddress.Y,
        ProjectID = siteModelID
      });

      filterMask = maskResponse?.Bits;
    }
  }
}
