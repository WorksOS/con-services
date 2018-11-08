﻿using System;
using VSS.TRex.Cells;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Depending on the context, a layer may be equivalent to a pass over a cell
  /// or may represent a lift over a cell, in which case the Passes collection
  /// for the layer will contain the passes making up that lift.
  /// </summary>
  public class ProfileLayer : IProfileLayer
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileLayer>();

    /// <summary>
    /// Owner is the profile cell that owns this layer. This is important as the
    /// layer itself does not own the passes that comprise it, rather the profile
    /// cell does and the layer keeps track of the range of passes in the profile
    /// that the layer represents. This is mainly a performance measure to reduce
    /// the number of time the lists of cell passes get copied and reconstructed
    /// in the process of extracting them from the databace (by RequestSubGrid() etc)
    /// </summary>
    public ProfileCell Owner;

    /// <summary>
    /// StartCellPssIdx and EndCellPassIdx hold the start and end indices of
    /// the cell passes that are involved in the layer.
    /// </summary>
    public int StartCellPassIdx { get; set; }

    /// <summary>
    /// StartCellPssIdx and EndCellPassIdx hold the start and end indices of
    /// the cell passes that are involved in the layer.
    /// </summary>
    public int EndCellPassIdx { get; set; }

    /// <summary>
    /// ID of the machine who made the pass.  Not relevant if the layer does not relate to a pass over a cell
    /// </summary>
    public short MachineID { get; set; }

    /// <summary>
    /// the time, in seconds(based on a Unix/Linux date time encoding (i.e.since 1 Jan 1970)) or as a TDateTime. Use PSTimeToDateTime to convert to a TDateTime
    /// or as a TDateTime. Use PSTimeToDateTime to convert to a TDateTime
    /// </summary>
    public DateTime LastLayerPassTime { get; set; }

    public short CCV { get; set; } // The compaction value recorded for this layer
    public DateTime CCV_Time { get; set; } // Transient - no persistence - used for calculation of TargetCCV
    public short CCV_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetCCV
    public float CCV_Elev { get; set; } // The height of the cell pass from which the CCV came from
    public short TargetCCV { get; set; } // The target compaction value recorded for this layer
    public int CCV_CellPassIdx { get; set; } // Used to calculate previous values for CCV\MDP

    public short MDP { get; set; } // The mdp compaction value recorded for this layer
    public DateTime MDP_Time { get; set; } // Transient - no persistence - used for calculation of TargetMDP
    public short MDP_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetMDP
    public float MDP_Elev { get; set; } // The height of the cell pass from which the MDP came from
    public short TargetMDP { get; set; } // The target mdp compaction value recorded for this layer

    public byte CCA { get; set; } // The cca compaction value recorded for this layer
    public DateTime CCA_Time { get; set; } // Transient - no persistence - used for calculation of TargetCCA
    public short CCA_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetCCA
    public float CCA_Elev { get; set; } // The height of the cell pass from which the CCA came from
    public short TargetCCA { get; set; } // The target cca compaction value recorded for this layer

    public byte RadioLatency { get; set; }
    public ushort TargetPassCount { get; set; }
    public float Thickness { get; set; } // The calculated thickness of the lift
    public float TargetThickness { get; set; }
    public float Height { get; set; } // The actual height of the surface of this layer
    public short RMV { get; set; }
    public ushort Frequency { get; set; }
    public ushort Amplitude { get; set; }
    public ushort MaterialTemperature { get; set; }
    public DateTime MaterialTemperature_Time { get; set; }
    public short MaterialTemperature_MachineID { get; set; }
    public float MaterialTemperature_Elev { get; set; } // The height of the cell pass from which the Temperature came from

    public LayerStatus Status { get; set; } // The status of the layer; complete, undercompacted, etc.

    /// <summary>
    /// The calculated maximum thickness of any pass in this layer (when interested in "uncompacted" lift thickness)
    /// </summary>
    public float MaxThickness { get; set; }

    public int FilteredPassCount { get; set; }
    public int FilteredHalfPassCount { get; set; }

    public float MinimumPassHeight { get; set; }
    public float MaximumPassHeight { get; set; }
    public float FirstPassHeight { get; set; }
    public float LastPassHeight { get; set; }

    /// <summary>
    /// Default no-arg constructor, not to be used as the profile layer requires an owning ProfileCell
    /// </summary>
    public ProfileLayer()
    {
      throw new ArgumentException(
        "No-args Create constructor for ProfileLayer may not be called. Use Create() with Owner instead");
    }

    /// <summary>
    /// Createa a new defaulted profile layer with the given ProfileCell as owner
    /// </summary>
    /// <param name="owner"></param>
    public ProfileLayer(ProfileCell owner)
    {
      Owner = owner;
      Clear();
    }

    /// <summary>
    /// Clears this profile layer to the default state
    /// </summary>
    public void Clear()
    {
      StartCellPassIdx = -1;
      EndCellPassIdx = -1;

      MachineID = -1;
      LastLayerPassTime = DateTime.MinValue;

      CCV = CellPassConsts.NullCCV;
      TargetCCV = CellPassConsts.NullCCV;
      MDP = CellPassConsts.NullMDP;
      TargetMDP = CellPassConsts.NullMDP;
      CCA = CellPassConsts.NullCCA;
      TargetCCA = CellPassConsts.NullCCA;

      RadioLatency = CellPassConsts.NullRadioLatency;
      Height = CellPassConsts.NullHeight;
      TargetPassCount = 0;

      RMV = CellPassConsts.NullRMV;

      Thickness = CellPassConsts.NullHeight;
      TargetThickness = CellPassConsts.NullHeight;

      MaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      MaterialTemperature_Elev = CellPassConsts.NullHeight;

      MinimumPassHeight = CellPassConsts.NullHeight;
      MaximumPassHeight = CellPassConsts.NullHeight;
      FirstPassHeight = CellPassConsts.NullHeight;
      LastPassHeight = CellPassConsts.NullHeight;
    }

    /// <summary>
    /// Assigns the cell passes contained in a set of filtered pass values into this layer
    /// </summary>
    /// <param name="cellPassValues"></param>
    public void Assign(FilteredMultiplePassInfo cellPassValues)
    {
      Clear();

      if (Owner.Layers.Count() != 0)
      {
        Log.LogCritical("Cannot add a layer via FilteredPassValues if there are already layers in the cell");
        return;
      }

      if (cellPassValues.PassCount > 0)
      {
        Owner.Passes.FilteredPassData = new FilteredPassData[cellPassValues.PassCount];
        Array.Copy(cellPassValues.FilteredPassData, Owner.Passes.FilteredPassData, cellPassValues.PassCount);

        Owner.Passes.PassCount = cellPassValues.PassCount;

        StartCellPassIdx = 0;
        EndCellPassIdx = cellPassValues.PassCount - 1;

        CellPass LastPass = Owner.Passes.FilteredPassData[EndCellPassIdx].FilteredPass;
        CellTargets TargetValues = Owner.Passes.FilteredPassData[EndCellPassIdx].TargetValues;

        Height = LastPass.Height;
        LastLayerPassTime = LastPass.Time;
        CCV = LastPass.CCV;
        MDP = LastPass.MDP;
        CCA = LastPass.CCA;
        TargetCCV = TargetValues.TargetCCV;
        TargetMDP = TargetValues.TargetMDP;
        TargetCCA = TargetValues.TargetCCA;
        RadioLatency = LastPass.RadioLatency;
        Frequency = LastPass.Frequency;
        RMV = LastPass.RMV;
        Amplitude = LastPass.Amplitude;
        MaterialTemperature = LastPass.MaterialTemperature;
      }
    }

    /// <summary>
    /// Assigns the contents of anothern prfile layer to this profile layer
    /// </summary>
    /// <param name="source_"></param>
    public void Assign(IProfileLayer source_)
    {
      ProfileLayer source = (ProfileLayer) source_;

      MachineID = source.MachineID;
      LastLayerPassTime = source.LastLayerPassTime;

      CCV = source.CCV;
      TargetCCV = source.TargetCCV;
      MDP = source.MDP;
      TargetMDP = source.TargetMDP;
      CCA = source.CCA;
      TargetCCA = source.TargetCCA;

      RadioLatency = source.RadioLatency;

      StartCellPassIdx = source.StartCellPassIdx;
      EndCellPassIdx = source.EndCellPassIdx;

      TargetPassCount = source.TargetPassCount;
      Status = source.Status;
      Thickness = source.Thickness;
      TargetThickness = source.Thickness;
      Height = source.Height;
      RMV = source.RMV;
      Frequency = source.Frequency;
      Amplitude = source.Amplitude;
      MaterialTemperature = source.MaterialTemperature;
      MaxThickness = source.MaxThickness;
    }

    // procedure LoadFromStream(const Stream : TStream);
    // procedure SaveToStream(const Stream : TStream);

    /// <summary>
    /// The number of cell passes within the layer
    /// </summary>
    public int PassCount => EndCellPassIdx - StartCellPassIdx + 1;

    /// <summary>
    /// Records the addition of a cell pass identified by its index in the overall set passes for
    /// the cell being analysed. The pass itself is not physically added, but the index range of
    /// cells included in the layer is nodified to take the newly added cell pass into account
    /// </summary>
    /// <param name="passIndex"></param>
    public void AddPass(int passIndex)
    {
      if (StartCellPassIdx == -1)
        StartCellPassIdx = passIndex;

      EndCellPassIdx = passIndex;
    }
  }
}
