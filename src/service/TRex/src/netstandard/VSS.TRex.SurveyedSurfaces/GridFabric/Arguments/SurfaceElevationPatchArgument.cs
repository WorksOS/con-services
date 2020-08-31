using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Caching;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Arguments
{
  public class SurfaceElevationPatchArgument : BaseRequestArgument, ISurfaceElevationPatchArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The ID of the SiteModel to execute the request against
    /// </summary>
    public Guid SiteModelID { get; set; }

    /// <summary>
    /// The bottom left on-the-ground cell origin X location for the patch of elevations to be computed from
    /// </summary>
    public int OTGCellBottomLeftX { get; set; }

    /// <summary>
    /// The bottom left on-the-ground cell origin Y location for the patch of elevations to be computed from
    /// </summary>
    public int OTGCellBottomLeftY { get; set; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Determines which surface information should be extracted: Earliest, Latest or Composite
    /// </summary>
    public SurveyedSurfacePatchType SurveyedSurfacePatchType { get; set; }

    /// <summary>
    /// A map of the cells within the sub grid patch to be computed
    /// </summary>
    public SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

    /// <summary>
    /// The list of surveyed surfaces to be included in the calculation
    /// [Note: This is fairly inefficient, the receiver of the request should be able to access surveyed surfaces locally...]
    /// </summary>
    public Guid[] IncludedSurveyedSurfaces { get; set; }

    /// <summary>
    /// Constructor taking the full state of the surface patch computation operation
    /// </summary>
    public SurfaceElevationPatchArgument(Guid siteModelID,
      int oTGCellBottomLeftX,
      int oTGCellBottomLeftY,
      double cellSize,
      SurveyedSurfacePatchType surveyedSurfacePatchType,
      SubGridTreeBitmapSubGridBits processingMap,
      ISurveyedSurfaces includedSurveyedSurfaces)
    {
      SiteModelID = siteModelID;
      OTGCellBottomLeftX = oTGCellBottomLeftX;
      OTGCellBottomLeftY = oTGCellBottomLeftY;
      CellSize = cellSize;
      SurveyedSurfacePatchType = surveyedSurfacePatchType;
      ProcessingMap = processingMap == null ? new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled) : new SubGridTreeBitmapSubGridBits(processingMap);

      // Prepare the list of surveyed surfaces for use by all invocations using this argument
      includedSurveyedSurfaces?.SortChronologically(surveyedSurfacePatchType == SurveyedSurfacePatchType.LatestSingleElevation);
      IncludedSurveyedSurfaces = includedSurveyedSurfaces?.Select(x => x.ID).ToArray() ?? new Guid[0];
    }

    public SurfaceElevationPatchArgument()
    {
    }

    /// <summary>
    /// Sets the location of the surveyed surface sub grid to be requested without modifying any other aspect
    /// of the request
    /// </summary>
    public void SetOTGBottomLeftLocation(int oTGCellBottomLeftX, int oTGCellBottomLeftY)
    {
      OTGCellBottomLeftX = oTGCellBottomLeftX;
      OTGCellBottomLeftY = oTGCellBottomLeftY;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{SiteModelID}, OTGOriginBL:{OTGCellBottomLeftX}/{OTGCellBottomLeftY}, CellSize:{CellSize}, SurfacePatchType:{SurveyedSurfacePatchType}";
    }

    /// <summary>
    /// Computes a Fingerprint for use in caching surveyed surface height + time responses
    /// Note: This fingerprint used the SurveyedSurfaceHeightAndTime grid data type in the cache fingerprint,
    /// even though the core engine returns HeightAndTime results. This allows HeightAndTime and
    /// SurveyedSurfaceHeightAndTime results to cohabit in the same cache
    /// </summary>
    public string CacheFingerprint()
    {
      return SpatialCacheFingerprint.ConstructFingerprint(SiteModelID, GridDataType.SurveyedSurfaceHeightAndTime, null, IncludedSurveyedSurfaces);
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(SiteModelID);
      writer.WriteInt((int)OTGCellBottomLeftX);
      writer.WriteInt((int)OTGCellBottomLeftY);
      writer.WriteDouble(CellSize);
      writer.WriteByte((byte)SurveyedSurfacePatchType);

      writer.WriteBoolean(ProcessingMap != null);
      if (ProcessingMap != null)
        writer.WriteByteArray(ProcessingMap.ToBytes());

      writer.WriteBoolean(IncludedSurveyedSurfaces != null);
      if (IncludedSurveyedSurfaces != null)
      {
        var count = IncludedSurveyedSurfaces.Length;
        writer.WriteInt(count);
        for (int i = 0; i < count; i++)
          writer.WriteGuid(IncludedSurveyedSurfaces[i]);
      }
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        SiteModelID = reader.ReadGuid() ?? Guid.Empty;
        OTGCellBottomLeftX = reader.ReadInt();
        OTGCellBottomLeftY = reader.ReadInt();
        CellSize = reader.ReadDouble();
        SurveyedSurfacePatchType = (SurveyedSurfacePatchType) reader.ReadByte();

        if (reader.ReadBoolean())
        {
          ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
          ProcessingMap.FromBytes(reader.ReadByteArray());
        }

        if (reader.ReadBoolean())
        {
          var count = reader.ReadInt();
          IncludedSurveyedSurfaces = new Guid[count];
          for (int i = 0; i < count; i++)
            IncludedSurveyedSurfaces[i] = reader.ReadGuid() ?? Guid.Empty;
        }
      }
    }
  }
}
