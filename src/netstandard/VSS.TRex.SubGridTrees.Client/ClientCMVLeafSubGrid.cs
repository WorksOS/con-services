﻿using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a CMV client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientCMVLeafSubGrid : GenericClientLeafSubGrid<SubGridCellPassDataCMVEntryRecord>
  {
    private bool _sWantsPreviousCCVValue;
    private bool _sIgnoresNullValueForLastCMV;

    public bool WantsPreviousCCVValue { get; set; }
    public bool IgnoresNullValueForLastCMV { get; set; }

    /// <summary>
    /// First pass map records which cells hold cell pass CMVs that were derived
    /// from the first pass a machine made over the corresponding cell
    /// </summary>
    public SubGridTreeBitmapSubGridBits FirstPassMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Initilise the null cell values for the client subgrid
    /// </summary>
    static ClientCMVLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = SubGridCellPassDataCMVEntryRecord.NullValue);
    }

    /// <summary>
    /// CMV subgrids require lift processing...
    /// </summary>
    /// <returns></returns>
    public override bool WantsLiftProcessingResults() => true;

    private void Initialise()
    {
      EventPopulationFlags |=
        PopulationControlFlags.WantsTargetCCAValues |
        PopulationControlFlags.WantsTargetThicknessValues |
        PopulationControlFlags.WantsEventVibrationStateValues |
        PopulationControlFlags.WantsEventDesignNameValues |
        PopulationControlFlags.WantsEventGPSAccuracyValues |
        PopulationControlFlags.WantsEventAutoVibrationStateValues |
        PopulationControlFlags.WantsEventICFlagsValues |
        PopulationControlFlags.WantsEventMachineGearValues |
        PopulationControlFlags.WantsEventMachineCompactionRMVJumpThreshold |
        PopulationControlFlags.WantsEventMachineAutomaticsValues |
        PopulationControlFlags.WantsEventMinElevMappingValues |
        PopulationControlFlags.WantsEventInAvoidZoneStateValues;

      if (WantsPreviousCCVValue)
      {
        _gridDataType = GridDataType.CCVPercentChange;

        if (IgnoresNullValueForLastCMV)
          _gridDataType = GridDataType.CCVPercentChangeIgnoredTopNullValue;
      }
      else
        _gridDataType = GridDataType.CCV;

      _sWantsPreviousCCVValue = WantsPreviousCCVValue;
      _sIgnoresNullValueForLastCMV = IgnoresNullValueForLastCMV;
    }

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    /// <param name="wantsPreviousCCVValue"></param>
    /// <param name="ignoresNullValueForLastCMV"></param>
    public ClientCMVLeafSubGrid(bool wantsPreviousCCVValue = false, bool ignoresNullValueForLastCMV = true) : base()
    {
      WantsPreviousCCVValue = wantsPreviousCCVValue;
      IgnoresNullValueForLastCMV = ignoresNullValueForLastCMV;

      Initialise();
    }

    /// <summary>
    /// Constructor. Set the grid to CCV.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCMVLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    /// <summary>
    /// Determine if a filtered CMV is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) =>
      _gridDataType != GridDataType.CCVPercentChange && IgnoresNullValueForLastCMV && filteredValue.FilteredPass.CCV == CellPassConsts.NullCCV;

    /// <summary>
    /// Assign filtered CMV value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="Context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      Cells[cellX, cellY].MeasuredCMV = Context.FilteredValue.FilteredPassData.FilteredPass.CCV;
      Cells[cellX, cellY].TargetCMV = Context.FilteredValue.FilteredPassData.TargetValues.TargetCCV;
      Cells[cellX, cellY].PreviousMeasuredCMV = Context.PreviousFilteredValue.FilteredPassData.FilteredPass.CCV;
      Cells[cellX, cellY].PreviousMeasuredCMV = Context.PreviousFilteredValue.FilteredPassData.TargetValues.TargetCCV;

      Cells[cellX, cellY].IsDecoupled = Context.PreviousFilteredValue.FilteredPassData.FilteredPass.RMV > Context.FilteredValue.FilteredPassData.EventValues.EventMachineRMVThreshold;
      Cells[cellX, cellY].IsUndercompacted = false;
      Cells[cellX, cellY].IsTooThick = false;
      Cells[cellX, cellY].IsTopLayerTooThick = false;
      Cells[cellX, cellY].IsTopLayerUndercompacted = false;
      Cells[cellX, cellY].IsOvercompacted = false;

      int lowLayerIndex = -1;
      int highLayerIndex = -1;

      IProfileLayers layers = ((IProfileCell) Context.CellProfile).Layers;

      if (Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly)
      {
        for (var i = layers.Count() - 1; i >= 0; i--)
        {
          if ((layers[i].Status & LayerStatus.Superseded) == 0)
          {
            lowLayerIndex = highLayerIndex = i;
            break;
          }
        }
      }
      else
      {
        for (var i = layers.Count() - 1; i >= 0; i--)
        {
          if ((layers[i].Status & LayerStatus.Superseded) == 0 && layers[i].CCV != CellPassConsts.NullCCV)
          {
            highLayerIndex = i;
            break;
          }
        }

        lowLayerIndex = highLayerIndex > -1 ? 0 : -1;
      }

      if (highLayerIndex > -1 && lowLayerIndex > -1)
      {
        for (var i = lowLayerIndex; i <= highLayerIndex; i++)
        {
          var layer = ((IProfileCell)Context.CellProfile).Layers[i];

          if (layer.FilteredPassCount == 0)
            continue;

          if ((layer.Status & LayerStatus.Undercompacted) != 0)
          {
            if (i == highLayerIndex)
              Cells[cellX, cellY].IsTopLayerUndercompacted = true;
            else
              Cells[cellX, cellY].IsUndercompacted = true;
          }

          if ((layer.Status & LayerStatus.Overcompacted) != 0)
            Cells[cellX, cellY].IsOvercompacted = true;

          if ((layer.Status & LayerStatus.TooThick) != 0)
          {
            if (i == highLayerIndex)
              Cells[cellX, cellY].IsTopLayerTooThick = true;
            else
              Cells[cellX, cellY].IsTooThick = true;
          }
        };
      }
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      const short DUMMY_CMV_VALUE = 1;

      ForEach((x, y) =>
      {
        Cells[x, y] = new SubGridCellPassDataCMVEntryRecord
        {
          MeasuredCMV = x,
          TargetCMV = y
        };

        if (y > 0)
        {
          Cells[x, y].PreviousMeasuredCMV = Cells[x, y - 1].MeasuredCMV;
          Cells[x, y].PreviousTargetCMV = Cells[x, y - 1].TargetCMV;
        }

        // The PreviousMeasuredCMV and PreviousTargetCMV values below are set to non-zero values as
        // these cannot be zeros.
        if (Cells[x, y].PreviousMeasuredCMV == 0)
          Cells[x, y].PreviousMeasuredCMV = DUMMY_CMV_VALUE;

        if (Cells[x, y].PreviousTargetCMV == 0)
          Cells[x, y].PreviousTargetCMV = DUMMY_CMV_VALUE;
      });
    }

    /// <summary>
    /// Determines if the CMV at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => _gridDataType == GridDataType.CCVPercentChange || Cells[cellX, cellY].MeasuredCMV != CellPassConsts.NullCCV;

    /// <summary>
    /// Provides a copy of the null value defined for cells in thie client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override SubGridCellPassDataCMVEntryRecord NullCell() => SubGridCellPassDataCMVEntryRecord.NullValue;

    /// <summary>
    /// Sets all cell CMVs to null and clears the first pass and sureyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      FirstPassMap.Clear();
    }

    public void RestoreInitialSettings()
    {
      WantsPreviousCCVValue = _sWantsPreviousCCVValue;
      IgnoresNullValueForLastCMV = _sIgnoresNullValueForLastCMV;
    }

    /// <summary>
    /// Dumps elevations from subgrid to the log
    /// </summary>
    /// <param name="title"></param>
    public override void DumpToLog(string title)
    {
        base.DumpToLog(title);
        /*
          * var
          I, J : Integer;
          S : String;
        begin
          SIGLogMessage.PublishNoODS(Nil, Format('Dump of machine speed map for subgrid %s', [Moniker]) , slmcDebug);

          for I := 0 to kSubGridTreeDimension - 1 do
            begin
              S := Format('%2d:', [I]);

              for J := 0 to kSubGridTreeDimension - 1 do
                if CellHasValue(I, J) then
                  S := S + Format('%9.3f', [Cells[I, J]])
                else
                  S := S + '     Null';

              SIGLogMessage.PublishNoODS(Nil, S, slmcDebug);
            end;
        end;
        */
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte [] buffer)
    {
      base.Write(writer, buffer);

      FirstPassMap.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte [] buffer)
    {
      base.Read(reader, buffer);

      FirstPassMap.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<SubGridCellPassDataCMVEntryRecord> _other = (IGenericClientLeafSubGrid<SubGridCellPassDataCMVEntryRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() +
             FirstPassMap.IndicativeSizeInBytes() +
             SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.SubGridTreeDimension * SubGridCellPassDataCMVEntryRecord.IndicateSizeInBytes();
    }
  }
}
