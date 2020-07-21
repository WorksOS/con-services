using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.ResultHandling.Profiling
{
  public class SummaryVolumesProfileCell
  {
    [JsonProperty]
    public double Station { get; private set; }
    [JsonProperty]
    public double InterceptLength { get; private set; }
    [JsonProperty]
    public int OTGCellX { get; private set; }
    [JsonProperty]
    public int OTGCellY { get; private set; }
    [JsonProperty]
    public float DesignElev { get; private set; }
    [JsonProperty]
    public float LastCellPassElevation1 { get; private set; }
    [JsonProperty]
    public float LastCellPassElevation2 { get; private set; }

    // Default public constructor.
    public SummaryVolumesProfileCell()
    {

    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="station"></param>
    /// <param name="interceptLength"></param>
    /// <param name="otgCellX"></param>
    /// <param name="otgCellY"></param>
    /// <param name="designElev"></param>
    /// <param name="lastCellPassElevation1"></param>
    /// <param name="lastCellPassElevation2"></param>
    public SummaryVolumesProfileCell(
      double station, 
      double interceptLength, 
      int otgCellX, 
      int otgCellY, 
      float designElev, 
      float lastCellPassElevation1, 
      float lastCellPassElevation2)
    {
      Station = station;
      InterceptLength = interceptLength;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      DesignElev = designElev;
      LastCellPassElevation1 = lastCellPassElevation1;
      LastCellPassElevation2 = lastCellPassElevation2;
    }
  }
}
