using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Utilities;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  /// <summary>
  /// Contains measured and target CCA values as well as previous measured and target CCA values.
  /// </summary>
  public struct SubGridCellPassDataCCAEntryRecord
  {
    private const byte BYTE_TYPES_COUNT = 4;

    #region CellPassFlags
    private const byte CCA_DECOUPLED_BIT_FLAG = 0;
    private const byte CCA_UNDERCOMPACTED_BIT_FLAG = 1;
    private const byte CCA_TOO_THICK_BIT_FLAG = 2;
    private const byte CCA_TOP_LAYER_TOO_THICK_BIT_FLAG = 3;
    private const byte CCA_OVERCOMPACTED_BIT_FLAG = 4;
    private const byte CCA_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG = 5;

    /// <summary>
    /// Storage for bit flags used for cell pass data statistics.
    /// </summary>
    private byte CellPassFlags;

    /// <summary>
    /// Specifies if decoupled.
    /// </summary>
    public bool IsDecoupled
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_DECOUPLED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_DECOUPLED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if under compacted.
    /// </summary>
    public bool IsUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if too thick.
    /// </summary>
    public bool IsTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is too thick.
    /// </summary>
    public bool IsTopLayerTooThick
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_TOP_LAYER_TOO_THICK_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_TOP_LAYER_TOO_THICK_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if top layer is under compacted.
    /// </summary>
    public bool IsTopLayerUndercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_TOP_LAYER_UNDERCOMPACTED_BIT_FLAG, value); }
    }

    /// <summary>
    /// Specifies if over compacted.
    /// </summary>
    public bool IsOvercompacted
    {
      get { return BitFlagHelper.IsBitOn(CellPassFlags, CCA_OVERCOMPACTED_BIT_FLAG); }
      set { BitFlagHelper.SetBit(ref CellPassFlags, CCA_OVERCOMPACTED_BIT_FLAG, value); }
    }
    #endregion

    /// <summary>
    /// Measured CMV value.
    /// </summary>
    public byte MeasuredCCA { get; set; }

    /// <summary>
    /// Target CMV value.
    /// </summary>
    public byte TargetCCA { get; set; }

    /// <summary>
    /// Previous measured CMV value.
    /// </summary>
    public byte PreviousMeasuredCCA { get; set; }

    /// <summary>
    /// Previous target CMV value.
    /// </summary>
    public byte PreviousTargetCCA { get; set; }

    public static int IndicativeSizeInBytes() => BYTE_TYPES_COUNT * sizeof(byte) + sizeof(byte); // 4 shorts and a flags byte

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="measuredCCA"></param>
    /// <param name="targetCCA"></param>
    /// <param name="previousMeasuredCCA"></param>
    /// <param name="previousTargetCCA"></param>
    public SubGridCellPassDataCCAEntryRecord(byte measuredCCA, byte targetCCA, byte previousMeasuredCCA, byte previousTargetCCA)
    {
      CellPassFlags = 0;
      MeasuredCCA = measuredCCA;
      TargetCCA = targetCCA;
      PreviousMeasuredCCA = previousMeasuredCCA;
      PreviousTargetCCA = previousTargetCCA;
    }

    /// <summary>
    /// Initializes the measured and target CMV properties with null values.
    /// </summary>
    public void Clear()
    {
      CellPassFlags = 0;
      MeasuredCCA = CellPassConsts.NullCCA;
      TargetCCA = CellPassConsts.NullCCATarget;
      PreviousMeasuredCCA = CellPassConsts.NullCCA;
      PreviousTargetCCA = CellPassConsts.NullCCATarget;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredCCA);
      writer.Write(TargetCCA);
      writer.Write(PreviousMeasuredCCA);
      writer.Write(PreviousTargetCCA);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredCCA = reader.ReadByte();
      TargetCCA = reader.ReadByte();
      PreviousMeasuredCCA = reader.ReadByte();
      PreviousTargetCCA = reader.ReadByte();
    }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataCCAEntryRecord NullValue = Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell value type
    /// </summary>
    /// <returns></returns>
    private static SubGridCellPassDataCCAEntryRecord Null()
    {
      SubGridCellPassDataCCAEntryRecord result = new SubGridCellPassDataCCAEntryRecord();
      result.Clear();

      return result;
    }
  }
}
