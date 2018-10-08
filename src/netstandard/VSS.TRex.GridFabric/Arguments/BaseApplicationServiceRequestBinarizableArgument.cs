using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  ///  Forms the base request argument state that specific application service request contexts may leverage. It's roles include
  ///  containing the identifier of a TRex Application Service Node that originated the request
  /// </summary>
  public class BaseApplicationServiceRequestBinarizableArgument : BaseRequestBinarizableArgument, IFromToBinary
  {
    private const byte versionNumber = 1;

    // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor

    /// <summary>
    /// The identifier of the TRex node responsible for issuing a request and to which messages containing responses
    /// should be sent on a message topic contained within the derived request. 
    /// </summary>
    public string TRexNodeID { get; set; } = string.Empty;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The set of filters to be applied to the requested subgrids
    /// </summary>
    public IFilterSet Filters { get; set; }

    /// <summary>
    /// The design to be used in cases of cut/fill or DesignHeights subgrid requests
    /// </summary>
    public Guid ReferenceDesignID { get; set; } = Guid.Empty;

    // TODO  LiftBuildSettings  :TICLiftBuildSettings;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteString(TRexNodeID);
      writer.WriteGuid(ProjectID);
      writer.WriteGuid(ReferenceDesignID);

      Filters.ToBinary(writer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      TRexNodeID = reader.ReadString();
      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      ReferenceDesignID = reader.ReadGuid() ?? Guid.Empty;

      Filters.FromBinary(reader);
    }
  }
}
