using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public class ClientCellProfileAllPassesLeafSubgrid : GenericClientLeafSubGrid<ClientCellProfileAllPassesLeafSubgridRecord>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ClientCellProfileLeafSubgrid>();

    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientCellProfileAllPassesLeafSubgrid()
    {
      var nullRecord = new ClientCellProfileAllPassesLeafSubgridRecord();
      nullRecord.Clear();
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = nullRecord);
    }

    public override bool WantsLiftProcessingResults() => true;

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.CellPasses;

      EventPopulationFlags |= 
        PopulationControlFlags.WantsTargetPassCountValues |
        PopulationControlFlags.WantsTargetCCVValues |
        PopulationControlFlags.WantsTargetMDPValues |
        // PopulationControlFlags.WantsEventGPSModeValues        |   todo??
        PopulationControlFlags.WantsEventGPSAccuracyValues |
        PopulationControlFlags.WantsTargetThicknessValues |
        PopulationControlFlags.WantsEventVibrationStateValues |
        PopulationControlFlags.WantsEventMachineGearValues |
        PopulationControlFlags.WantsEventDesignNameValues;
    }

    public ClientCellProfileAllPassesLeafSubgrid() : base()
    {
      Initialise();
    }

    /// <summary>
    /// Constructor. Set the grid to CellProfile.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCellProfileAllPassesLeafSubgrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Time == DateTime.MinValue;

    public override void Clear()
    {
      Array.Copy(NullCells, Cells, SubGridTreeConsts.CellsPerSubgrid);
      TopLayerOnly = false;
    }

    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    {
      if (context.CellProfile == null)
      {
        Log.LogError($"{nameof(AssignFilteredValue)}: Error=CellProfile not assigned.");
        return;
      }

      IProfileCell cellProfileFromContext = context.CellProfile as IProfileCell;

      Cells[cellX, cellY].TotalPasses = cellProfileFromContext.Passes.PassCount;

      // Cells[CellX, CellY].CellPasses is an array of cell passes we will update
      Cells[cellX, cellY].CellPasses = new ClientCellProfileLeafSubgridRecord[cellProfileFromContext.Passes.PassCount];      

      //int iTotalHalfPasses = cellProfileFromContext.TotalNumberOfHalfPasses(true); // include superseded layers
      //int iTotalWholePasses = cellProfileFromContext.TotalNumberOfWholePasses(true); // include superseded layers

      // foreach layer
      for (int i = 0; i < cellProfileFromContext.Layers.Count(); i++)
      {
        int k = 0;
        // cycle through layer and update pass record (Cells[CellX, CellY].CellPasses[j])
        for (int j = cellProfileFromContext.Layers[i].StartCellPassIdx; i <= cellProfileFromContext.Layers[i].EndCellPassIdx; j++)
        {
        //  if (j >= iTotalHalfPasses) // protect this from range check error that can bring down ps node
        //    continue;

          k++;

          Cells[cellX, cellY].CellPasses[j].CellXOffset = context.ProbePositions[cellX, cellY].XOffset;
          Cells[cellX, cellY].CellPasses[j].CellYOffset = context.ProbePositions[cellX, cellY].YOffset;
          Cells[cellX, cellY].CellPasses[j].LastPassTime = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.Time;
          Cells[cellX, cellY].CellPasses[j].PassCount = k; // in this case pass id in this layer
          Cells[cellX, cellY].CellPasses[j].LastPassValidRadioLatency = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.RadioLatency;
          Cells[cellX, cellY].CellPasses[j].EventDesignNameID = cellProfileFromContext.Passes.FilteredPassData[j].EventValues.EventDesignNameID;
          Cells[cellX, cellY].CellPasses[j].InternalSiteModelMachineIndex = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.InternalSiteModelMachineIndex;
          Cells[cellX, cellY].CellPasses[j].MachineSpeed = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.MachineSpeed;
          Cells[cellX, cellY].CellPasses[j].LastPassValidGPSMode = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.gpsMode;
          Cells[cellX, cellY].CellPasses[j].GPSTolerance = cellProfileFromContext.Passes.FilteredPassData[j].EventValues.GPSTolerance;
          Cells[cellX, cellY].CellPasses[j].GPSAccuracy = cellProfileFromContext.Passes.FilteredPassData[j].EventValues.GPSAccuracy;
          Cells[cellX, cellY].CellPasses[j].TargetPassCount = cellProfileFromContext.Passes.FilteredPassData[j].TargetValues.TargetPassCount;
          //Cells[cellX, cellY].CellPasses[j].TotalHalfPasses = iTotalHalfPasses;
          //Cells[cellX, cellY].CellPasses[j].TotalWholePasses = iTotalWholePasses;
          Cells[cellX, cellY].CellPasses[j].LayersCount = i + 1; // layer
          Cells[cellX, cellY].CellPasses[j].LastPassValidCCV = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.CCV;
          Cells[cellX, cellY].CellPasses[j].TargetCCV = cellProfileFromContext.Passes.FilteredPassData[j].TargetValues.TargetCCV;
          Cells[cellX, cellY].CellPasses[j].LastPassValidMDP = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.MDP;
          Cells[cellX, cellY].CellPasses[j].TargetMDP = cellProfileFromContext.Passes.FilteredPassData[j].TargetValues.TargetMDP;
          Cells[cellX, cellY].CellPasses[j].LastPassValidRMV = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.RMV;
          Cells[cellX, cellY].CellPasses[j].LastPassValidFreq = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.Frequency;
          Cells[cellX, cellY].CellPasses[j].LastPassValidAmp = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.Amplitude;
          Cells[cellX, cellY].CellPasses[j].TargetThickness = cellProfileFromContext.Passes.FilteredPassData[j].TargetValues.TargetLiftThickness;
          Cells[cellX, cellY].CellPasses[j].EventMachineGear = cellProfileFromContext.Passes.FilteredPassData[j].EventValues.EventMachineGear;
          Cells[cellX, cellY].CellPasses[j].EventVibrationState = cellProfileFromContext.Passes.FilteredPassData[j].EventValues.EventVibrationState;
          Cells[cellX, cellY].CellPasses[j].LastPassValidTemperature = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.MaterialTemperature;
          Cells[cellX, cellY].CellPasses[j].Height = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.Height;
          Cells[cellX, cellY].CellPasses[j].HalfPass = cellProfileFromContext.Passes.FilteredPassData[j].FilteredPass.HalfPass;
        }
      }
    }

    public override bool CellHasValue(byte cellX, byte cellY)
    {
      return Cells[cellX, cellY].CellPasses.Length > 0 ? Cells[cellX, cellY].CellPasses[0].LastPassTime != DateTime.MinValue : false;
    }

    public override void FillWithTestPattern()
    {
      ForEach((x, y) =>
      {
        Cells[x, y] = new ClientCellProfileAllPassesLeafSubgridRecord
        {
          TotalPasses = 1,
          CellPasses = new []
          {
            new ClientCellProfileLeafSubgridRecord
            {
              LastPassTime = new DateTime(1000 * x + y + 1)
              // Add others as appropriate here
            }
          } 
        };
      });
    }

    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<ClientCellProfileAllPassesLeafSubgridRecord> _other = (IGenericClientLeafSubGrid<ClientCellProfileAllPassesLeafSubgridRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    public override ClientCellProfileAllPassesLeafSubgridRecord NullCell()
    {
      var nullRecord = new ClientCellProfileAllPassesLeafSubgridRecord();
      nullRecord.Clear();
      return nullRecord;
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }
  }
}
