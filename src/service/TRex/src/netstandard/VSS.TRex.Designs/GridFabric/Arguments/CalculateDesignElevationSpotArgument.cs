using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignElevationSpotArgument : DesignSubGridRequestArgumentBase
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The X origin location for the spot elevation to be computed from
    /// </summary>
    public double SpotX { get; set; }

    /// <summary>
    /// The Y origin location for the spot elevation to be computed from
    /// </summary>
    public double SpotY { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignElevationSpotArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="spotX"></param>
    /// <param name="spotY"></param>
    /// <param name="referenceDesign"></param>
    public CalculateDesignElevationSpotArgument(Guid siteModelID,
      double spotX,
      double spotY,
      DesignOffset referenceDesign) : base(siteModelID, referenceDesign)
    {
      SpotX = spotX;
      SpotY = spotY;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Location:{SpotX}/{SpotY}, Design:{ReferenceDesign?.DesignID}, Offset:{ReferenceDesign?.Offset}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(SpotX);
      writer.WriteDouble(SpotY);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        SpotX = reader.ReadDouble();
        SpotY = reader.ReadDouble();
      }
    }
  }
}
