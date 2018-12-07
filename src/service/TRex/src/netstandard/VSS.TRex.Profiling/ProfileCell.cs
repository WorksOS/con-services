using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.Types;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;

namespace VSS.TRex.Profiling
{
  public class ProfileCellBase : IProfileCellBase
  {
    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// The real-world length of that part of the profile line which crosses the underlying cell;
    /// used to determine the width of the profile column as displayed in the client GUI
    /// </summary>
    public double InterceptLength { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellX { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellY { get; set; }

    public float DesignElev { get; set; }
  }

  /// <summary>
  /// ProfileCell is a package of information relating to one cell in a profile drawn across IC data
  /// </summary>
  public class ProfileCell : ProfileCellBase, IProfileCell
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileCell>();

    /// <summary>
    /// A collection of layers constituting a profile through a cell.
    /// Depending on the context, the layers may be equivalent to the passes over a cell
    /// or may represent the lifts over a cell, in which case the Passes collection
    /// for an individual layer will contain the passes making up that lift.
    /// </summary>
    public IProfileLayers Layers { get; set; }

    public float CellLowestElev;
    public float CellHighestElev;
    public float CellLastElev;
    public float CellFirstElev;
    public float CellLowestCompositeElev;
    public float CellHighestCompositeElev;
    public float CellLastCompositeElev;
    public float CellFirstCompositeElev;

    public short CellCCV { get; set; }
    public short CellTargetCCV { get; set; }
    public short CellPreviousMeasuredCCV { get; set; }
    public short CellPreviousMeasuredTargetCCV { get; set; }
    public float CellCCVElev { get; set; }

    public short CellMDP  { get; set; }
    public short CellTargetMDP { get; set; }
    public float CellMDPElev { get; set; }

    public byte CellCCA { get; set; }
    public short CellTargetCCA { get; set; }
    public float CellCCAElev { get; set; }

    public float CellTopLayerThickness;
    public bool IncludesProductionData;

    public ushort TopLayerPassCount;
    public ushort TopLayerPassCountTargetRangeMin;
    public ushort TopLayerPassCountTargetRangeMax;

    public ushort CellMaxSpeed { get; set; }
    public ushort CellMinSpeed { get; set; }

    /// <summary>
    /// Passes contains the entire list of passes that all the layers in the layer collection refer to
    /// </summary>
    public FilteredMultiplePassInfo Passes { get; private set; } = new FilteredMultiplePassInfo();

    public bool[] FilteredPassFlags = new bool[0];
    public int FilteredPassCount;
    public int FilteredHalfPassCount;
    public ProfileCellAttributeExistenceFlags AttributeExistenceFlags;

    public ushort CellMaterialTemperature;
    public ushort CellMaterialTemperatureWarnMin, CellMaterialTemperatureWarnMax;
    public float CellMaterialTemperatureElev;

    /// <summary>
    /// Default no-args constructor that initialises internal state to 'null';
    /// </summary>
    public ProfileCell()
    {
      Layers = new ProfileLayers();

      CellLowestElev = Consts.NullHeight;
      CellHighestElev = Consts.NullHeight;
      CellLastElev = Consts.NullHeight;
      CellFirstElev = Consts.NullHeight;
      CellLowestCompositeElev = Consts.NullHeight;
      CellHighestCompositeElev = Consts.NullHeight;
      CellLastCompositeElev = Consts.NullHeight;
      CellFirstCompositeElev = Consts.NullHeight;
      DesignElev = Consts.NullHeight;

      CellMaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      CellMaterialTemperatureElev = Consts.NullHeight;

      AttributeExistenceFlags = ProfileCellAttributeExistenceFlags.None;

      CellMaxSpeed = 0;
      CellMinSpeed = CellPassConsts.NullMachineSpeed;

      CellPreviousMeasuredCCV = CellPassConsts.NullCCV;
      CellPreviousMeasuredTargetCCV = CellPassConsts.NullCCV;
    }

    public void SetFilteredPasses(FilteredPassData[] filteredPasses)
    {
      Passes.FilteredPassData = filteredPasses;
      Passes.PassCount = filteredPasses.Length;
    }

    /// <summary>
    /// Changes the count of passes present in the filtered set of passes held in the profile cell
    /// </summary>
    /// <param name="filteredPassCount"></param>
    public void SetFilteredPassCount(int filteredPassCount) => Passes.PassCount = filteredPassCount;

    /// <summary>
    /// Constructs a profiler layer from a given set of filtered passes and the location of the cell on the profile line
    /// </summary>
    /// <param name="filteredPassInfo"></param>
    /// <param name="oTGX"></param>
    /// <param name="oTGY"></param>
    /// <param name="station"></param>
    /// <param name="interceptLength"></param>
    /// <param name="includesProductionData"></param>
    public ProfileCell(FilteredMultiplePassInfo filteredPassInfo,
      uint oTGX, uint oTGY,
      double station, double interceptLength,
      bool includesProductionData = true) : this()
    {
      if (includesProductionData)
        AddLayer(filteredPassInfo);

      Station = station;
      InterceptLength = interceptLength;
      OTGCellX = oTGX;
      OTGCellY = oTGY;
      IncludesProductionData = includesProductionData;

      // Set the first/last/lowest/height attributes for the cell
      if (filteredPassInfo.PassCount > 0)
      {
        CellFirstElev = filteredPassInfo.FilteredPassData[0].FilteredPass.Height;
        CellLastElev = filteredPassInfo.FilteredPassData[filteredPassInfo.PassCount - 1].FilteredPass.Height;

        foreach (var Pass in filteredPassInfo.FilteredPassData)
        {
          if (CellLowestElev == Consts.NullHeight || CellLowestElev > Pass.FilteredPass.Height)
            CellLowestElev = Pass.FilteredPass.Height;
          if (CellHighestElev == Consts.NullHeight || CellHighestElev < Pass.FilteredPass.Height)
            CellHighestElev = Pass.FilteredPass.Height;
        }
      }
    }

    /// <summary>
    /// Determines if the internal layer contains no cell passes from the internal stack of cell passes.
    /// Note: This count is derived from teh start and stop index within the list of cells; layers do
    /// not separately contain the cell passes themselves.
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
      try
      {
        // ReSharper disable once UseMethodAny.2
        return Layers.Count() == 0;
      }
      catch
      {
        return true;
      }
    }

    /// <summary>
    /// Returns the height of the top most (latest in time) cell pass within the cell passes in this layer
    /// </summary>
    /// <returns></returns>
    public float TopMostHeight() => IsEmpty() ? 0 : Layers.Last().Height;

    /// <summary>
    /// Constructs a new layer from the set of cell passes contained in a multiple cell pass filtering result
    /// </summary>
    /// <param name="filteredPassValues"></param>
    public void AddLayer(FilteredMultiplePassInfo filteredPassValues)
    {
      if (Layers.Count() != 0)
      {
        Log.LogError("Cannot add a layer via FilteredPassValues if there are already layers in the cell");
        return;
      }

      if (filteredPassValues.PassCount == 0)
        return;

      IProfileLayer NewLayer = RequestNewLayer(out int LayerRecycledIndex);

      NewLayer.StartCellPassIdx = 0;
      NewLayer.EndCellPassIdx = filteredPassValues.PassCount - 1;

      NewLayer.Assign(filteredPassValues);

      AddLayer(NewLayer, LayerRecycledIndex);
    }

    /// <summary>
    /// Adds a new layer to the layers collection for this cell
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="layerRecycledIndex"></param>
    public void AddLayer(IProfileLayer layer, int layerRecycledIndex)
    {
      // ReSharper disable once UseMethodAny.2
      if (!(Layers.Count() == 0 || layer.LastLayerPassTime >= Layers.Last().LastLayerPassTime))
      {
        Log.LogError("Layer times are out of order");
        layerRecycledIndex = -1;
        return;
      }

      // If the layer has been recycled then there is no need to add it again to the layer list
      Layers.Add(layer, layerRecycledIndex);
    }

    /// <summary>
    /// Requests a new layer, which may be provision from the recycle list in the layer
    /// </summary>
    /// <param name="layerRecycledIndex"></param>
    /// <returns></returns>
    public IProfileLayer RequestNewLayer(out int layerRecycledIndex)
    {
      IProfileLayer result = Layers.GetRecycledLayer(out layerRecycledIndex);

      if (layerRecycledIndex != -1)
        result.Clear();
      else
        result = new ProfileLayer(this);

      return result;
    }

    /// <summary>
    /// Compute the first, last, lowest and highest elevations for the set of cell passes in the layer
    /// </summary>
    /// <param name="hasElevationTypeFilter"></param>
    /// <param name="elevationType"></param>
    public void SetFirstLastHighestLowestElevations(bool hasElevationTypeFilter, ElevationType elevationType)
    {
      for (int j = 0; j < Layers.Count(); j++) // from oldest to newest
      {
        if (FilteredPassCount > 0)
        {
          if ((LayerStatus.Superseded & Layers[j].Status) != 0)
            continue;

          if (Layers[j].LastPassHeight != Consts.NullHeight)
            CellLastElev = Layers[j].LastPassHeight; // keep updating to last layer
          if (CellFirstElev == Consts.NullHeight)
            CellFirstElev = Layers[j].FirstPassHeight; // record if not yet set for first value

          if (((Layers[j].MaximumPassHeight != Consts.NullHeight) && (Layers[j].MaximumPassHeight > CellHighestElev)) ||
              (CellHighestElev == Consts.NullHeight))
            CellHighestElev = Layers[j].MaximumPassHeight;
          if (((Layers[j].MinimumPassHeight != Consts.NullHeight) && (Layers[j].MinimumPassHeight < CellLowestElev)) ||
              (CellLowestElev == Consts.NullHeight))
            CellLowestElev = Layers[j].MinimumPassHeight;
        }
      }

      if (hasElevationTypeFilter)
      {
        // this means we are only interested in one pass so other results should match elev type selected
        switch (elevationType)
        {
          case ElevationType.Last:
            {
              CellLowestElev = CellLastElev;
              CellHighestElev = CellLastElev;
              CellFirstElev = CellLastElev;
              break;
            }
          case ElevationType.First:
            {
              CellLowestElev = CellFirstElev;
              CellHighestElev = CellFirstElev;
              CellLastElev = CellFirstElev;
              break;
            }
          case ElevationType.Highest:
            {
              CellLowestElev = CellHighestElev;
              CellFirstElev = CellHighestElev;
              CellLastElev = CellHighestElev;
              break;
            }
          case ElevationType.Lowest:
            {
              CellHighestElev = CellLowestElev;
              CellFirstElev = CellLowestElev;
              CellLastElev = CellLowestElev;
              break;
            }
        }
      }
    }

    /// <summary>
    /// Clears all layers in this cell
    /// </summary>
    public void ClearLayers() => Layers.Clear();

    /// <summary>
    /// Checks to see if compaction as measured by CCV/MDP or CCA has met or not the compaction metrics
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridDataType"></param>
    public void CheckLiftCompaction(ProfileLayer layer, /*todo const LiftBuildSettings :TICLiftBuildSettings; */
      GridDataType gridDataType)
    {
      // CCA tracking vars
      int Tolerance = Dummy_LiftBuildSettings.CCATolerance;
      bool TargetMeet = false;
      int ValidCCAPasses = 0;

      bool IsCCV = gridDataType == GridDataType.CCV || gridDataType == GridDataType.CCVPercent;
      bool IsMDP = gridDataType == GridDataType.MDP || gridDataType == GridDataType.MDPPercent;
      bool IsCCA = gridDataType == GridDataType.CCA || gridDataType == GridDataType.CCAPercent;

      short ATargetCCV = Dummy_LiftBuildSettings.OverrideMachineCCV
        ? Dummy_LiftBuildSettings.OverridingMachineCCV
        : CellPassConsts.NullCCV;
      short ATargetMDP = Dummy_LiftBuildSettings.OverrideMachineMDP
        ? Dummy_LiftBuildSettings.OverridingMachineMDP
        : CellPassConsts.NullMDP;

      for (int I = layer.EndCellPassIdx; I >= layer.StartCellPassIdx; I--)
      {
        if (Passes.FilteredPassData[I].FilteredPass.CCV == CellPassConsts.NullCCV &&
            Passes.FilteredPassData[I].FilteredPass.MDP == CellPassConsts.NullMDP &&
            Passes.FilteredPassData[I].FilteredPass.CCA == CellPassConsts.NullCCA)
          continue;

        if (!FilteredPassFlags[I])
          continue;

        // If gridType = icdtAll then only one type should have a non null value

        if (IsCCV || (gridDataType == GridDataType.All))
        {
          if (Passes.FilteredPassData[I].FilteredPass.CCV != CellPassConsts.NullCCV)
          {
            if (!Dummy_LiftBuildSettings.OverrideMachineCCV)
              ATargetCCV = Passes.FilteredPassData[I].TargetValues.TargetCCV;

            if (ATargetCCV == CellPassConsts.NullCCV)
              continue;

            if (Passes.FilteredPassData[I].FilteredPass.CCV < ATargetCCV * Dummy_LiftBuildSettings.CCVRange.Min / 100)
              layer.Status |= LayerStatus.Undercompacted;
            else if (Passes.FilteredPassData[I].FilteredPass.CCV >
                     ATargetCCV * Dummy_LiftBuildSettings.CCVRange.Max / 100)
              layer.Status |= LayerStatus.Overcompacted;
          }
        }

        if (IsMDP || (gridDataType == GridDataType.All))
        {
          if (Passes.FilteredPassData[I].FilteredPass.MDP != CellPassConsts.NullMDP)
          {
            if (!Dummy_LiftBuildSettings.OverrideMachineMDP)
              ATargetMDP = Passes.FilteredPassData[I].TargetValues.TargetMDP;

            if (ATargetMDP == CellPassConsts.NullMDP)
              continue;

            if (Passes.FilteredPassData[I].FilteredPass.MDP < ATargetMDP * Dummy_LiftBuildSettings.MDPRange.Min / 100)
              layer.Status |= LayerStatus.Undercompacted;
            else if (Passes.FilteredPassData[I].FilteredPass.MDP >
                     ATargetMDP * Dummy_LiftBuildSettings.MDPRange.Max / 100)
              layer.Status |= LayerStatus.Overcompacted;
          }
        }

        // For CCA it will always be for one machine we are looking at
        // Also compaction logic is different to above compaction types
        if (IsCCA || (gridDataType == GridDataType.All))
        {
          if (Passes.FilteredPassData[I].TargetValues.TargetCCA == CellPassConsts.NullCCA)
            continue;

          if (Passes.FilteredPassData[I].FilteredPass.CCA != CellPassConsts.NullCCA)
          {
            ValidCCAPasses++; // Last valid CCA pass is the most important to state
            if ((Passes.FilteredPassData[I].FilteredPass.CCA / 2) >= Passes.FilteredPassData[I].TargetValues.TargetCCA
            ) // is target meet
            {
              if (TargetMeet) // has happened before
              {
                if ((ValidCCAPasses - 1) > Tolerance)
                {
                  // They have done too much work
                  layer.Status |= LayerStatus.Overcompacted;
                  break;
                }
              }
              else
                TargetMeet = true; // Next check to see if previous pass also meet target
            }
            else
            {
              // Target not meet
              layer.Status |= TargetMeet ? LayerStatus.Complete : LayerStatus.Undercompacted;
              break;
            }
          }

          if (I == layer.StartCellPassIdx) // if no more passes to check
            layer.Status |= TargetMeet ? LayerStatus.Complete : LayerStatus.Undercompacted;

          continue; // Continue until satisfied checking for over-compacted or at end of layer
        }

        break;
      }
    }

    /// <summary>
    /// Checks and modifies the min and max speed values for the cell based on a supplied speed value from a cell pass.
    /// </summary>
    /// <param name="speed"></param>
    public void AnalyzeSpeedTargets(ushort speed)
    {
      if (CellMinSpeed > speed)
        CellMinSpeed = speed;
      if (CellMaxSpeed < speed)
        CellMaxSpeed = speed;
    }

    /// <summary>
    /// Determines if the recorded time of a given pass lies within the time range of a layer that is
    /// deemed to be superseded by another layer
    /// </summary>
    /// <param name="Pass"></param>
    /// <returns></returns>
    public bool IsInSupersededLayer(CellPass Pass)
    {
      for (int I = 0; I < Layers.Count(); I++)
      {
        IProfileLayer layer = Layers[I];
        if ((layer.Status & LayerStatus.Superseded) != 0)
        {
          if (Pass.Time >= Passes.FilteredPassData[layer.StartCellPassIdx].FilteredPass.Time &&
              Pass.Time <= layer.LastLayerPassTime)
            return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteDouble(InterceptLength);

      int count = Layers?.Count() ?? 0;
      writer.WriteInt(count);
      for (int i = 0; i < count; i++)
        Layers[i].ToBinary(writer);

      writer.WriteInt((int)OTGCellX);
      writer.WriteInt((int)OTGCellY);

      writer.WriteFloat(CellLowestElev);
      writer.WriteFloat(CellHighestElev);
      writer.WriteFloat(CellLastElev);
      writer.WriteFloat(CellFirstElev);
      writer.WriteFloat(CellLowestCompositeElev);
      writer.WriteFloat(CellHighestCompositeElev);
      writer.WriteFloat(CellLastCompositeElev);
      writer.WriteFloat(CellFirstCompositeElev);
      writer.WriteFloat(DesignElev);

      writer.WriteShort(CellCCV);
      writer.WriteShort(CellTargetCCV);
      writer.WriteShort(CellPreviousMeasuredCCV);
      writer.WriteShort(CellPreviousMeasuredTargetCCV);

      writer.WriteFloat(CellCCVElev);

      writer.WriteShort(CellMDP);
      writer.WriteShort(CellTargetMDP);
      writer.WriteFloat(CellMDPElev);

      writer.WriteByte(CellCCA);
      writer.WriteShort(CellTargetCCA);
      writer.WriteFloat(CellCCAElev);

      writer.WriteFloat(CellTopLayerThickness);
      writer.WriteBoolean(IncludesProductionData);

      writer.WriteInt(TopLayerPassCount);
      writer.WriteInt(TopLayerPassCountTargetRangeMin);
      writer.WriteInt(TopLayerPassCountTargetRangeMax);

      writer.WriteInt(CellMaxSpeed);
      writer.WriteInt(CellMinSpeed);

      Passes.ToBinary(writer);

      writer.WriteBooleanArray(FilteredPassFlags);
      writer.WriteInt(FilteredPassCount);
      writer.WriteInt(FilteredHalfPassCount);
      writer.WriteInt((int)AttributeExistenceFlags);

      writer.WriteInt(CellMaterialTemperature);
      writer.WriteInt(CellMaterialTemperatureWarnMin);
      writer.WriteInt(CellMaterialTemperatureWarnMax);

      writer.WriteFloat(CellMaterialTemperatureElev);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      InterceptLength = reader.ReadDouble();

      Layers = new ProfileLayers();      
      var numberOfLayers = reader.ReadInt();

      for (var i = 1; i <= numberOfLayers; i++)
      {
        var layer = new ProfileLayer(this);
        layer.FromBinary(reader);

        Layers.Add(layer, -1);
      }

      OTGCellX = (uint)reader.ReadInt();
      OTGCellY = (uint)reader.ReadInt();

      CellLowestElev = reader.ReadFloat();
      CellHighestElev = reader.ReadFloat();
      CellLastElev = reader.ReadFloat();
      CellFirstElev = reader.ReadFloat();
      CellLowestCompositeElev = reader.ReadFloat();
      CellHighestCompositeElev = reader.ReadFloat();
      CellLastCompositeElev = reader.ReadFloat();
      CellFirstCompositeElev = reader.ReadFloat();
      DesignElev = reader.ReadFloat();

      CellCCV = reader.ReadShort();
      CellTargetCCV = reader.ReadShort();
      CellPreviousMeasuredCCV = reader.ReadShort();
      CellPreviousMeasuredTargetCCV = reader.ReadShort();

      CellCCVElev = reader.ReadFloat();

      CellMDP = reader.ReadShort();
      CellTargetMDP = reader.ReadShort();
      CellMDPElev = reader.ReadFloat();

      CellCCA = reader.ReadByte();
      CellTargetCCA = reader.ReadShort();
      CellCCAElev = reader.ReadFloat();

      CellTopLayerThickness = reader.ReadFloat();
      IncludesProductionData = reader.ReadBoolean();

      TopLayerPassCount = (ushort)reader.ReadInt();
      TopLayerPassCountTargetRangeMin = (ushort)reader.ReadInt();
      TopLayerPassCountTargetRangeMax = (ushort)reader.ReadInt();

      CellMaxSpeed = (ushort)reader.ReadInt();
      CellMinSpeed = (ushort)reader.ReadInt();

      (Passes ?? (Passes = new FilteredMultiplePassInfo())).FromBinary(reader);

      FilteredPassFlags = reader.ReadBooleanArray();
      FilteredPassCount = reader.ReadInt();
      FilteredHalfPassCount = reader.ReadInt();
      AttributeExistenceFlags = (ProfileCellAttributeExistenceFlags)reader.ReadInt();

      CellMaterialTemperature = (ushort)reader.ReadInt();
      CellMaterialTemperatureWarnMin = (ushort)reader.ReadInt();
      CellMaterialTemperatureWarnMax = (ushort)reader.ReadInt();

      CellMaterialTemperatureElev = reader.ReadFloat();
    }

    /// <summary>
    /// Use this function to loop through all passes regardless if half pass or not
    /// if no half passes present then this is a one for one relationship
    /// </summary>
    /// <param name="includeSupersededLayers"></param>
    /// <returns></returns>
    public int TotalNumberOfHalfPasses(bool includeSupersededLayers)
    {
      var Result = 0;

      var layerCount = Layers.Count();
      for (int i = 0; i < layerCount; i++)
      {
        if (!includeSupersededLayers && (LayerStatus.Superseded & Layers[i].Status) != 0)
          continue;

        Result += Layers[i].PassCount; // count all passes even if half
      }

      return Result;
    }


    /// <summary>
    /// Use this function to get a count of whole passes. It takes two half passes to make one whole pass
    /// </summary>
    /// <param name="includeSupersededLayers"></param>
    /// <returns></returns>
    public int TotalNumberOfWholePasses(bool includeSupersededLayers)
    {
      var Result = 0;

      var layerCount = Layers.Count();
      for (int i = 0; i < layerCount; i++)
      {
        if (!includeSupersededLayers && (LayerStatus.Superseded & Layers[i].Status) != 0)
          continue;

        Result += FilteredHalfPassCount > 0 ? FilteredHalfPassCount / 2 : Layers[i].PassCount;
      }

      return Result;
    }

    //procedure ReadFromStream(const Stream : TStream; APassesPackager : TICFilteredMultiplePassInfoPackager);
    //procedure WriteToStream(const Stream : TStream; const WriteCellPassesAndLayers : Boolean; APassesPackager : TICFilteredMultiplePassInfoPackager);

    /*
      TICProfileCell = {$IFDEF PRISM} public {$ENDIF} class(TObject)
      public
        // Initialises a profile cell by assigning the passes over a cell
        // to the layers constituting a profile
        procedure Assign(const FilteredPassInfo: TICFilteredMultiplePassInfo;
                         const AOTGX, AOTGY: Integer;
                         const AStation, AInterceptLength: Double); Overload;
        procedure Assign(const Source : TICProfileCell); Overload;

        procedure GetNotSupersededLayers(NotSupersededLayers :TICProfileLayers);

        function  GetNearestValdLayerHeight(const LayerIndex :Integer) :TICCellHeight;

        function  MaxNumberOfPasses  (const IncludeSupersededLayers :Boolean = False) :Integer;

        function  MaxCCVValue              :TICCCVValue;
        function  MaxTargetCCVValue        :TICCCVValue;
        function  MaxMDPValue              :TICMDPValue;
        function  MaxTargetMDPValue        :TICMDPValue;
        function  MaxCCAValue              :TICCCAValue;
        function  MaxTargetCCAValue        :TICCCAMinPassesValue;

        function  MinLayerThickness        :TICCellHeight;
        function  MaxLayerThickness        :TICCellHeight;
        function  MinNonZeroLayerThickness :TICCellHeight;
        function  MaxLayerHeight           :TICCellHeight;
        function  MinLayerHeight           :TICCellHeight; // This is the lowest height of any of the layers minus it's thickness
        function  MaxLayerElevation        :TICCellHeight;
        function  MinLayerElevation        :TICCellHeight;

        function  TopPassTargetCCVByCompactor      (const Layer :TICProfileLayer) :TICCCVValue;
        function  TopPassTargetMDPByCompactor      (const Layer :TICProfileLayer) :TICMDPValue;
        function  TopPassTargetCCAByCompactor      (const Layer :TICProfileLayer) :TICCCAMinPassesValue;
        function  TopPassTargetThicknessByCompactor(const Layer :TICProfileLayer) :TICLiftThickness;

        procedure NormalizeLayersMaxThickness(const FirstPassThickness :TICCellHeight);

        Function  NullCellCoordinate : Boolean; Inline;

        procedure SetLayersStatus(const LiftBuildSettings :TICLiftBuildSettings; GridDataType :TICGridDataType);

        Function ToString : String; Override;
      end;
     */
  }
}
