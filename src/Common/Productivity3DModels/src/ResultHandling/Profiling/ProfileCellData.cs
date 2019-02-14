namespace VSS.Productivity3D.Models.ResultHandling.Profiling
{
  public class ProfileCellData
  {
    public double Station { get; set; }
    public double InterceptLength { get; set; }
    public float FirstPassHeight { get; set; }
    public float LastPassHeight { get; set; }
    public float LowestPassHeight { get; set; }
    public float HighestPassHeight { get; set; }
    public float CompositeFirstPassHeight { get; set; }
    public float CompositeLastPassHeight { get; set; }
    public float CompositeLowestPassHeight { get; set; }
    public float CompositeHighestPassHeight { get; set; }
    public float DesignHeight { get; set; }
    public short CCV { get; set; }
    public short TargetCCV { get; set; }
    public float CCVElev { get; set; }
    public short PrevCCV { get; set; }
    public short PrevTargetCCV { get; set; }
    public short MDP { get; set; }
    public short TargetMDP { get; set; }
    public float MDPElev { get; set; }
    public ushort MaterialTemperature { get; set; }
    public ushort MaterialTemperatureWarnMin { get; set; }
    public ushort MaterialTemperatureWarnMax { get; set; }
    public float MaterialTemperatureElev { get; set; }
    public float TopLayerThickness { get; set; }
    public int TopLayerPassCount { get; set; }
    public ushort TopLayerPassCountTargetRangeMin { get; set; }
    public ushort TopLayerPassCountTargetRangeMax { get; set; }
    public ushort CellMinSpeed { get; set; }
    public ushort CellMaxSpeed { get; set; }
    
    // Default public constructor.
    public ProfileCellData()
    {
    }

    public ProfileCellData(
      double station,
      double interceptLength,
      float firstPassHeight,
      float lastPassHeight,
      float lowestPassHeight,
      float highestPassHeight,
      float compositeFirstPassHeight,
      float compositeLastPassHeight,
      float compositeLowestPassHeight,
      float compositeHighestPassHeight,
      float designHeight,
      short ccv,
      short targetCCV,
      float ccvElev,
      short prevCCV,
      short prevTargetCCV,
      short mdp,
      short targetMDP,
      float mdpElev,
      ushort materialTemperature,
      ushort materialTemperatureWarnMin,
      ushort materialTemperatureWarnMax,
      float materialTemperatureElev,
      float topLayerThickness,
      int topLayerPassCount,
      ushort topLayerPassCountTargetRangeMin,
      ushort topLayerPassCountTargetRangeMax,
      ushort cellMinSpeed,
      ushort cellMaxSpeed
      )
    {
      Station = station;
      InterceptLength = interceptLength;
      FirstPassHeight = firstPassHeight;
      LastPassHeight = lastPassHeight;
      LowestPassHeight = lowestPassHeight;
      HighestPassHeight = highestPassHeight;
      CompositeFirstPassHeight = compositeFirstPassHeight;
      CompositeHighestPassHeight = compositeHighestPassHeight;
      CompositeLastPassHeight = compositeLastPassHeight;
      CompositeLowestPassHeight = compositeLowestPassHeight;
      DesignHeight = designHeight;
      CCV = ccv;
      TargetCCV = targetCCV;
      CCVElev = ccvElev;
      PrevCCV = prevCCV;
      PrevTargetCCV = prevTargetCCV;
      MDP = mdp;
      TargetMDP = targetMDP;
      MDPElev = mdpElev;
      MaterialTemperature = materialTemperature;
      MaterialTemperatureWarnMin = materialTemperatureWarnMin;
      MaterialTemperatureWarnMax = materialTemperatureWarnMax;
      MaterialTemperatureElev = materialTemperatureElev;
      TopLayerThickness = topLayerThickness;
      TopLayerPassCount = topLayerPassCount;
      TopLayerPassCountTargetRangeMin = topLayerPassCountTargetRangeMin;
      TopLayerPassCountTargetRangeMax = topLayerPassCountTargetRangeMax;
      CellMinSpeed = cellMinSpeed;
      CellMaxSpeed = cellMaxSpeed;
    }
  }
}
