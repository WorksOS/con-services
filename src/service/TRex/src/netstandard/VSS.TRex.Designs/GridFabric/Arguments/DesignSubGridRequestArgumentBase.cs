using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class DesignSubGridRequestArgumentBase : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The offset to be applied to computed elevations
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public DesignSubGridRequestArgumentBase()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="referenceDesignUID"></param>
    /// <param name="offset"></param>
    protected DesignSubGridRequestArgumentBase(Guid siteModelID,
                                     Guid referenceDesignUID,
                                     double offset) : this()
    {
      ProjectID = siteModelID;
      ReferenceDesignUID = referenceDesignUID;
      Offset = offset;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> SiteModel:{ProjectID}, Design:{ReferenceDesignUID}, Offset:{Offset}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ReferenceDesignUID);
      writer.WriteDouble(Offset);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ReferenceDesignUID = reader.ReadGuid() ?? Guid.Empty;
      Offset = reader.ReadDouble();
    }
  }
}
