using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Designs.Models;
using VSS.TRex.Rendering.Palettes.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Rendering.GridFabric.Arguments
{
  public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TileRenderRequestArgument>();

    private const byte VERSION_NUMBER = 2;
    private static byte[] VERSION_NUMBERS = { 1, 2 };


    public DisplayMode Mode { get; set; } = DisplayMode.Height;

    public IPlanViewPalette Palette { get; set; }

    public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

    public bool CoordsAreGrid { get; set; }

    public ushort PixelsX { get; set; } = 256;
    public ushort PixelsY { get; set; } = 256;

    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    public TileRenderRequestArgument()
    { }

    public TileRenderRequestArgument(Guid siteModelId,
                                     DisplayMode mode,
                                     IPlanViewPalette palette,
                                     BoundingWorldExtent3D extents,
                                     bool coordsAreGrid,
                                     ushort pixelsX,
                                     ushort pixelsY,
                                     IFilterSet filters,
                                     DesignOffset referenceDesign,
                                     VolumeComputationType volumeType)
    {
      ProjectID = siteModelId;
      Mode = mode;
      Palette = palette;
      Extents = extents;
      CoordsAreGrid = coordsAreGrid;
      PixelsX = pixelsX;
      PixelsY = pixelsY;
      Filters = filters;
      ReferenceDesign = referenceDesign;
      VolumeType = volumeType;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        base.ToBinary(writer);

        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteInt((int)Mode);

        writer.WriteBoolean(Palette != null);
        Palette?.ToBinary(writer);

        writer.WriteBoolean(Extents != null);
        Extents.ToBinary(writer);

        writer.WriteBoolean(CoordsAreGrid);
        writer.WriteInt(PixelsX);
        writer.WriteInt(PixelsY);
        writer.WriteByte((byte)VolumeType);
      }
      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(TileRenderRequestArgument)}.ToBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderRequestArgument)}.ToBinary()");
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        base.FromBinary(reader);

        var messageVersion = VersionSerializationHelper.CheckVersionsByte(reader, VERSION_NUMBERS);

        Mode = (DisplayMode) reader.ReadInt();

        if (reader.ReadBoolean())
        {
          Palette = TileRenderRequestArgumentPaletteFactory.GetPalette(Mode);
          Palette.FromBinary(reader);
        }

        if (reader.ReadBoolean())
        {
          Extents = new BoundingWorldExtent3D();
          Extents.FromBinary(reader);
        }

        CoordsAreGrid = reader.ReadBoolean();
        PixelsX = (ushort) reader.ReadInt();
        PixelsY = (ushort) reader.ReadInt();
        if (messageVersion >= 2)
        {
          VolumeType = (VolumeComputationType) reader.ReadByte();
        }
      }

      catch (TRexSerializationVersionException e)
      {
        _log.LogError(e, $"Serialization version exception in {nameof(TileRenderRequestArgument)}.FromBinary()");
        throw; // Mostly for testing purposes...
      }
      catch (Exception e)
      {
        _log.LogCritical(e, $"Exception in {nameof(TileRenderRequestArgument)}.FromBinary()");
      }
    }
  }
}
