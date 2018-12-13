using System;
using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Utilities;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  /// <summary>
  /// Contains measured and target MDP values.
  /// </summary>
  public struct SubGridCellPassDataMDPEntryRecord
  {
    private const byte SHORT_TYPES_COUNT = 2;

    #region CellPassFlags
    private const byte MDP_UNDERCOMPACTED_BIT_FLAG = 1;
    private const byte MDP_TOO_THICK_BIT_FLAG = 2;
    private const byte MDP_TOP_LAYER_TOO_THICK_BIT_FLAG = 3;
    private const byte MDP_OVERCOMPACTED_BIT_FLAG = 4;
    private const byte MDP_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG = 5;

    /// <summary>
    /// Storage for bit flags used for cell pass data statistics.
    /// </summary>
    private byte CellPassFlags;

    /// <summary>
    /// Specifies if undercompacted.
    /// </summary>
    public bool IsUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, MDP_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, MDP_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if too thick.
    /// </summary>
    public bool IsTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, MDP_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, MDP_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is too thick.
    /// </summary>
    public bool IsTopLayerTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, MDP_TOP_LAYER_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, MDP_TOP_LAYER_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is undercompacted.
    /// </summary>
    public bool IsTopLayerUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, MDP_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, MDP_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if overcompacted.
    /// </summary>
    public bool IsOvercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, MDP_OVERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, MDP_OVERCOMPACTED_BIT_FLAG, value); }
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
    public static int IndicativeSizeInBytes() => SHORT_TYPES_COUNT * sizeof(short) + sizeof(byte);

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
  }
}
