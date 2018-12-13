using System;
using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  /// <summary>
  /// Contains measured and target CMV values as well as previous measured and target CMV values.
  /// </summary>
  public struct SubGridCellPassDataCMVEntryRecord
  {
    private const byte SHORT_TYPES_COUNT = 4;

    #region CellPassFlags
    private const byte CMV_DECOUPLED_BIT_FLAG = 0;
    private const byte CMV_UNDERCOMPACTED_BIT_FLAG = 1;
    private const byte CMV_TOO_THICK_BIT_FLAG = 2;
    private const byte CMV_TOP_LAYER_TOO_THICK_BIT_FLAG = 3;
    private const byte CMV_OVERCOMPACTED_BIT_FLAG = 4;
    private const byte CMV_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG = 5;

    /// <summary>
    /// Storage for bit flags used for cell pass data statistics.
    /// </summary>
    private byte CellPassFlags;

    /// <summary>
    /// Specifies if decoupled.
    /// </summary>
    public bool IsDecoupled
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_DECOUPLED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_DECOUPLED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if under compacted.
    /// </summary>
    public bool IsUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if too thick.
    /// </summary>
    public bool IsTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is too thick.
    /// </summary>
    public bool IsTopLayerTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_TOP_LAYER_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_TOP_LAYER_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is under compacted.
    /// </summary>
    public bool IsTopLayerUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if over compacted.
    /// </summary>
    public bool IsOvercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CMV_OVERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CMV_OVERCOMPACTED_BIT_FLAG, value); }
    }
    #endregion

    /// <summary>
    /// Measured CMV value.
    /// </summary>
    public short MeasuredCMV { get; set; }

    /// <summary>
    /// Target CMV value.
    /// </summary>
    public short TargetCMV { get; set; }

    /// <summary>
    /// Previous measured CMV value.
    /// </summary>
    public short PreviousMeasuredCMV { get; set; }

    /// <summary>
    /// Previous target CMV value.
    /// </summary>
    public short PreviousTargetCMV { get; set; }

    public static int IndicativeSizeInBytes() => SHORT_TYPES_COUNT * sizeof(short) + sizeof(byte); // 4 shorts and a flags byte

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="measuredCMV"></param>
    /// <param name="targetCMV"></param>
    /// <param name="previousMeasuredCMV"></param>
    /// <param name="previousTargetCMV"></param>
    public SubGridCellPassDataCMVEntryRecord(short measuredCMV, short targetCMV, short previousMeasuredCMV, short previousTargetCMV)
    {
      CellPassFlags = 0;
      MeasuredCMV = measuredCMV;
      TargetCMV = targetCMV;
      PreviousMeasuredCMV = previousMeasuredCMV;
      PreviousTargetCMV = previousTargetCMV;
    }

    /// <summary>
    /// Initializes the measured and target CMV properties with null values.
    /// </summary>
    public void Clear()
    {
      CellPassFlags = 0;
      MeasuredCMV = CellPassConsts.NullCCV;
      TargetCMV = CellPassConsts.NullCCV;
      PreviousMeasuredCMV = CellPassConsts.NullCCV;
      PreviousTargetCMV = CellPassConsts.NullCCV;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredCMV);
      writer.Write(TargetCMV);
      writer.Write(PreviousMeasuredCMV);
      writer.Write(PreviousTargetCMV);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredCMV = reader.ReadInt16();
      TargetCMV = reader.ReadInt16();
      PreviousMeasuredCMV = reader.ReadInt16();
      PreviousTargetCMV = reader.ReadInt16();
    }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataCMVEntryRecord NullValue = Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell value type
    /// </summary>
    /// <returns></returns>
    private static SubGridCellPassDataCMVEntryRecord Null()
    {
      SubGridCellPassDataCMVEntryRecord result = new SubGridCellPassDataCMVEntryRecord();
      result.Clear();
      return result;
    }
  }
}
