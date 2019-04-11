using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.GridFabric.Arguments
{
  public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public DisplayMode Mode { get; set; } = DisplayMode.Height;

    public IPlanViewPalette Palette { get; set; }

    public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

    public bool CoordsAreGrid { get; set; }

    public ushort PixelsX { get; set; } = 256;
    public ushort PixelsY { get; set; } = 256;

    public TileRenderRequestArgument()
    { }

    public TileRenderRequestArgument(Guid siteModelID,
                                     DisplayMode mode,
                                     PaletteBase palette,
                                     BoundingWorldExtent3D extents,
                                     bool coordsAreGrid,
                                     ushort pixelsX,
                                     ushort pixelsY,
                                     IFilterSet filters,
                                     Guid referenceDesignUid)
    {
      ProjectID = siteModelID;
      Mode = mode;
      Palette = palette;
      Extents = extents;
      CoordsAreGrid = coordsAreGrid;
      PixelsX = pixelsX;
      PixelsY = pixelsY;
      Filters = filters;
      ReferenceDesignUID = referenceDesignUid;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)Mode);

      writer.WriteBoolean(Palette != null);
      ((PaletteBase)Palette)?.ToBinary(writer);

      writer.WriteBoolean(Extents != null);
      Extents.ToBinary(writer);

      writer.WriteBoolean(CoordsAreGrid);
      writer.WriteInt(PixelsX);
      writer.WriteInt(PixelsY);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Mode = (DisplayMode)reader.ReadInt();

      if (reader.ReadBoolean())
      {
        Palette = GetPalette();
        ((PaletteBase)Palette).FromBinary(reader);
      }

      if (reader.ReadBoolean())
      {
        Extents = new BoundingWorldExtent3D();
        Extents.FromBinary(reader);
      }

      CoordsAreGrid = reader.ReadBoolean();
      PixelsX = (ushort)reader.ReadInt();
      PixelsY = (ushort)reader.ReadInt();
    }

    private PaletteBase GetPalette()
    {
      switch (Mode)
      {
        case DisplayMode.CCA:
          return new CCAPalette();
        case DisplayMode.CCASummary:
          return new CCASummaryPalette();
        case DisplayMode.CCV:
          return new CMVPalette();
        case DisplayMode.CCVPercentSummary:
          return new CMVSummaryPalette();
        case DisplayMode.CutFill:
          return new CutFillPalette();
        case DisplayMode.Height:
          return new HeightPalette();
        case DisplayMode.MDP:
          return new MDPPalette();
        case DisplayMode.MDPPercentSummary:
          return new MDPSummaryPalette();
        case DisplayMode.PassCount:
          return new PassCountPalette();
        case DisplayMode.PassCountSummary:
          return new PassCountSummaryPalette();
        case DisplayMode.MachineSpeed:
          return new SpeedPalette();
        case DisplayMode.TargetSpeedSummary:
          return new SpeedSummaryPalette();
        case DisplayMode.TemperatureDetail:
          return new TemperaturePalette();
        case DisplayMode.TemperatureSummary:
          return new TemperatureSummaryPalette();
        default:
            throw new TRexException($"No implemented colour palette for this mode ({Mode})");
      }
    }
  }
}
