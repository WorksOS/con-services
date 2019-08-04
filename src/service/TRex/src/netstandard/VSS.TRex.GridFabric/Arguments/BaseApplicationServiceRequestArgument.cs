using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  ///  Forms the base request argument state that specific application service request contexts may leverage. It's roles include
  ///  containing the identifier of a TRex Application Service Node that originated the request
  /// </summary>
  public class BaseApplicationServiceRequestArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

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
    /// The set of filters to be applied to the requested sub grids
    /// </summary>
    public IFilterSet Filters { get; set; }

    /// <summary>
    /// The design to be used in cases of cut/fill or DesignHeights sub grid requests  together with its offset for a reference surface.
    /// </summary>
    public DesignOffset ReferenceDesign { get; set; } = new DesignOffset();

    /// <summary>
    /// Any overriding targets to be used instead of machine targets
    /// </summary>
    public IOverrideParameters Overrides { get; set; } = new OverrideParameters();

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    public ILiftParameters LiftParams { get; set; } = new LiftParameters();

    // TODO  LiftBuildSettings  :TICLiftBuildSettings;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(TRexNodeID);
      writer.WriteGuid(ProjectID);

      writer.WriteBoolean(ReferenceDesign != null);
      ReferenceDesign?.ToBinary(writer);

      writer.WriteBoolean(Filters != null);
      Filters?.ToBinary(writer);

      writer.WriteBoolean(Overrides != null);
      Overrides?.ToBinary(writer);

      writer.WriteBoolean(LiftParams != null);
      LiftParams?.ToBinary(writer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      TRexNodeID = reader.ReadString();
      ProjectID = reader.ReadGuid() ?? Guid.Empty;

      ReferenceDesign = new DesignOffset();
      if (reader.ReadBoolean())
        ReferenceDesign.FromBinary(reader);

      if (reader.ReadBoolean())
      {
        Filters = DI.DIContext.Obtain<IFilterSet>();
        Filters.FromBinary(reader);
      }

      if (reader.ReadBoolean())
      {
        Overrides = new OverrideParameters();
        Overrides.FromBinary(reader);
      }

      if (reader.ReadBoolean())
      {
        LiftParams = new LiftParameters();
        LiftParams.FromBinary(reader);
      }
    }
  }
}
