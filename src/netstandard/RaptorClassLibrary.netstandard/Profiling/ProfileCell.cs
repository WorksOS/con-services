using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Filters;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// ProfileCell is a package of information relating to one cell in a profile drawn across IC data
  /// </summary>
  public class ProfileCell
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileCell>();

    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    public double Station;

    /// <summary>
    /// The real-world length of that part of the profile line which crosses the underlying cell;
    /// used to determine the width of the profile column as displayed in the client GUI
    /// </summary>
    public double InterceptLength;

    /// <summary>
    /// A collection of layers constituting a profile through a cell.
    /// Depending on the context, the layers may be equivalent to the passes over a cell
    /// or may represent the lifts over a cell, in which case the Passes collection
    /// for an individual layer will contain the passes making up that lift.
    /// </summary>
    public ProfileLayers Layers;

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public int OTGCellX;

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public int OTGCellY;

    public float CellLowestElev;
    public float CellHighestElev;
    public float CellLastElev;
    public float CellFirstElev;
    public float CellLowestCompositeElev;
    public float CellHighestCompositeElev;
    public float CellLastCompositeElev;
    public float CellFirstCompositeElev;
    public float DesignElev;

    public short CellCCV, CellTargetCCV, CellPreviousMeasuredCCV, CellPreviousMeasuredTargetCCV;
    public float CellCCVElev;

    public short CellMDP, CellTargetMDP;
    public float CellMDPElev;

    public byte CellCCA;
    public short CellTargetCCA;
    public float CellCCAElev;

    public float CellTopLayerThickness;
    public bool IncludesProductionData;

    public ushort TopLayerPassCount;
    public ushort TopLayerPassCountTargetRangeMin;
    public ushort TopLayerPassCountTargetRangeMax;

    public ushort MaxSpeed;
    public ushort MinSpeed;

    // public

    /// <summary>
    // Passes contains the entire list of passes that all the layers in the layer collection refer to
    /// </summary>
    public FilteredMultiplePassInfo Passes;

    public bool[] FilteredPassFlags;
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

      CellMaterialTemperature = CellPass.NullMaterialTemperatureValue;
      CellMaterialTemperatureElev = Consts.NullHeight;

      AttributeExistenceFlags = ProfileCellAttributeExistenceFlags.None;

      MaxSpeed = 0;
      MinSpeed = CellPass.NullMachineSpeed;

      CellPreviousMeasuredCCV = CellPass.NullCCV;
      CellPreviousMeasuredTargetCCV = CellPass.NullCCV;
    }

    /// <summary>
    /// Constructs a profiler from a given set of filtered passes and the location of the cell on the profile line
    /// </summary>
    /// <param name="filteredPassInfo"></param>
    /// <param name="oTGX"></param>
    /// <param name="oTGY"></param>
    /// <param name="station"></param>
    /// <param name="interceptLength"></param>
    /// <param name="includesProductionData"></param>
    public ProfileCell(FilteredMultiplePassInfo filteredPassInfo,
      int oTGX, int oTGY,
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

    public bool IsEmpty()
    {
      try
      {
        return Layers.Count() == 0;
      }
      catch
      {
        return true;
      }
    }

    public float TopMostHeight() => IsEmpty() ? 0 : Layers.Last().Height;

    public void AddLayer(FilteredMultiplePassInfo filteredPassValues)
    {
      if (Layers.Count() != 0)
      {
        Log.LogError("Cannot add a layer via FilteredPassValues if there are already layers in the cell");
        return;
      }

      if (filteredPassValues.PassCount == 0)
        return;

      ProfileLayer NewLayer = RequestNewLayer(out int LayerRecycledIndex);

      NewLayer.StartCellPassIdx = 0;
      NewLayer.EndCellPassIdx = filteredPassValues.PassCount - 1;

      NewLayer.Assign(filteredPassValues);

      AddLayer(NewLayer, LayerRecycledIndex);
    }

    public void AddLayer(ProfileLayer layer, int layerRecycledIndex)
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


    public ProfileLayer RequestNewLayer(out int layerRecycledIndex)
    {
      ProfileLayer result = Layers.GetRecycledLayer(out layerRecycledIndex);

      if (layerRecycledIndex != -1)
        result.Clear();
      else
        result = new ProfileLayer(this);

      return result;
    }


  }

  /*
    TICProfileCell = {$IFDEF PRISM} public {$ENDIF} class(TObject)
    public
      // Initialises a profile cell by assigning the passes over a cell
      // to the layers constituting a profile
      procedure Assign(const FilteredPassInfo: TICFilteredMultiplePassInfo;
                       const AOTGX, AOTGY: Integer;
                       const AStation, AInterceptLength: Double); Overload;
      procedure Assign(const Source : TICProfileCell); Overload;

      function RequestNewLayer(var LayerRecycledIndex : Integer) : TICProfileLayer;

      procedure ReadFromStream(const Stream : TStream;
                               APassesPackager : TICFilteredMultiplePassInfoPackager);
      procedure WriteToStream(const Stream : TStream;
                              const WriteCellPassesAndLayers : Boolean;
                              APassesPackager : TICFilteredMultiplePassInfoPackager);

      procedure ClearLayers;
      procedure GetNotSupersededLayers(NotSupersededLayers :TICProfileLayers);

      function  GetNearestValidLayerHeight(const LayerIndex :Integer) :TICCellHeight;

      function  MaxNumberOfPasses  (const IncludeSupersededLayers :Boolean = False) :Integer;
      function  TotalNumberOfHalfPasses(const IncludeSupersededLayers :Boolean = False) :Integer;
      function  TotalNumberOfWholePasses(const IncludeSupersededLayers :Boolean = False) :Integer;

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
      procedure CheckLiftCompaction     (const Layer :TICProfileLayer; const LiftBuildSettings :TICLiftBuildSettings; GridDataType :TICGridDataType);

      procedure NormalizeLayersMaxThickness(const FirstPassThickness :TICCellHeight);

      function  IsInSupersededLayer(const Pass :TICCellPassValue) :Boolean;

      Function  NullCellCoordinate : Boolean; Inline;

      procedure SetLayersStatus(const LiftBuildSettings :TICLiftBuildSettings; GridDataType :TICGridDataType);

      procedure SetFirstLastHighestLowestElevations(const HasElevationTypeFilter : Boolean; const ElevationType : TICElevationType);

      Function ToString : String; Override;

      Procedure AnalyzeSpeedTargets( const Speed : TICMachineSpeed ); inLine;
    end;
   */
  
}
