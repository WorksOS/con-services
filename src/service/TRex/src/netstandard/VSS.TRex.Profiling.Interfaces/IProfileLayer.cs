using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters.Models;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLayer
  {
    /// <summary>
    /// StartCellPssIdx and EndCellPassIdx hold the start and end indices of
    /// the cell passes that are involved in the layer.
    /// </summary>
    int StartCellPassIdx { get; set; }

    /// <summary>
    /// StartCellPssIdx and EndCellPassIdx hold the start and end indices of
    /// the cell passes that are involved in the layer.
    /// </summary>
    int EndCellPassIdx { get; set; }

    /// <summary>
    /// ID of the machine who made the pass.  Not relevant if the layer does not relate to a pass over a cell
    /// </summary>
    short MachineID { get; set; }

    /// <summary>
    /// the time, in seconds(based on a Unix/Linux date time encoding (i.e.since 1 Jan 1970)) or as a TDateTime. Use PSTimeToDateTime to convert to a TDateTime
    /// or as a TDateTime. Use PSTimeToDateTime to convert to a TDateTime
    /// </summary>
    DateTime LastLayerPassTime { get; set; }

    short CCV { get; set; } // The compaction value recorded for this layer
    DateTime CCV_Time { get; set; } // Transient - no persistence - used for calculation of TargetCCV
    short CCV_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetCCV
    float CCV_Elev { get; set; } // The height of the cell pass from which the CCV came from
    short TargetCCV { get; set; } // The target compaction value recorded for this layer
    int CCV_CellPassIdx { get; set; } // Used to calculate previous values for CCV\MDP

    short MDP { get; set; } // The mdp compaction value recorded for this layer
    DateTime MDP_Time { get; set; } // Transient - no persistence - used for calculation of TargetMDP
    short MDP_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetMDP
    float MDP_Elev { get; set; } // The height of the cell pass from which the MDP came from
    short TargetMDP { get; set; } // The target mdp compaction value recorded for this layer

    byte CCA { get; set; } // The cca compaction value recorded for this layer
    DateTime CCA_Time { get; set; } // Transient - no persistence - used for calculation of TargetCCA
    short CCA_MachineID { get; set; } // Transient - no persistence - used for calculation of TargetCCA
    float CCA_Elev { get; set; } // The height of the cell pass from which the CCA came from
    short TargetCCA { get; set; } // The target cca compaction value recorded for this layer

    byte RadioLatency { get; set; }
    ushort TargetPassCount { get; set; }
    float Thickness { get; set; } // The calculated thickness of the lift
    float TargetThickness { get; set; }

    float Height { get; set; }

    short RMV { get; set; }
    ushort Frequency { get; set; }
    ushort Amplitude { get; set; }
    ushort MaterialTemperature { get; set; }
    DateTime MaterialTemperature_Time { get; set; }
    short MaterialTemperature_MachineID { get; set; }
    float MaterialTemperature_Elev { get; set; } // The height of the cell pass from which the Temperature came from

    /// <summary>
    /// The calculated maximum thickness of any pass in this layer (when interested in "uncompacted" lift thickness)
    /// </summary>
    float MaxThickness { get; set; }
    
    int FilteredPassCount { get; set; }
    int FilteredHalfPassCount { get; set; }

    float MinimumPassHeight { get; set; }
    float MaximumPassHeight { get; set; }
    float FirstPassHeight { get; set; }
    float LastPassHeight { get; set; }

    LayerStatus Status { get; set; }

    /// <summary>
    /// Clears this profile layer to the default state
    /// </summary>
    void Clear();

    /// <summary>
    /// Assigns the cell passes contained in a set of filtered pass values into this layer
    /// </summary>
    /// <param name="cellPassValues"></param>
    void Assign(FilteredMultiplePassInfo cellPassValues);

    /// <summary>
    /// Assigns the contents of another profile layer to this profile layer
    /// </summary>
    /// <param name="source"></param>
    void Assign(IProfileLayer source);

    /// <summary>
    /// The number of cell passes within the layer
    /// </summary>
    int PassCount { get; }

    /// <summary>
    /// Records the addition of a cell pass identified by its index in the overall set passes for
    /// the cell being analysed. The pass itself is not physically added, but the index range of
    /// cells included in the layer is nodified to take the newly added cell pass into account
    /// </summary>
    /// <param name="passIndex"></param>
    void AddPass(int passIndex);

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    void ToBinary(IBinaryRawWriter writer);

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    void FromBinary(IBinaryRawReader reader);
  }
}
