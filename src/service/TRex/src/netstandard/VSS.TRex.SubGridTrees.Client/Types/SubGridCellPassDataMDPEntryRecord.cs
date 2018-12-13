using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.SubGridTrees.Types
{
  /// <summary>
  /// Contains measured and target MDP values.
  /// </summary>
  public struct SubGridCellPassDataMDPEntryRecord
  {
    #region CellPassFlags
    private const byte kMDPUndercompactedBitFlag = 1;
    private const byte kMDPTooThickBitFlag = 2;
    private const byte kMDPTopLayerTooThickFlag = 3;
    private const byte kMDPOvercompactedBitFlag = 4;
    private const byte kMDPTopLayerUndercompactedFlag = 5;

    /// <summary>
    /// Storage for bit flags used for cell pass data statistics.
    /// </summary>
    private byte CellPassFlags;

    /// <summary>
    /// Specifies if undercompacted.
    /// </summary>
    public bool IsUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, kMDPUndercompactedBitFlag); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, kMDPUndercompactedBitFlag, value); }
    }

    /// <summary>
    /// Specifies if too thick.
    /// </summary>
    public bool IsTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, kMDPTooThickBitFlag); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, kMDPTooThickBitFlag, value); }
    }

    /// <summary>
    /// Specifies if top layer is too thick.
    /// </summary>
    public bool IsTopLayerTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, kMDPTopLayerTooThickFlag); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, kMDPTopLayerTooThickFlag, value); }
    }

    /// <summary>
    /// Specifies if top layer is undercompacted.
    /// </summary>
    public bool IsTopLayerUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, kMDPTopLayerUndercompactedFlag); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, kMDPTopLayerUndercompactedFlag, value); }
    }

    /// <summary>
    /// Specifies if overcompacted.
    /// </summary>
    public bool IsOvercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, kMDPOvercompactedBitFlag); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, kMDPOvercompactedBitFlag, value); }
    }
    #endregion

    /// <summary>
    /// Measured MDP value.
    /// </summary>
    public short MeasuredMDP { get; set; }

    /// <summary>
    /// Target MDP value.
    /// </summary>
    public short TargetMDP { get; set; }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public static int IndicativeSizeInBytes() => 2 * sizeof(short);

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="measuredMDP"></param>
    /// <param name="targetMDP"></param>
    public SubGridCellPassDataMDPEntryRecord(short measuredMDP, short targetMDP)
    {
      CellPassFlags = 0;
      MeasuredMDP = measuredMDP;
      TargetMDP = targetMDP;
    }

    /// <summary>
    /// Initializes the measured and target MDP properties with null values.
    /// </summary>
    public void Clear()
    {
      MeasuredMDP = CellPassConsts.NullMDP;
      TargetMDP = CellPassConsts.NullMDP;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredMDP);
      writer.Write(TargetMDP);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredMDP = reader.ReadInt16();
      TargetMDP = reader.ReadInt16();
    }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataMDPEntryRecord NullValue = Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell value type
    /// </summary>
    /// <returns></returns>
    private static SubGridCellPassDataMDPEntryRecord Null()
    {
      SubGridCellPassDataMDPEntryRecord result = new SubGridCellPassDataMDPEntryRecord();
      result.Clear();
      return result;
    }

    public bool Equals(SubGridCellPassDataMDPEntryRecord other)
    {
      return IsOvercompacted == other.IsOvercompacted &&
             IsTooThick == other.IsTooThick &&
             IsTopLayerTooThick == other.IsTopLayerTooThick &&
             IsTopLayerUndercompacted == other.IsTopLayerUndercompacted &&
             IsUndercompacted == other.IsUndercompacted &&
             MeasuredMDP == other.MeasuredMDP &&
             TargetMDP == other.TargetMDP;
    }
  }
}
