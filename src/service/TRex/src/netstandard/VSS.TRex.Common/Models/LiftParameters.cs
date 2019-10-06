using Apache.Ignite.Core.Binary;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Types;
using VSS.TRex.Types.Types;

namespace VSS.TRex.Common.Models
{
  /// <summary>
  /// Parameters used for lift analysis
  /// </summary>
  public class LiftParameters : ILiftParameters, IBinarizable
  {
    private const byte VERSION_NUMBER = 1;

    public bool OverrideMachineThickness { get; set; }
    public LiftThicknessType LiftThicknessType { get; set; }
    public double OverridingLiftThickness { get; set; }
    public CCVSummaryTypes CCVSummaryTypes { get; set; }
    public bool CCVSummarizeTopLayerOnly { get; set; }
    public float FirstPassThickness { get; set; }
    public MDPSummaryTypes MDPSummaryTypes { get; set; }
    public bool MDPSummarizeTopLayerOnly { get; set; }
    public LiftDetectionType LiftDetectionType { get; set; }
    public bool IncludeSuperseded { get; set; }
    //Parameters controlling TargetLiftThicknessSummary overlay
    public double TargetLiftThickness { get; set; }
    public double AboveToleranceLiftThickness { get; set; }
    public double BelowToleranceLiftThickness { get; set; }
    // Boundaries extending above/below a cell pass constituting the dead band
    public double DeadBandLowerBoundary { get; set; }
    public double DeadBandUpperBoundary { get; set; }

    public LiftParameters()
    {
      OverrideMachineThickness = false;
      LiftThicknessType = LiftThicknessType.Compacted;
      OverridingLiftThickness = CellPassConsts.NullOverridingTargetLiftThicknessValue;
      CCVSummaryTypes = CCVSummaryTypes.None;
      CCVSummarizeTopLayerOnly = true;//match Raptor
      FirstPassThickness = 0.0f;
      MDPSummaryTypes = MDPSummaryTypes.None;
      MDPSummarizeTopLayerOnly = true;
      LiftDetectionType = LiftDetectionType.None;
      IncludeSuperseded = false;
      TargetLiftThickness = 0.0;
      AboveToleranceLiftThickness = 0.0;
      BelowToleranceLiftThickness = 0.0;
      DeadBandLowerBoundary = 0.0;
      DeadBandUpperBoundary = 0.0;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(OverrideMachineThickness);
      writer.WriteInt((int)LiftThicknessType);
      writer.WriteDouble(OverridingLiftThickness);
      writer.WriteInt((int)CCVSummaryTypes);
      writer.WriteBoolean(CCVSummarizeTopLayerOnly);
      writer.WriteFloat(FirstPassThickness);
      writer.WriteInt((int)MDPSummaryTypes);
      writer.WriteBoolean(MDPSummarizeTopLayerOnly);
      writer.WriteInt((int)LiftDetectionType);
      writer.WriteBoolean(IncludeSuperseded);
      writer.WriteDouble(TargetLiftThickness);
      writer.WriteDouble(AboveToleranceLiftThickness);
      writer.WriteDouble(BelowToleranceLiftThickness);
      writer.WriteDouble(DeadBandLowerBoundary);
      writer.WriteDouble(DeadBandUpperBoundary);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OverrideMachineThickness = reader.ReadBoolean();
      LiftThicknessType = (LiftThicknessType) reader.ReadInt();
      OverridingLiftThickness = reader.ReadDouble();
      CCVSummaryTypes = (CCVSummaryTypes)reader.ReadInt();
      CCVSummarizeTopLayerOnly = reader.ReadBoolean();
      FirstPassThickness = reader.ReadFloat();
      MDPSummaryTypes = (MDPSummaryTypes)reader.ReadInt();
      MDPSummarizeTopLayerOnly = reader.ReadBoolean();
      LiftDetectionType = (LiftDetectionType)reader.ReadInt();
      IncludeSuperseded = reader.ReadBoolean();
      TargetLiftThickness = reader.ReadDouble();
      AboveToleranceLiftThickness = reader.ReadDouble();
      BelowToleranceLiftThickness = reader.ReadDouble();
      DeadBandLowerBoundary = reader.ReadDouble();
      DeadBandUpperBoundary = reader.ReadDouble();
    }

    //TODO: refactor the BaseRequestArgument to have a base base class with just the below methods to avoid this duplication

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialize this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

  }
}
